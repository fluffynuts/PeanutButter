using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

namespace SpacedService
{
    public class Service : ServiceBase
    {
        private Thread _thread;
        private bool _running;
        public string[] Args { get; set; }

        protected override void OnStart(string[] args)
        {
            _running = true;
            _thread = new Thread(() =>
            {
                while (_running)
                {
                    Debug.WriteLine($"Spaced Service is Running! ({Args.Length} args)");
                    Thread.Sleep(100);
                }
                Debug.WriteLine("Spaced Service is outta here!");
            });
            _thread.Start();
        }

        protected override void OnStop()
        {
            _running = false;
            _thread.Join();
        }
    }
}