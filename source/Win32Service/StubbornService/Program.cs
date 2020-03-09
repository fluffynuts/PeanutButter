using System;
using System.Configuration;
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
            var howLongToWait = WaitTime;
            if (howLongToWait < 0)
            {
                // deadlock FOREVER
                semaphore.Wait();
            }
            else
            {
                // deadlock just for a while (:
                semaphore.Wait(TimeSpan.FromSeconds(howLongToWait));
            }
        }

        private static int WaitTime
            => int.TryParse(ConfigurationManager.AppSettings["WaitTime"], out var result)
                ? result
                : 60;
    }
}