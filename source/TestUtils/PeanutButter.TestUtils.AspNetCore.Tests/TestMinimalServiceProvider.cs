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