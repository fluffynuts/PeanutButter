using System;
using Castle.Windsor;

namespace NugetPackageVersionIncrementer
{
    public interface IResolvingContainer
    {
        T Resolve<T>() where T: class;
        object Resolve(Type type);
    }
    public class ResolvingContainer: IResolvingContainer
    {
        private readonly IWindsorContainer _container;

        public ResolvingContainer(IWindsorContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            _container = container;
        }

        public T Resolve<T>() where T : class
        {
            return _container.Resolve<T>();
        }

        public object Resolve(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }
    }
}