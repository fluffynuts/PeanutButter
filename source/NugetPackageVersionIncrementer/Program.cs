using System;
using System.IO;
using System.Linq;
using System.Reflection;

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
            EngageBackgroundJIT();
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
                return -1;
            }
        }

        // ReSharper disable once InconsistentNaming
        private static void EngageBackgroundJIT()
        {
            var appPath = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            var appFolder = Path.GetDirectoryName(appPath);
            var appName = Path.GetFileName(appPath);
            System.Runtime.ProfileOptimization.SetProfileRoot(appFolder);
            System.Runtime.ProfileOptimization.StartProfile(appName + ".profile");
        }

        private static INuspecVersionCoordinator ResolveNuspecCoordinator()
        {
            var bootstrapper = new WindsorBootstrapper();
            var container = bootstrapper.Bootstrap();
            var coordinator = container.Resolve<INuspecVersionCoordinator>();
            return coordinator;
        }
    }
}
