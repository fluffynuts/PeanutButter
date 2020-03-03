using System;
using System.ServiceProcess;
using System.Threading;

namespace StubbornService
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceBase.Run(new Service());
        }
    }

    public class Service : ServiceBase
    {
        protected override void OnStop()
        {
            var semaphore = new SemaphoreSlim(1);
            semaphore.Wait();
            // deadlock, on purpose
            semaphore.Wait(TimeSpan.FromMinutes(1));
        }
    }
}