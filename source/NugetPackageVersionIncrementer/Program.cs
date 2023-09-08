using System;
using System.Linq;
using PeanutButter.Utils;

namespace NugetPackageVersionIncrementer
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Contains("-v") || args.Contains("--version"))
            {
                Console.WriteLine("Version: 1.2");
                return 0;
            }
            try
            {
                var coordinator = ResolveNuspecCoordinator();
                coordinator.LogAction = Console.WriteLine;
                coordinator.IncrementVersionsUnder(args);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ruh-Roh! Something went wrong, Shaggy!");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Stack trace follows:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine($"Commandline: {MyExe} {args.Select(a => a.Contains(" ") ? $"\"{a}\"" : a).JoinWith(" ")}");
                return -1;
            }
        }
        
        private static string MyExe =>
            new Uri(typeof(Program).Assembly.Location).LocalPath;

        private static INuspecVersionCoordinator ResolveNuspecCoordinator()
        {
            var bootstrapper = new Bootstrapper();
            var container = bootstrapper.Bootstrap();
            var coordinator = container.Resolve<INuspecVersionCoordinator>();
            return coordinator;
        }
    }
}
