using System;
using System.Linq;
using System.Reflection;
using Castle.Core.Internal;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace PeanutButter.Utils.Windsor
{
    public static class WindsorContainerExtensions
    {
        public static void RegisterAllOneToOneResolutionsAsTransientFrom(this IWindsorContainer container, params Assembly[] assemblies)
        {
            if (assemblies.IsEmpty())
                throw NoAssembliesException;
            var allTypes = assemblies.SelectMany(a => a.GetTypes()).ToArray();
            var allInterfaces = allTypes.Where(t => t.IsInterface);
            var allImplementations = allTypes.Where(t => !t.IsInterface &&
                                                         !t.IsAbstract &&
                                                         !t.IsGenericType);
            allInterfaces.ForEach(interfaceType =>
            {
                if (container.Kernel.HasComponent(interfaceType))
                    return;
                var implementingTypes = allImplementations
                                            .Where(interfaceType.IsAssignableFrom)
                                            .ToArray();
                if (implementingTypes.Length != 1)
                    return;
                container.Register(Component.For(interfaceType)
                    .ImplementedBy(implementingTypes.Single())
                    .LifestyleTransient());
            });
        }

        public static void RegisterAllControllersFrom(this IWindsorContainer container, params Assembly[] assemblies)
        {
            if (assemblies.IsEmpty())
                throw NoAssembliesException;
            var controllerTypes = assemblies.SelectMany(a => a.GetTypes().Where(IsBasedOnMvcController));
            controllerTypes.ForEach(t => container.Register(Component.For(t).ImplementedBy(t).LifestyleTransient()));
        }

        public static void RegisterSingleton<TService, TImplementation>(this IWindsorContainer container)
            where TService: class
            where TImplementation: TService
        {
            container.Register(Component.For<TService>()
                                    .ImplementedBy<TImplementation>()
                                    .LifestyleSingleton());
        }

        public static void RegisterTransient<TService, TImplementation>(this IWindsorContainer container)
            where TService: class
            where TImplementation: TService
        {
            container.Register(Component.For<TService>()
                                        .ImplementedBy<TImplementation>()
                                        .LifestyleTransient());
        }

        public static void RegisterPerWebRequest<TService, TImplementation>(this IWindsorContainer container)
            where TService: class
            where TImplementation: TService
        {
            container.Register(Component.For<TService>()
                                        .ImplementedBy<TImplementation>()
                                        .LifestylePerWebRequest());
        }

        private static bool IsBasedOnMvcController(Type type)
        {
            return type.Ancestry()
                        .Any(t => t.Name == "Controller" &&
                                    t.Namespace == CONTROLLER_NAMESPACE &&
                                    t.Assembly.FullName.StartsWith(CONTROLLER_ASSEMBLY));
        }

        private const string CONTROLLER_NAMESPACE = "System.Web.Mvc";
        private const string CONTROLLER_ASSEMBLY = CONTROLLER_NAMESPACE + ",";

        private static ArgumentException NoAssembliesException => 
                new ArgumentException("No assemblies provided to search for registrations");
    }
}
