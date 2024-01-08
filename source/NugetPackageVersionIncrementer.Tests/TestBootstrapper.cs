using System;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;

namespace NugetPackageVersionIncrementer.Tests
{
    [TestFixture]
    public class TestBootstrapper
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
            Expect(result)
                .To.Be.An.Instance.Of(expectedResolution);

        }

        private Bootstrapper Create()
        {
            return new Bootstrapper();
        }
    }
}