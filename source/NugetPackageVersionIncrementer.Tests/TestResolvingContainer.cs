using System;
using DryIoc;
using NSubstitute;
using NUnit.Framework;
// ReSharper disable ObjectCreationAsStatement
// ReSharper disable MemberCanBePrivate.Global

namespace NugetPackageVersionIncrementer.Tests
{
    [TestFixture]
    public class TestResolvingContainer
    {
        [Test]
        public void Type_ShouldImplement_IResolvingContainer()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ResolvingContainer);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(sut)
                .To.Implement<IResolvingContainer>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_GivenNullContainer_ShouldThrowANE()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => new ResolvingContainer(null))
                .To.Throw<ArgumentException>();

            //---------------Test Result -----------------------
        }

        public interface ITestInterface;
        public class TestClass : ITestInterface;

        [Test]
        public void Resolve_ShouldCallIntoProvidedContainer()
        {
            //---------------Set up test pack-------------------
            var container = Substitute.For<IContainer>();
            var expected = new TestClass();
            container.Resolve<ITestInterface>().Returns(expected);
            var sut = Create(container);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.Resolve<ITestInterface>();

            //---------------Test Result -----------------------
            Expect(container)
                .To.Have.Received(1)
                .Resolve<ITestInterface>();
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void Resolve_GivenType_ShouldCallIntoProvidedContainer()
        {
            //---------------Set up test pack-------------------
            var container = Substitute.For<IContainer>();
            var expected = new TestClass();
            var serviceType = typeof(ITestInterface);
            container.Resolve(serviceType).Returns(expected);
            var sut = Create(container);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.Resolve(serviceType);

            //---------------Test Result -----------------------
            Expect(container)
                .To.Have.Received(1)
                .Resolve(serviceType);
            Expect(result).To.Equal(expected);
        }

        private static IResolvingContainer Create(IContainer container)
        {
            return new ResolvingContainer(container);
        }
    }
}
