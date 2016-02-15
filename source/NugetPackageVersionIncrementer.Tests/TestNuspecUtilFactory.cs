using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                Assert.IsInstanceOf<NuspecUtil>(result1);
                Assert.IsInstanceOf<NuspecUtil>(result2);
                Assert.IsInstanceOf<NuspecUtil>(dup1);

                Assert.AreEqual("PeanutButter.TestUtils.Entity", result1.PackageId);
                Assert.AreEqual("PeanutButter.DatabaseHelpers", result2.PackageId);
                Assert.AreEqual("PeanutButter.TestUtils.Entity", dup1.PackageId);
                Assert.AreNotEqual(result1, result2);
                Assert.AreNotEqual(result1, dup1);
                Assert.AreNotEqual(result2, dup1);
            }
        }

        private INuspecUtilFactory Create()
        {
            return new NuspecUtilFactory();
        }
    }
}
