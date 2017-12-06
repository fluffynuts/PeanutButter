using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable VirtualMemberCallInConstructor
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.SimpleTcpServer
{
    public interface IProcessor
    {
        void ProcessRequest();
    }

    public abstract class TcpServer : IDisposable
    {
        public bool LogRandomPortDiscovery { get; set; }
        public Action<string> LogAction { get; set; } = Console.WriteLine;
        // ReSharper disable once MemberCanBePrivate.Global
        public int Port { get; protected set; }

        private TcpListener _listener;
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly bool _portExplicitlySpecified;
        private readonly object _lock = new object();
        private readonly Random _random = new Random(DateTime.Now.Millisecond);
        private readonly int _randomPortMin;
        private readonly int _randomPortMax;

        protected TcpServer(int minPort = 5000, int maxPort = 32000)
        {
            _randomPortMin = minPort;
            _randomPortMax = maxPort;
            Port = FindOpenRandomPort();
            Init();
        }

        protected TcpServer(int port)
        {
            _portExplicitlySpecified = true;
            Port = port;
            Init();
        }
        protected abstract void Init();

        protected void Log(string message, params object[] parameters)
        {
            var logAction = LogAction;
            if (logAction == null)
                return;
            try
            {
                logAction(string.Format(message, parameters));
            }
            catch
            {
                /* intentionally left blank */
            }
        }

        protected abstract IProcessor CreateProcessorFor(TcpClient client);
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
                        AcceptClient();
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
                        throw new PortUnavailableException(Port);
                    if (attempts++ > 150)
                        throw new UnableToFindAvailablePortException();
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

        private void AcceptClient()
        {
            var listener = _listener;
            if (listener == null)
                return;
            var s = listener.AcceptTcpClient();
            var clientInfo = s.Client.RemoteEndPoint.ToString();
            Log("Accepting incoming client request from {0}", clientInfo);
            var processor = CreateProcessorFor(s);
            Log("Spawning processor in background task...");
            Task.Run(() =>
            {
                Log("Processing request for {0}", clientInfo);
                processor.ProcessRequest();
            });
        }

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
                if (_listener != null)
                {
                    _cancellationTokenSource.Cancel();
                    _listener.Stop();
                    try {
                        _task.Wait();
                    } catch { /* we can end up in here if the task is cancelled really early */}

                    _listener = null;
                    _task = null;
                    _cancellationTokenSource = null;
                }
            }
            catch (Exception ex)
            {
                Log("Internal DoStop() fails: {0}", ex.Message);
            }
        }

        public void Dispose()
        {
            Stop();
        }

        protected int FindOpenRandomPort()
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            var tryThis = NextRandomPort();
            var seekingPort = true;
            Action<string> log = s => { if (LogRandomPortDiscovery) Log(s); };
            while (seekingPort)
            {
                try
                {
                    log($"Attempting to bind to random port {tryThis} on any available IP address");
                    var listener = new TcpListener(IPAddress.Any, tryThis);
                    log("Attempt to listen...");
                    listener.Start();
                    log("Attempt to stop listening...");
                    listener.Stop();
                    log($"HUZZAH! We have a port, squire! ({tryThis})");
                    seekingPort = false;
                }
                catch
                {
                    Thread.Sleep(rnd.Next(1, 50));
                    tryThis = NextRandomPort();
                }
            }
            return tryThis;
        }

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
