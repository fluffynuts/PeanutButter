using System.Linq;
using System.Reflection;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using PeanutButter.Utils;

namespace NugetPackageVersionIncrementer
{
    public static class WindsorContainerBootstrapperExtensions
    {
        public static void RegisterAllOneToOneResolutionsAsTransientFrom(this WindsorContainer container, params Assembly[] assemblies)
        {
            var allTypes = assemblies.SelectMany(a => a.GetTypes());
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
    }
}