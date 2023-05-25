using NExpect;
using NUnit.Framework;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.Utils;
using static NExpect.Expectations;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestMinimalServiceProvider
{
    [Test]
    public void ShouldResolveFromFactory()
    {
        // Arrange
        var sut = Create();
        // Act
        sut.Register<IService>(() => new Service());
        var result = sut.Resolve<IService>();
        // Assert
        Expect(result)
            .To.Be.An.Instance.Of<Service>();
    }

    [Test]
    public void ShouldResolveFromTypeMapWithNoConstructorParams()
    {
        // Arrange
        var sut = Create();
        // Act
        sut.Register<IService, Service>();
        var result = sut.Resolve<IService>();
        // Assert
        Expect(result)
            .To.Be.An.Instance.Of<Service>();
    }

    [Test]
    public void ShouldResolveConcreteTypeWithoutRegistration()
    {
        // Arrange
        var sut = Create();
        // Act
        var result = sut.Resolve<Service>();
        // Assert
        Expect(result)
            .To.Be.An.Instance.Of<Service>();
    }

    [Test]
    public void ShouldResolveSingleImplementationWithoutRegistration()
    {
        // Arrange
        var sut = Create();
        // Act
        var result = sut.Resolve<IService>();
        // Assert
        Expect(result)
            .To.Be.An.Instance.Of<Service>();
    }

    [Test]
    public void ShouldResolveParameterlessImplementationWhenMultipleImplementationsExist()
    {
        // Arrange
        var sut = Create();
        // Act
        var result = sut.Resolve<IService2>();
        // Assert
        Expect(result)
            .To.Be.An.Instance.Of<Service2b>();
    }

    [Test]
    public void ShouldResolveParameteredConstructorWhenParametersCanBeResolved()
    {
        // Arrange
        var sut = Create();
        // Act
        var result = sut.Resolve<IService3>();
        // Assert
        Expect(result)
            .To.Be.An.Instance.Of<Service3>();
        Expect(result.Dependency)
            .To.Be.An.Instance.Of<Service>();
    }

    [Test]
    public void ShouldResolveParameteredConstructorWhenMultipleChoicesAndOneCanBeResolved()
    {
        // Arrange
        var sut = Create();
        // Act
        var result = sut.Resolve<IService4>();
        // Assert
        Expect(result)
            .To.Be.An.Instance.Of<Service4b>();
    }

    [Test]
    public void ShouldResolveServiceWithMultipleConstructorsWhenHaveAllDependencies()
    {
        // Arrange
        var sut = Create();
        // Act
        var result = sut.Resolve<IService5>();
        // Assert
        Expect(result)
            .To.Be.An.Instance.Of<Service5>();
        Expect(result.Service4)
            .Not.To.Be.Null();
        Expect(result.Service3)
            .Not.To.Be.Null();
    }

    public interface IService5
    {
        IService4 Service4 { get; }
        IService3 Service3 { get; }
    }

    public class Service5: IService5
    {
        public IService4 Service4 { get; }
        public IService3 Service3 { get; }

        public Service5()
        {
        }

        public Service5(
            IService4 service4,
            IService3 service3
        )
        {
            Service4 = service4;
            Service3 = service3;
        }
    }

    public interface IService4
    {
    }

    public interface IDependency1
    {
    }

    public interface IDependency2
    {
    }

    public class Dependency : IDependency2
    {
    }

    public class Service4a : IService4
    {
        public IDependency1 Dep { get; }

        public Service4a(IDependency1 dep)
        {
            Dep = dep;
        }
    }

    public class Service4b: IService4
    {
        public Service4b(IDependency2 dep)
        {
        }
    }

    public interface IService3
    {
        IService Dependency { get; }
    }

    public class Service3 : IService3
    {
        public IService Dependency { get; }

        public Service3(IService dependency)
        {
            Dependency = dependency;
        }
    }

    public interface IService2
    {
    }

    public class Service2a : IService2
    {
        public Service2a(bool flag)
        {
        }
    }

    public class Service2b : IService2
    {
        public Service2b()
        {
        }
    }

    public class Service : IService
    {
    }

    public interface IService
    {
    }

    [Test]
    public void ShouldResolveSimpleConcreteType()
    {
        // Arrange
        var sut = Create();
        // Act
        var result = sut.Resolve<Service>();
        // Assert
        Expect(result)
            .Not.To.Be.Null();
    }

    [Test]
    public void ShouldResolveSimpleDependency()
    {
        // Arrange
        var sut = Create();
        sut.Register<IService, Service>();
        // Act
        var result = sut.Resolve<Cow>();
        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Dep)
            .Not.To.Be.Null();
    }

    [Test]
    public void ShouldResolveTwoLevelsOfDependency()
    {
        // Arrange
        var sut = Create();
        sut.Register<IService, Service>();
        sut.Register<ICow, Cow>();
        // Act
        var result = sut.Resolve<Barn>();
        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result.Cow)
            .Not.To.Be.Null();
        Expect(result.Cow.Dep)
            .Not.To.Be.Null();
    }

    public class Barn
    {
        public ICow Cow { get; }

        public Barn(ICow cow)
        {
            Cow = cow;
        }
    }

    public interface ICow
    {
        IService Dep { get; }
        void Moo();
    }

    public class Cow
        : ICow
    {
        public IService Dep { get; }

        public Cow(IService dep)
        {
            Dep = dep;
        }

        public void Moo()
        {
        }
    }

    private static MinimalServiceProvider Create()
    {
        return new();
    }
}