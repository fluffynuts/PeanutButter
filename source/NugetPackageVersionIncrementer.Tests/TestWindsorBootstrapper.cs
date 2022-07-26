using System;
using NUnit.Framework;

namespace NugetPackageVersionIncrementer.Tests
{
    [TestFixture]
    public class TestWindsorBootstrapper
    {
        [TestCase(typeof(INuspecUtil), typeof(NuspecUtil))]
        [TestCase(typeof(INuspecVersionCoordinator), typeof(NuspecVersionCoordinator))]
        public void ShouldBeAbleToResolve_(Type interfaceType, Type expectedResolution)
        {
            //---------------Set up test pack-------------------
            var sut = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var container = sut.Bootstrap();
            var result = container.Resolve(interfaceType);

            //---------------Test Result -----------------------
            Assert.IsInstanceOf(expectedResolution, result);

        }

        private Bootstrapper Create()
        {
            return new Bootstrapper();
        }
    }
}