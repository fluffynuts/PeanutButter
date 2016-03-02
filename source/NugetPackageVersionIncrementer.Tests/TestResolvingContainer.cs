using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Windsor;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;

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
            sut.ShouldImplement<IResolvingContainer>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_GivenNullContainer_ShouldThrowANE()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<ArgumentNullException>(() => new ResolvingContainer(null));

            //---------------Test Result -----------------------
        }

        public interface ITestInterface
        {
        }
        public class TestClass : ITestInterface
        {
        }

        [Test]
        public void Resolve_ShouldCallIntoProvidedContainer()
        {
            //---------------Set up test pack-------------------
            var container = Substitute.For<IWindsorContainer>();
            var expected = new TestClass();
            container.Resolve<ITestInterface>().Returns(expected);
            var sut = Create(container);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.Resolve<ITestInterface>();

            //---------------Test Result -----------------------
            container.Received().Resolve<ITestInterface>();
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Resolve_GivenType_ShouldCallIntoProvidedContainer()
        {
            //---------------Set up test pack-------------------
            var container = Substitute.For<IWindsorContainer>();
            var expected = new TestClass();
            var serviceType = typeof(ITestInterface);
            container.Resolve(serviceType).Returns(expected);
            var sut = Create(container);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.Resolve(serviceType);

            //---------------Test Result -----------------------
            container.Received().Resolve(serviceType);
            Assert.AreEqual(expected, result);
        }

        private static IResolvingContainer Create(IWindsorContainer container)
        {
            return new ResolvingContainer(container);
        }
    }
}
