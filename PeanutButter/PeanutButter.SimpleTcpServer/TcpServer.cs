using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
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
        public int RandomPortMin { get; set; }
        public int RandomPortMax { get; set; }
        public int Port { get; protected set; }
        private TcpListener _listener;
        private Task _task;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _portExplicitlySpecified;
        private object _lock = new object();
        private Random _random = new Random(DateTime.Now.Millisecond);

        protected TcpServer()
        {
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

        protected abstract IProcessor CreateProcessor(TcpClient client);
        public void Start() 
        {
            lock (_lock)
            {
                DoStop();
                AttemptBind();
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

        private void AttemptBind()
        {
            _listener = new TcpListener(IPAddress.Any, Port);
            var attempts = 0;
            while (true)
            {
                try
                {
                    _listener.Start();
                    break;
                }
                catch
                {
                    if (_portExplicitlySpecified)
                        throw new Exception("Can't listen on specified port '" + Port + "': probably already in use?");
                    if (attempts++ > 150)
                        throw new Exception("Can't find a port to listen on ):");
                    Thread.Sleep(10); // back off the bind attempts briefly
                    Port = FindOpenRandomPort();
                }
            }
        }

        private static void LogException(Exception ex)
        {
            Debug.WriteLine("Exception occurred whilst accepting client: " + ex.Message);
            Debug.WriteLine("Stack trace follows");
            Debug.WriteLine(ex.StackTrace);
        }

        private void AcceptClient()
        {
            var listener = _listener;
            if (listener == null)
                return;
            var s = listener.AcceptTcpClient();
            var processor = CreateProcessor(s);
            var thread = new Thread(new ThreadStart(processor.ProcessRequest));
            thread.Start();
            Thread.Sleep(0);
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

        public void Dispose()
        {
            Stop();
        }

        protected int FindOpenRandomPort()
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            var tryThis = NextRandomPort();
            var seekingPort = true;
            while (seekingPort)
            {
                try
                {
                    var listener = new TcpListener(IPAddress.Any, tryThis);
                    listener.Start();
                    listener.Stop();
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
            var minPort = RandomPortMin;
            var maxPort = RandomPortMax;
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
