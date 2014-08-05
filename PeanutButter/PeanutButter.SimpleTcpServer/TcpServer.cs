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
        public int Port { get; protected set; }
        TcpListener _listener;
        protected Task _task;
        protected CancellationTokenSource _cancellationTokenSource;
        private bool _portExplicitlySpecified;

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
        protected abstract void Init();

        protected abstract IProcessor CreateProcessor(TcpClient client);
        public void Start() 
        {
            lock (this)
            {
                DoStop();
                _cancellationTokenSource = new CancellationTokenSource();
                _task = Task.Factory.StartNew((token) =>
                                              {
                                                  if (_cancellationTokenSource.IsCancellationRequested) return;
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
                                                          if (attempts++ > 50)
                                                              throw new Exception("Can't find a port to listen on ):");
                                                          Port = FindOpenRandomPort();
                                                      }
                                                  }
                                                  while (!_cancellationTokenSource.IsCancellationRequested) {
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
                                              }, _cancellationTokenSource.Token);
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
            var s = _listener.AcceptTcpClient();
            var processor = CreateProcessor(s);
            var thread = new Thread(new ThreadStart(processor.ProcessRequest));
            thread.Start();
            Thread.Sleep(0);
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

        public void Dispose()
        {
            Stop();
        }

        protected static int FindOpenRandomPort()
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            var tryThis = rnd.Next(5000, 50000);
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
                }
            }
            return tryThis;
        }
    }
}