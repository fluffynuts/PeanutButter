using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace NugetPackageVersionIncrementer
{
    public class WindsorBootstrapper
    {
        public WindsorContainer Bootstrap()
        {
            var container = new WindsorContainer();
            container.RegisterTransient<INuspecUtil, NuspecUtil>();
            return container;
        }
    }

    public static class WindsorContainerBootstrapperExtensions
    {
        public static void RegisterTransient<TService, TImplementation>(this WindsorContainer container)
            where TImplementation: TService
            where TService: class
        {
            container.Register(Component.For<TService>()
                                .ImplementedBy<TImplementation>()
                                .LifestyleTransient());
        }
    }
}