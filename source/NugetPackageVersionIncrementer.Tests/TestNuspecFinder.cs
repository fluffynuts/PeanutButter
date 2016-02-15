using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;

namespace NugetPackageVersionIncrementer.Tests
{
    [TestFixture]
    public class TestNuspecFinder
    {
        [Test]
        public void Type_ShouldImplement_INuspecFinder()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(NuspecFinder);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldImplement<INuspecFinder>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => Create());

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_ShouldSetNuspecPathsToEmptyCollection()
        {
            //---------------Set up test pack-------------------
            var sut = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.NuspecPaths;

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void FindNuspecsUnder_GivenInvalidPath_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            var folder = GetUnknownTempFolder();
            using (new AutoDeleter(folder))
            {
                var sut = Create();

                //---------------Assert Precondition----------------
                Assert.IsFalse(Directory.Exists(folder));

                //---------------Execute Test ----------------------
                var ex = Assert.Throws<ArgumentException>(() => sut.FindNuspecsUnder(folder));

                //---------------Test Result -----------------------
                StringAssert.Contains("not found", ex.Message);
            }
        }

        private static string GetUnknownTempFolder()
        {
            string folder;
            do
            {
                folder = Path.Combine(Path.GetTempPath(), RandomValueGen.GetRandomString(10, 20));
            } while (Directory.Exists(folder));
            return folder;
        }

        private string CreateNewTempFolder()
        {
            var folder = GetUnknownTempFolder();
            Directory.CreateDirectory(folder);
            return folder;
        }

        [Test]
        public void FindNuspecsUnder_GivenExistingPathWithNoNuspecs_ShouldLeaveNuspecPathsEmpty()
        {
            //---------------Set up test pack-------------------
            var tempFolder = CreateNewTempFolder();
            var sut = Create();
            using (new AutoDeleter(tempFolder))
            {
                //---------------Assert Precondition----------------
                var files = Directory.EnumerateFileSystemEntries(tempFolder, "*", SearchOption.AllDirectories);
                CollectionAssert.IsEmpty(files);

                //---------------Execute Test ----------------------
                sut.FindNuspecsUnder(tempFolder);

                //---------------Test Result -----------------------
                CollectionAssert.IsEmpty(sut.NuspecPaths);
            }
        }

        [Test]
        public void FindNuspecsUnder_GivenExistingPathWithOneNuspecInIt_ShouldAddThatNuspecPath()
        {
            //---------------Set up test pack-------------------
            var tempFolder = CreateNewTempFolder();
            var sut = Create();
            using (new AutoDeleter(tempFolder))
            {
                var expected = Path.Combine(tempFolder, RandomValueGen.GetRandomString(2,20) + ".nuspec");
                File.WriteAllBytes(expected, TestResources.package1.AsBytes());

                //---------------Assert Precondition----------------
                CollectionAssert.AreEqual(TestResources.package1, File.ReadAllBytes(expected));

                //---------------Execute Test ----------------------
                sut.FindNuspecsUnder(tempFolder);

                //---------------Test Result -----------------------
                Assert.AreEqual(expected, sut.NuspecPaths.Single());
            }
        }

        [Test]
        public void FindNuspecsUnder_GivenExistingPathWithNuspecsSprinkledUnderThatPath_ShouldAddAllNuspecPaths()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var file1 = RandomValueGen.GetRandomString(5, 10) + ".nuspec";
            var file2 = RandomValueGen.GetRandomString(5, 10) + ".nuspec";
            using (var folder = new AutoTempFolder())
            using (var nuspec1 = new AutoTempFile(folder.Path, file1, TestResources.package1.AsBytes()))
            using (var nuspec2 = new AutoTempFile(SomeRandomFolderUnder(folder), file2, TestResources.package2.AsBytes()))
            {
                //---------------Assert Precondition----------------
                CollectionAssert.AreEqual(TestResources.package1, nuspec1.BinaryData);
                CollectionAssert.AreEqual(TestResources.package2, nuspec2.BinaryData);
                CollectionAssert.IsEmpty(sut.NuspecPaths);

                //---------------Execute Test ----------------------
                sut.FindNuspecsUnder(folder.Path);

                //---------------Test Result -----------------------
                CollectionAssert.IsNotEmpty(sut.NuspecPaths);
                CollectionAssert.Contains(sut.NuspecPaths, nuspec1.Path);
                CollectionAssert.Contains(sut.NuspecPaths, nuspec2.Path);
            }
        }

        private static string SomeRandomFolderUnder(AutoTempFolder folder)
        {
            return Path.Combine(folder.Path, RandomValueGen.GetRandomAlphaNumericString(5, 10));
        }


        private static NuspecFinder Create()
        {
            return new NuspecFinder();
        }
    }
}
