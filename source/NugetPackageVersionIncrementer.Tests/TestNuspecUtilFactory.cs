using NUnit.Framework;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;

namespace NugetPackageVersionIncrementer.Tests
{
    [TestFixture]
    public class TestNuspecUtilFactory
    {
        [Test]
        public void Type_ShouldImplement_INuspecUtilFactory()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(NuspecUtilFactory);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldImplement<INuspecUtilFactory>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void LoadNuspecAt_ShouldReturnNewNuspecUtilEveryTimeWithNuspecLoaded()
        {
            //---------------Set up test pack-------------------
            var sut = Create();

            using (var tempFile1 = new AutoTempFile(TestResources.package1))
            using (var tempFile2 = new AutoTempFile(TestResources.package2))
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result1 = sut.LoadNuspecAt(tempFile1.Path);
                var result2 = sut.LoadNuspecAt(tempFile2.Path);
                var dup1 = sut.LoadNuspecAt(tempFile1.Path);

                //---------------Test Result -----------------------
                Expect(result1)
                    .To.Be.An.Instance.Of<NuspecUtil>();
                Expect(result2)
                    .To.Be.An.Instance.Of<NuspecUtil>();
                Expect(dup1)
                    .To.Be.An.Instance.Of<NuspecUtil>();

                Expect(result1.PackageId)
                    .To.Equal("PeanutButter.TestUtils.Entity");
                Expect(result2.PackageId)
                    .To.Equal("PeanutButter.DatabaseHelpers");
                Expect(dup1.PackageId)
                    .To.Equal("PeanutButter.TestUtils.Entity");
            }
        }

        private INuspecUtilFactory Create()
        {
            return new NuspecUtilFactory();
        }
    }
}
