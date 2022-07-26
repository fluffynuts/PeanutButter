using System;
using DryIoc;

namespace NugetPackageVersionIncrementer
{
    public interface IResolvingContainer
    {
        T Resolve<T>() where T: class;
        object Resolve(Type type);
    }
    public class ResolvingContainer: IResolvingContainer
    {
        private readonly IContainer _container;

        public ResolvingContainer(IContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
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