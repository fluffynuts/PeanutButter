using Castle.Windsor;

namespace NugetPackageVersionIncrementer
{
    public class WindsorBootstrapper
    {
        public IResolvingContainer Bootstrap()
        {
            var container = new WindsorContainer();
            container.RegisterAllOneToOneResolutionsAsTransientFrom(GetType().Assembly);
            return new ResolvingContainer(container);
        }
    }
}