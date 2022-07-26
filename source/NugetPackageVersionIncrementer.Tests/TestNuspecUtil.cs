using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;
using PeanutButter.Utils;
using static NExpect.Expectations;
using NExpect;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace NugetPackageVersionIncrementer.Tests
{
    [TestFixture]
    public class TestNuspecUtil
    {
        [Test]
        public void LoadNuspecAt_GivenExistingNuspecPath_ShouldSetVersionProperty()
        {
            //---------------Set up test pack-------------------
            var tempFile = Path.GetTempFileName();
            using (new AutoDeleter(tempFile))
            {
                File.WriteAllBytes(tempFile, TestResources.package1.AsBytes());
                var sut = Create();
                var expected = "1.1.33";
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                sut.LoadNuspecAt(tempFile);
                var result = sut.Version;

                //---------------Test Result -----------------------
                Expect(result).To.Equal(expected);
            }
        }

        [Test]
        public void LoadNuspecAt_GivenExistingNuspecPath_ShouldSetOriginalVersionProperty()
        {
            //---------------Set up test pack-------------------
            var tempFile = Path.GetTempFileName();
            using (new AutoDeleter(tempFile))
            {
                File.WriteAllBytes(tempFile, TestResources.package1.AsBytes());
                var sut = Create();
                var expected = "1.1.33";
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                sut.LoadNuspecAt(tempFile);
                var result = sut.OriginalVersion;

                //---------------Test Result -----------------------
                Expect(result).To.Equal(expected);
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
                File.WriteAllBytes(tempFile, TestResources.package1.AsBytes());
                var sut = CreateAndLoad(tempFile);
                var expected = "1.1.34";
                var xmlBefore = Encoding.UTF8.GetString(TestResources.package1.AsBytes());
                var originalVersion = sut.OriginalVersion;
                //---------------Assert Precondition----------------
                Assert.AreEqual(sut.OriginalVersion, sut.Version);

                //---------------Execute Test ----------------------
                sut.IncrementVersion();
                var result = sut.Version;

                //---------------Test Result -----------------------
                Expect(result).To.Equal(expected);
                Expect(sut.Version).Not.To.Equal(sut.OriginalVersion);
                Expect(sut.NuspecXml).Not.To.Equal(xmlBefore);
                Expect(sut.OriginalVersion).To.Equal(originalVersion);

                // test that changes aren't persisted yet
                var onDisk = Encoding.UTF8.GetString(File.ReadAllBytes(tempFile));
                var doc = XDocument.Parse(onDisk);
                var versionNode = doc.XPathSelectElement("/package/metadata/version");
                Expect(versionNode).Not.To.Be.Null();
                Expect(versionNode.Value).Not.To.Equal(expected);
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
                File.WriteAllBytes(tempFile, TestResources.package1.AsBytes());
                var sut = CreateAndLoad(tempFile);
                var expected = "1.1.34";
                var xmlBefore = Encoding.UTF8.GetString(TestResources.package1.AsBytes());
                //---------------Assert Precondition----------------
                Assert.AreEqual(sut.Version, sut.Version);

                //---------------Execute Test ----------------------
                sut.IncrementVersion();
                sut.Persist();
                var result = sut.Version;

                //---------------Test Result -----------------------
                Expect(result).To.Equal(expected);
                Expect(sut.NuspecXml).Not.To.Equal(xmlBefore);
                var onDisk = Encoding.UTF8.GetString(File.ReadAllBytes(tempFile));
                var doc = XDocument.Parse(onDisk);
                var versionNode = doc.XPathSelectElement("/package/metadata/version");
                Expect(versionNode).Not.To.Be.Null();
                Expect(versionNode.Value).To.Equal(expected);
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
                File.WriteAllBytes(tempFile, TestResources.package1.AsBytes());
                var sut = CreateAndLoad(tempFile);
                var expected = "1.1.34";
                var xmlBefore = Encoding.UTF8.GetString(TestResources.package1.AsBytes());
                //---------------Assert Precondition----------------
                Assert.AreEqual(sut.Version, sut.Version);

                //---------------Execute Test ----------------------
                sut.IncrementVersion();
                sut.Persist();
                var result = sut.Version;

                //---------------Test Result -----------------------
                Expect(result).To.Equal(expected);
                Expect(sut.NuspecXml).Not.To.Equal(xmlBefore);
                var onDisk = Encoding.UTF8.GetString(File.ReadAllBytes(tempFile));
                newMethodArtifact = onDisk;
                var doc = XDocument.Parse(onDisk);
                var versionNode = doc.XPathSelectElement("/package/metadata/version");
                Expect(versionNode).Not.To.Be.Null();
                Expect(versionNode.Value).To.Equal(expected);
            }

            using (var packageFile = new AutoTempFile(TestResources.package1))
            {
                var legacy = new LegacyPackageVersionIncrementer();
                legacy.IncrementVersionOn(packageFile.Path);
                var legacyArtifact = Encoding.UTF8.GetString(File.ReadAllBytes(packageFile.Path));
                Expect(newMethodArtifact)
                    .To.Equal(legacyArtifact);
            }
        }

        [Test]
        public void LoadNuspecAt_ShouldSetPackageIdFromNuspec()
        {
            var tempFile = Path.GetTempFileName();
            using (new AutoDeleter(tempFile))
            {
                File.WriteAllBytes(tempFile, TestResources.package1.AsBytes());
                var sut = Create();
                var expected = "PeanutButter.TestUtils.Entity";
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                sut.LoadNuspecAt(tempFile);
                var result = sut.PackageId;

                //---------------Test Result -----------------------
                Expect(result).To.Equal(expected);
            }
        }

        [Test]
        public void SetPackageDependencyVersionIfExists_GivenUnknownPackageId_ShouldNotChangeAnything()
        {
            using (var packageFile = new AutoTempFile(TestResources.package1))
            {
                //---------------Set up test pack-------------------
                var doc = XDocument.Parse(packageFile.StringData);
                var packageId = GetRandomString(10, 20);
                var version = GetRandomVersionString();
                var sut = CreateAndLoad(packageFile.Path);

                //---------------Assert Precondition----------------
                Expect(doc.GetDependencyVersionFor(packageId))
                    .To.Be.Null();

                //---------------Execute Test ----------------------
                sut.SetPackageDependencyVersionIfExists(packageId, version);


                //---------------Test Result -----------------------
                var result = packageFile.BinaryData;
                Expect(result.AsString())
                    .To.Equal(TestResources.package1);
            }
        }

        [Test]
        public void ShouldSetDepsForAllFrameworks()
        {
            using (var packageFile = new AutoTempFile(TestResources.multi_target_package))
            {
                // Arrange
                var doc = XDocument.Parse(packageFile.StringData);
                var startingNodes = doc.XPathSelectElements(
                    "/package/metadata/dependencies/group/dependency"
                );

                var sut = CreateAndLoad(packageFile.Path);
                Expect(startingNodes).Not.To.Be.Empty();
                Expect(startingNodes.All(
                    n => n.Parent?.Attribute("targetFramework")?.Value == "net40"
                )).To.Be.True();
                var targetFrameworks = sut.FindTargetedFrameworks();
                Expect(targetFrameworks)
                    .To.Be.Equivalent.To(new[] { "net462", "netstandard2.0" });
                // Act
                sut.EnsureSameDependencyGroupForAllTargetFrameworks();
                // Assert
                sut.Persist();
                
                var after = XDocument.Parse(packageFile.StringData);
                var groups = after.XPathSelectElements(
                    "/package/metadata/dependencies/group"
                );
                Expect(groups).To.Contain.Only(2).Items();
                Expect(groups).To.Contain.Exactly(1)
                    .Matched.By(n => n.Attribute("targetFramework")?.Value == "net462");
                Expect(groups).To.Contain.Exactly(1)
                    .Matched.By(n => n.Attribute("targetFramework")?.Value == "netstandard2.0");
            }
        }

        private static string GetRandomVersionString()
        {
            return string.Join(".", GetRandomCollection<int>(3, 3));
        }


        private INuspecUtil CreateAndLoad(string path)
        {
            var util = new NuspecUtil();
            util.LoadNuspecAt(path);
            return util;
        }

        private INuspecUtil Create()
        {
            return new NuspecUtil();
        }
    }
}