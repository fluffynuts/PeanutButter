using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;
using NExpect;
using static NExpect.Expectations;

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
            Expect(result)
                .Not.To.Be.Null();
            Expect(result)
                .To.Be.Empty();
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
                Expect(folder)
                    .Not.To.Exist();

                //---------------Execute Test ----------------------
                Expect(() => sut.FindNuspecsUnder(folder))
                    .To.Throw<ArgumentException>()
                    .With.Message.Containing("not found");

                //---------------Test Result -----------------------
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
                Expect(files)
                    .To.Be.Empty();

                //---------------Execute Test ----------------------
                sut.FindNuspecsUnder(tempFolder);

                //---------------Test Result -----------------------
                Expect(sut.NuspecPaths)
                    .To.Be.Empty();
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
                var expected = Path.Combine(tempFolder, RandomValueGen.GetRandomString(2, 20) + ".nuspec");
                File.WriteAllBytes(expected, TestResources.package1.AsBytes());

                //---------------Assert Precondition----------------
                Expect(File.ReadAllBytes(expected).Select(b => (char)b))
                    .To.Equal(TestResources.package1);

                //---------------Execute Test ----------------------
                sut.FindNuspecsUnder(tempFolder);

                //---------------Test Result -----------------------
                Expect(sut.NuspecPaths)
                    .To.Contain.Only(1)
                    .Equal.To(expected);
            }
        }

        [Test]
        public void FindNuspecsUnder_GivenExistingPathWithNuspecsSprinkledUnderThatPath_ShouldAddAllNuspecPaths()
        {
            //---------------Set up test pack-------------------
            var sut = Create();
            var file1 = RandomValueGen.GetRandomString(5, 10) + ".nuspec";
            var file2 = RandomValueGen.GetRandomString(5, 10) + ".nuspec";
            using var folder = new AutoTempFolder();
            using var nuspec1 = new AutoTempFile(folder.Path, file1, TestResources.package1.AsBytes());
            using var nuspec2 = new AutoTempFile(
                SomeRandomFolderUnder(folder),
                file2,
                TestResources.package2.AsBytes()
            );
            //---------------Assert Precondition----------------
            Expect(nuspec1.BinaryData.Select(b => (char)b))
                .To.Equal(TestResources.package1);
            Expect(nuspec2.BinaryData.Select(b => (char)b))
                .To.Equal(TestResources.package2);
            Expect(sut.NuspecPaths)
                .To.Be.Empty();

            //---------------Execute Test ----------------------
            sut.FindNuspecsUnder(folder.Path);

            //---------------Test Result -----------------------
            Expect(sut.NuspecPaths)
                .To.Be.Equivalent.To(
                    new[]
                    {
                        nuspec1.Path,
                        nuspec2.Path
                    }
                );
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