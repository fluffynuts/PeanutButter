using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.SimpleTcpServer
{
    /// <summary>
    /// Interface to be implemented by a service which processes requests
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        /// Process the request
        /// </summary>
        void ProcessRequest();
    }

    /// <summary>
    /// Provides the base TCP server upon which more complex
    /// TCP-based servers can be built.
    /// </summary>
    public abstract class TcpServer : IDisposable
    {
        /// <summary>
        /// Maximum time, in milliseconds, to wait on the
        /// listener task when shutting down
        /// </summary>
        public int MaxShutDownTime { get; set; } = 1000;

        /// <summary>
        /// Whether or not to log random port discovery processes
        /// </summary>
        public bool LogRandomPortDiscovery { get; set; }

        /// <summary>
        /// Action to employ when logging (defaults to logging to the console)
        /// </summary>
        public Action<string> LogAction { get; set; } = Console.WriteLine;

        // ReSharper disable once MemberCanBePrivate.Global

        /// <summary>
        /// Port which this server has bound to
        /// </summary>
        public int Port { get; protected set; }

        /// <summary>
        /// Flag exposing listening state
        /// </summary>
        public bool IsListening => _listener != null;

        private TcpListener _listener;
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly bool _portExplicitlySpecified;
        private readonly object _lock = new object();
        private readonly Random _random = new Random(DateTime.Now.Millisecond);
        private readonly int _randomPortMin;
        private readonly int _randomPortMax;


        /// <summary>
        /// Construct the server with a random port within the provided range
        /// </summary>
        /// <param name="minPort"></param>
        /// <param name="maxPort"></param>
        protected TcpServer(int minPort = 5000, int maxPort = 32000)
        {
            _randomPortMin = minPort;
            _randomPortMax = maxPort;
            Port = FindOpenRandomPort();
            Init();
        }

        /// <summary>
        /// Construct the server with the explicitly-provided port
        /// </summary>
        /// <param name="port"></param>
        protected TcpServer(int port)
        {
            _portExplicitlySpecified = true;
            Port = port;
            Init();
        }

        /// <summary>
        /// Override in derived classes: this initializes the server
        /// system
        /// </summary>
        protected abstract void Init();

        /// <summary>
        /// Provides a convenience logging mechanism which outputs via
        /// the established LogAction
        /// </summary>
        /// <param name="message"></param>
        /// <param name="parameters"></param>
        protected void Log(string message, params object[] parameters)
        {
            var logAction = LogAction;
            if (logAction == null)
            {
                return;
            }

            try
            {
                logAction(string.Format(message, parameters));
            }
            catch
            {
                /* intentionally left blank */
            }
        }

        /// <summary>
        /// Create a processor for a particular TCP client
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected abstract IProcessor CreateProcessorFor(TcpClient client);

        /// <summary>
        /// Start the server
        /// </summary>
        public void Start()
        {
            lock (_lock)
            {
                DoStop();
                AttemptToBind();
                ListenForClients();
            }
        }

        private void ListenForClients()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;
            _cancellationTokenSource = cancellationTokenSource;
            _task = Task.Run(() =>
            {
                if (token.IsCancellationRequested) return;
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        AcceptClientAsync();
                    }
                    catch (Exception ex)
                    {
                        if (!_cancellationTokenSource.IsCancellationRequested)
                        {
                            LogException(ex);
                        }
                    }
                }
            }, token);
        }

        private void AttemptToBind()
        {
            _listener = new TcpListener(IPAddress.Any, Port);
            var attempts = 0;
            while (true)
            {
                try
                {
                    Log("Attempting to listen on port {0}; overall attempt {1}", Port, attempts);
                    _listener.Start();
                    Log(" --> success!");
                    break;
                }
                catch
                {
                    Log(" --> failed ):");
                    if (_portExplicitlySpecified)
                    {
                        throw new PortUnavailableException(Port);
                    }

                    if (attempts++ > 150)
                    {
                        throw new UnableToFindAvailablePortException();
                    }

                    Thread.Sleep(10); // back off the bind attempts briefly
                    Port = FindOpenRandomPort();
                }
            }
        }

        private void LogException(Exception ex)
        {
            Log("Exception occurred whilst accepting client: " + ex.Message);
            Log("Stack trace follows");
            Log(ex.StackTrace);
        }

        private void AcceptClientAsync()
        {
            var listener = _listener;
            if (listener == null)
            {
                return;
            }

            var state = new AsyncAcceptState(this, listener);
            listener.BeginAcceptTcpClient(TcpClientAcceptHandler, state);
            state.ResetEvent.Wait();
        }

        private void TcpClientAcceptHandler(IAsyncResult ar)
        {
            var state = ar.AsyncState as AsyncAcceptState;
            if (state == null)
            {
                throw new InvalidOperationException(
                    $"Should have received AsyncAcceptState object"
                );
            }

            try
            {
                if (!state.TcpServer.IsListening)
                {
                    return;
                }

                var client = state.Listener.EndAcceptTcpClient(ar);
                var clientInfo = client.Client.RemoteEndPoint.ToString();
                Log($"Accepting incoming client request from {clientInfo}");
                var processor = CreateProcessorFor(client);
                Log($"Processing request for {clientInfo}");
                processor.ProcessRequest();
            }
            finally
            {
                state.ResetEvent.Set();
            }
        }

        private class AsyncAcceptState
        {
            public TcpServer TcpServer { get; }
            public TcpListener Listener { get; }
            public ManualResetEventSlim ResetEvent { get; }

            public AsyncAcceptState(
                TcpServer tcpServer,
                TcpListener listener)
            {
                TcpServer = tcpServer;
                Listener = listener;
                ResetEvent = new ManualResetEventSlim();
            }
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                DoStop();
            }
        }

        private void DoStop()
        {
            try
            {
                var listener = _listener;
                _listener = null;
                if (listener == null)
                {
                    return;
                }

                _cancellationTokenSource.Cancel();
                listener.Stop();
                try
                {
                    if (!_task.Wait(MaxShutDownTime))
                    {
                        Debug.WriteLine(
                            $"TcpServer did not shut down gracefully within ${MaxShutDownTime}ms"
                        );
                    }
                }
                catch
                {
                    /* we can end up in here if the task is cancelled really early */
                }

                _task = null;
                _cancellationTokenSource = null;
            }
            catch (Exception ex)
            {
                Log($"Internal DoStop() fails: {ex.Message}");
            }
        }

        /// <summary>
        /// Disposes the server (stops it if it is running)
        /// </summary>
        public virtual void Dispose()
        {
            Stop();
        }

        /// <summary>
        /// Attempts to find a random port to bind to
        /// </summary>
        /// <returns></returns>
        protected int FindOpenRandomPort()
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            var tryThis = NextRandomPort();
            var seekingPort = true;
            Action<string> log = s =>
            {
                if (LogRandomPortDiscovery) Log(s);
            };
            while (seekingPort)
            {
                try
                {
                    log($"Attempting to connect to random port {tryThis} on localhost");
                    using (var client = new TcpClient())
                    {
                        client.Connect(new IPEndPoint(IPAddress.Loopback, tryThis));
                    }

                    Thread.Sleep(rnd.Next(1, 50));
                    tryThis = NextRandomPort();
                }
                catch
                {
                    log($"HUZZAH! We have a port, squire! ({tryThis})");
                    seekingPort = false;
                }
            }

            return tryThis;
        }

        /// <summary>
        /// Guesses the next random port to attempt to bind to
        /// </summary>
        /// <returns></returns>
        protected virtual int NextRandomPort()
        {
            var minPort = _randomPortMin;
            var maxPort = _randomPortMax;
            if (minPort > maxPort)
            {
                var swap = minPort;
                minPort = maxPort;
                maxPort = swap;
            }

            return _random.Next(minPort, maxPort);
        }
    }
}