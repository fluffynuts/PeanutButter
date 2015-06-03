using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;

namespace PeanutButter.SimpleTcpServer
{
    public interface IProcessor
    {
        void ProcessRequest();
    }

    public abstract class TcpServer : IDisposable
    {
        public Action<string> LogAction { get; set; }
        public int Port { get; protected set; }
        TcpListener _listener;
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly bool _portExplicitlySpecified;

        protected TcpServer()
        {
            Port = FindOpenRandomPort();
            Init();
        }

        protected TcpServer(int port)
        {
            _portExplicitlySpecified = true;
            this.Port = port;
            Init();
        }

        protected void Log(string message, params object[] parameters)
        {
            var logAction = LogAction;
            if (logAction == null)
                return;
            try
            {
                logAction(string.Format(message, parameters));
            }
            catch { }
        }

        protected abstract void Init();

        protected abstract IProcessor CreateProcessorFor(TcpClient client);
        public void Start() 
        {
            lock (this)
            {
                DoStop();
                _cancellationTokenSource = new CancellationTokenSource();
                var cancellationTokenSource = _cancellationTokenSource;
                _listener = new TcpListener(IPAddress.Any, Port);
                var attempts = 0;
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    if (AttemptToBind(ref attempts)) break;
                }
                _task = Task.Run(() =>
                                              {
                                                  if (cancellationTokenSource.IsCancellationRequested) return;
                                                  while (!cancellationTokenSource.IsCancellationRequested) 
                                                  {
                                                      AcceptClientRequests(cancellationTokenSource);
                                                  }
                                              }, cancellationTokenSource.Token);
            }
        }

        private void AcceptClientRequests(CancellationTokenSource cancellationTokenSource)
        {
            try
            {
                AcceptClient();
            }
            catch (Exception ex)
            {
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    LogException(ex);
                }
            }
        }

        private bool AttemptToBind(ref int attempts)
        {
            try
            {
                Log("Attempting to listen on port {0}; overall attempt {1}", Port, attempts);
                _listener.Start();
                Log(" --> success!");
                return true;
            }
            catch
            {
                Log(" --> failed ):");
                if (_portExplicitlySpecified)
                    throw new Exception("Can't listen on specified port '" + Port + "': probably already in use?");
                if (attempts++ > 150)
                    throw new Exception("Can't find a port to listen on ):");
                Port = FindOpenRandomPort();
            }
            return false;
        }

        private static void LogException(Exception ex)
        {
            Debug.WriteLine("Exception occurred whilst accepting client: " + ex.Message);
            Debug.WriteLine("Stack trace follows");
            Debug.WriteLine(ex.StackTrace);
        }

        private void AcceptClient()
        {
            var s = _listener.AcceptTcpClient();
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
            lock (this)
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
                    _task.Wait();
                    _listener = null;
                    _task = null;
                    _cancellationTokenSource = null;
                }
            }
            catch (Exception ex)
            {
                Log("Internal DoStop fails: {0}", ex.Message);
            }
        }

        public void Dispose()
        {
            Stop();
        }

        protected int FindOpenRandomPort()
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            var tryThis = rnd.Next(5000, 50000);
            var seekingPort = true;
            while (seekingPort)
            {
                try
                {
                    Log("Attempting to bind to random port {0} on any available IP address", tryThis);
                    var listener = new TcpListener(IPAddress.Any, tryThis);
                    Log("Attempt to listen...");
                    listener.Start();
                    Log("Attempt to stop listening...");
                    listener.Stop();
                    Log("HUZZAH! We have a port, squire! ({0})", tryThis);
                    seekingPort = false;
                }
                catch
                {
                    Thread.Sleep(rnd.Next(1, 50));
                }
            }
            return tryThis;
        }
    }
}