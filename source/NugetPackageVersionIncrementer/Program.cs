using System;

namespace NugetPackageVersionIncrementer
{
    public class Program
    {
        static void Main(string[] args)
        {
            var coordinator = ResolveNuspecCoordinator();
            coordinator.LogAction = Console.WriteLine;
            coordinator.IncrementVersionsUnder(args);
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
