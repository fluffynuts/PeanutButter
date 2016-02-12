using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.Utils;

namespace NugetPackageVersionIncrementer.Tests
{
    [TestFixture]
    public class TestNuspecIncrementor
    {
        [TestCase("")]
        [TestCase("\r")]
        [TestCase("\n")]
        [TestCase(" ")]
        public void Construct_GivenInvalidNuspecPath_ShouldThrowAE(string input)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.Throws<ArgumentException>(() => new NuspecUtil(input));

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_GivenExistingNuspecPath_ShouldSetVersionProperty()
        {
            //---------------Set up test pack-------------------
            var tempFile = Path.GetTempFileName();
            using (new AutoDeleter(tempFile))
            {
                File.WriteAllBytes(tempFile, TestResources.package1);
                var sut = Create(tempFile);
                var expected = "1.1.33";
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.Version;

                //---------------Test Result -----------------------
                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        public void IncrementVersion_ShouldIncrementTheVersion()
        {
            // not defensive enough -- assumes pass-through to 
            //---------------Set up test pack-------------------
            var tempFile = Path.GetTempFileName();
            using (new AutoDeleter(tempFile))
            {
                File.WriteAllBytes(tempFile, TestResources.package1);
                var sut = Create(tempFile);
                var expected = "1.1.34";
                var xmlBefore = Encoding.UTF8.GetString(TestResources.package1);
                //---------------Assert Precondition----------------
                Assert.AreEqual(sut.NuspecVersionIncrementer.Version, sut.Version);

                //---------------Execute Test ----------------------
                sut.IncrementVersion();
                var result = sut.Version;

                //---------------Test Result -----------------------
                Assert.AreEqual(expected, result);
                Assert.AreEqual(sut.NuspecVersionIncrementer.Version, sut.Version);
                Assert.AreNotEqual(xmlBefore, sut.NuspecXml);
            }
        }

        [Test]
        public void PackageId_ShouldReturnPackageId()
        {
            var tempFile = Path.GetTempFileName();
            using (new AutoDeleter(tempFile))
            {
                File.WriteAllBytes(tempFile, TestResources.package1);
                var sut = Create(tempFile);
                var expected = "PeanutButter.TestUtils.Entity";
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.PackageId;

                //---------------Test Result -----------------------
                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        [Ignore("WIP")]
        public void SetPackageDependencyMinimumVersion_GivenUnknownPackageId_ShouldNotChangeAnything()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.Fail("Test Not Yet Implemented");
        }




        private NuspecUtil Create(string path)
        {
            return new NuspecUtil(path);
        }
    }
}
