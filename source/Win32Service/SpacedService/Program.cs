using System.ServiceProcess;

namespace SpacedService
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Program
    {
        public static void Main(string[] args)
        {
            ServiceBase.Run(new Service()
            {
                Args = args
            });
        }
    }
}