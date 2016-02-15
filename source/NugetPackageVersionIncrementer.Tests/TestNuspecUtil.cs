using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.Utils;

namespace NugetPackageVersionIncrementer.Tests
{
    [TestFixture]
    public class TestNuspecUtil
    {
        [Test]
        public void Construct_GivenExistingNuspecPath_ShouldSetVersionProperty()
        {
            //---------------Set up test pack-------------------
            var tempFile = Path.GetTempFileName();
            using (new AutoDeleter(tempFile))
            {
                File.WriteAllBytes(tempFile, TestResources.package1);
                var sut = CreateAndLoad(tempFile);
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
                var sut = CreateAndLoad(tempFile);
                var expected = "1.1.34";
                var xmlBefore = Encoding.UTF8.GetString(TestResources.package1);
                //---------------Assert Precondition----------------
                Assert.AreEqual(sut.Version, sut.Version);

                //---------------Execute Test ----------------------
                sut.IncrementVersion();
                var result = sut.Version;

                //---------------Test Result -----------------------
                Assert.AreEqual(expected, result);
                Assert.AreEqual(sut.Version, sut.Version);
                Assert.AreNotEqual(xmlBefore, sut.NuspecXml);
                // test that changes aren't persisted yet
                var onDisk = Encoding.UTF8.GetString(File.ReadAllBytes(tempFile));
                var doc = XDocument.Parse(onDisk);
                var versionNode = doc.XPathSelectElement("/package/metadata/version");
                Assert.IsNotNull(versionNode);
                Assert.AreNotEqual(expected, versionNode.Value);
            }
        }

        [Test]
        public void Persist_ShouldPersist()
        {
            // not defensive enough -- assumes pass-through to 
            //---------------Set up test pack-------------------
            var tempFile = Path.GetTempFileName();
            using (new AutoDeleter(tempFile))
            {
                File.WriteAllBytes(tempFile, TestResources.package1);
                var sut = CreateAndLoad(tempFile);
                var expected = "1.1.34";
                var xmlBefore = Encoding.UTF8.GetString(TestResources.package1);
                //---------------Assert Precondition----------------
                Assert.AreEqual(sut.Version, sut.Version);

                //---------------Execute Test ----------------------
                sut.IncrementVersion();
                sut.Persist();
                var result = sut.Version;

                //---------------Test Result -----------------------
                Assert.AreEqual(expected, result);
                Assert.AreEqual(sut.Version, sut.Version);
                Assert.AreNotEqual(xmlBefore, sut.NuspecXml);
                var onDisk = Encoding.UTF8.GetString(File.ReadAllBytes(tempFile));
                var doc = XDocument.Parse(onDisk);
                var versionNode = doc.XPathSelectElement("/package/metadata/version");
                Assert.IsNotNull(versionNode);
                Assert.AreEqual(expected, versionNode.Value);
            }
        }

        [Test]
        public void IncrementVersion_then_Persist_ShouldPersistExactlyLikeLegacy()
        {
            //---------------Set up test pack-------------------
            var tempFile = Path.GetTempFileName();
            string newMethodArtifact;
            using (new AutoDeleter(tempFile))
            {
                File.WriteAllBytes(tempFile, TestResources.package1);
                var sut = CreateAndLoad(tempFile);
                var expected = "1.1.34";
                var xmlBefore = Encoding.UTF8.GetString(TestResources.package1);
                //---------------Assert Precondition----------------
                Assert.AreEqual(sut.Version, sut.Version);

                //---------------Execute Test ----------------------
                sut.IncrementVersion();
                sut.Persist();
                var result = sut.Version;

                //---------------Test Result -----------------------
                Assert.AreEqual(expected, result);
                Assert.AreEqual(sut.Version, sut.Version);
                Assert.AreNotEqual(xmlBefore, sut.NuspecXml);
                var onDisk = Encoding.UTF8.GetString(File.ReadAllBytes(tempFile));
                newMethodArtifact = onDisk;
                var doc = XDocument.Parse(onDisk);
                var versionNode = doc.XPathSelectElement("/package/metadata/version");
                Assert.IsNotNull(versionNode);
                Assert.AreEqual(expected, versionNode.Value);
            }

            using (var packageFile = new AutoTempFile(TestResources.package1))
            {
                var legacy = new LegacyPackageVersionIncrementer();
                legacy.IncrementVersionOn(packageFile.FileName);
                var legacyArtifact = Encoding.UTF8.GetString(File.ReadAllBytes(packageFile.FileName));
                Assert.AreEqual(legacyArtifact, newMethodArtifact);
            }
        }

        [Test]
        public void PackageId_ShouldReturnPackageId()
        {
            var tempFile = Path.GetTempFileName();
            using (new AutoDeleter(tempFile))
            {
                File.WriteAllBytes(tempFile, TestResources.package1);
                var sut = CreateAndLoad(tempFile);
                var expected = "PeanutButter.TestUtils.Entity";
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.PackageId;

                //---------------Test Result -----------------------
                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        public void SetPackageDependencyVersionIfExists_GivenUnknownPackageId_ShouldNotChangeAnything()
        {
            using (var packageFile = new AutoTempFile(TestResources.package1))
            {
                //---------------Set up test pack-------------------
                var doc = XDocument.Parse(packageFile.StringData);
                var packageId = RandomValueGen.GetRandomString(10, 20);
                var version = GetRandomVersionString();
                var sut = CreateAndLoad(packageFile.FileName);

                //---------------Assert Precondition----------------
                Assert.IsNull(doc.GetDependencyVersionFor(packageId));

                //---------------Execute Test ----------------------
                sut.SetPackageDependencyVersionIfExists(packageId, version);


                //---------------Test Result -----------------------
                var result = packageFile.BinaryData;
                CollectionAssert.AreEqual(TestResources.package1, result);
            }
        }

        private static string GetRandomVersionString()
        {
            return string.Join(".", RandomValueGen.GetRandomCollection<int>(3,3));
        }


        private NuspecUtil CreateAndLoad(string path)
        {
            var util = new NuspecUtil();
            util.LoadNuspecAt(path);
            return util;
        }
    }
}
