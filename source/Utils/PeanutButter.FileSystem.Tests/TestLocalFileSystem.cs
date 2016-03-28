using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.FileSystem.Tests
{
    [TestFixture]
    public class TestLocalFileSystem
    {
        [Test]
        [Ignore("WIP")]
        public void GetCurrentDirectory_WhenNoDirectorySet_ShouldReturnSameDirectoryAsOuterScope()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.Fail("Test Not Yet Implemented");
        }


        [Test]
        public void Type_ShouldImplent_IFileSystem()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(LocalFileSystem);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldImplement<IFileSystem>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => new LocalFileSystem());

            //---------------Test Result -----------------------
        }

        [Test]
        public void List_GivenNoArguments_ShouldListAllFilesAndDirectoriesInGivenPath()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var sub1 = "folder" + GetRandomString();
                var sub2 = "folder" + GetRandomString();
                var file1 = "file" + GetRandomString();
                var file2 = "file" + GetRandomString();

                Directory.CreateDirectory(Path.Combine(folder.Path, sub1));
                Directory.CreateDirectory(Path.Combine(folder.Path, sub2));
                File.WriteAllBytes(Path.Combine(folder.Path, file1), GetRandomBytes());
                File.WriteAllBytes(Path.Combine(folder.Path, file2), GetRandomBytes());

                var sut = Create();

                //---------------Assert Precondition----------------
                Assert.IsTrue(Directory.Exists(Path.Combine(folder.Path, sub1)));
                Assert.IsTrue(Directory.Exists(Path.Combine(folder.Path, sub1)));
                Assert.IsTrue(File.Exists(Path.Combine(folder.Path, file1)));
                Assert.IsTrue(File.Exists(Path.Combine(folder.Path, file2)));

                //---------------Execute Test ----------------------
                var result = sut.List(folder.Path);

                //---------------Test Result -----------------------
                CollectionAssert.Contains(result, sub1);
                CollectionAssert.Contains(result, sub2);
                CollectionAssert.Contains(result, file1);
                CollectionAssert.Contains(result, file2);
            }
        }

        [Test]
        public void List_GivenSearchPattern_ShouldReturnOnlyMatchingDirectoriesAndFiles()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var matchPrefix = GetRandomString(3,5);
                var nonMatchPrefix = GetAnother(matchPrefix);
                var matchDirectory = matchPrefix + GetRandomString();
                var matchFile = matchPrefix + GetRandomString();
                var nonMatchDirectory = nonMatchPrefix + GetRandomString();
                var nonMatchFile = nonMatchPrefix + GetRandomString();

                Directory.CreateDirectory(Path.Combine(folder.Path, matchDirectory));
                Directory.CreateDirectory(Path.Combine(folder.Path, nonMatchDirectory));
                File.WriteAllBytes(Path.Combine(folder.Path, matchFile), GetRandomBytes());
                File.WriteAllBytes(Path.Combine(folder.Path, nonMatchFile), GetRandomBytes());
                var sut = Create();

                //---------------Assert Precondition----------------
                Assert.IsTrue(Directory.Exists(Path.Combine(folder.Path, matchDirectory)));
                Assert.IsTrue(Directory.Exists(Path.Combine(folder.Path, nonMatchDirectory)));
                Assert.IsTrue(File.Exists(Path.Combine(folder.Path, matchFile)));
                Assert.IsTrue(File.Exists(Path.Combine(folder.Path, nonMatchFile)));

                //---------------Execute Test ----------------------
                var result = sut.List(folder.Path, matchPrefix + "*");

                //---------------Test Result -----------------------
                CollectionAssert.Contains(result, matchDirectory);
                CollectionAssert.Contains(result, matchFile);
                CollectionAssert.DoesNotContain(result, nonMatchDirectory);
                CollectionAssert.DoesNotContain(result, nonMatchFile);
            }
        }

        [Test]
        public void List_GivenPathAndFileExtensionMatch_ShouldFineMatchingFiles()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var ext = GetRandomString(3,3);
                var otherExt = GetAnother(ext, () => GetRandomString(3,3));
                var matchFiles = GetRandomCollection(() => GetRandomString(2,4) + "." + ext, 3, 5);
                var nonMatchFiles = GetRandomCollection(() => GetRandomString(2,4) + "." + otherExt, 3, 5);
                matchFiles.Union(nonMatchFiles).ForEach(f =>
                    File.WriteAllBytes(Path.Combine(folder.Path, f), GetRandomBytes()));
                var sut = Create();
                //---------------Assert Precondition----------------
                Assert.IsTrue(matchFiles.Union(nonMatchFiles).All(f => File.Exists(Path.Combine(folder.Path, f))));

                //---------------Execute Test ----------------------
                var result = sut.List(folder.Path, "*." + ext);

                //---------------Test Result -----------------------
                Assert.IsTrue(matchFiles.All(f => result.Contains(f)));
                Assert.IsFalse(nonMatchFiles.Any(f => result.Contains(f)));
            }
        }

        [Test]
        public void ListRecursive_GivenPath_ShouldReturnAllFilesAndDirectoriesUnderThatPath()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var expected = CreateRandomFileTreeIn(folder.Path);
                var sut = Create();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.ListRecursive(folder.Path);

                //---------------Test Result -----------------------
                CollectionAssert.AreEquivalent(expected, result);
            }
        }

        [Test]
        public void ListRecursive_GivenPathAndSearchPatther_ShouldReturnAllFilesAndDirectoriesUnderThatPathWhichMatchTheSearchPattern()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                CreateRandomFileTreeIn(folder.Path);
                var search = "*a*";
                var expected = Directory.EnumerateFileSystemEntries(folder.Path, "*a*", SearchOption.AllDirectories)
                                    .Select(p => p.Substring(folder.Path.Length + 1));;
                var sut = Create();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.ListRecursive(folder.Path, search);

                //---------------Test Result -----------------------
                CollectionAssert.AreEquivalent(expected, result);
            }
        }

        [Test]
        public void ListFiles_GivenPath_ShouldReturnOnlyFilesFromPath()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var expected = GetRandomString();
                var unexpected = GetAnother(expected);
                Directory.CreateDirectory(Path.Combine(folder.Path, unexpected));
                File.WriteAllBytes(Path.Combine(folder.Path, expected), GetRandomBytes());
                var sut = Create();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.ListFiles(folder.Path);

                //---------------Test Result -----------------------
                CollectionAssert.Contains(result, expected);
                CollectionAssert.DoesNotContain(result, unexpected);
            }
        }

        [Test]
        public void ListFiles_GivenPathAndSearchPattern_ShouldReturnListOfMatchingFilesInPath()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var ext = GetRandomString(3,3);
                var otherExt = GetAnother(ext, () => GetRandomString(3,3));
                var matchFiles = GetRandomCollection(() => GetRandomString(2,4) + "." + ext, 3, 5);
                var nonMatchFiles = GetRandomCollection(() => GetRandomString(2,4) + "." + otherExt, 3, 5);
                matchFiles.Union(nonMatchFiles).ForEach(f =>
                    File.WriteAllBytes(Path.Combine(folder.Path, f), GetRandomBytes()));
                var unexpected = GetAnother(matchFiles, () => GetRandomString(2,4) + "." + ext);
                Directory.CreateDirectory(Path.Combine(folder.Path, unexpected));
                var sut = Create();
                //---------------Assert Precondition----------------
                Assert.IsTrue(matchFiles.Union(nonMatchFiles).All(f => File.Exists(Path.Combine(folder.Path, f))));

                //---------------Execute Test ----------------------
                var result = sut.ListFiles(folder.Path, "*." + ext);

                //---------------Test Result -----------------------
                Assert.IsTrue(matchFiles.All(f => result.Contains(f)));
                Assert.IsFalse(result.Contains(unexpected));
                Assert.IsFalse(nonMatchFiles.Any(f => result.Contains(f)));
            }
        }

        [Test]
        public void ListDirectories_GivenPath_ShouldReturnOnlyDirectoriesFromPath()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var expected = GetRandomString();
                var unexpected = GetAnother(expected);
                Directory.CreateDirectory(Path.Combine(folder.Path, expected));
                File.WriteAllBytes(Path.Combine(folder.Path, unexpected), GetRandomBytes());
                var sut = Create();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.ListDirectories(folder.Path);

                //---------------Test Result -----------------------
                CollectionAssert.Contains(result, expected);
                CollectionAssert.DoesNotContain(result, unexpected);
            }
        }

        [Test]
        public void ListDirectories_GivenPathAndSearchPattern_ShouldReturnListOfMatchingDirectoriesInPath()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var ext = GetRandomString(3, 3);
                var otherExt = GetAnother(ext, () => GetRandomString(3, 3));
                var matchDirectories = GetRandomCollection(() => GetRandomString(2, 4) + "." + ext, 3, 5);
                var nonMatchDirectories = GetRandomCollection(() => GetRandomString(2, 4) + "." + otherExt, 3, 5);
                matchDirectories.Union(nonMatchDirectories).ForEach(f =>
                    Directory.CreateDirectory(Path.Combine(folder.Path, f)));
                var unexpected = GetAnother(matchDirectories, () => GetRandomString(2, 4) + "." + ext);
                File.WriteAllBytes(Path.Combine(folder.Path, unexpected), GetRandomBytes());
                var sut = Create();
                //---------------Assert Precondition----------------
                Assert.IsTrue(matchDirectories.Union(nonMatchDirectories).All(f => Directory.Exists(Path.Combine(folder.Path, f))));

                //---------------Execute Test ----------------------
                var result = sut.ListDirectories(folder.Path, "*." + ext);

                //---------------Test Result -----------------------
                Assert.IsTrue(matchDirectories.All(f => result.Contains(f)));
                Assert.IsFalse(result.Contains(unexpected));
                Assert.IsFalse(nonMatchDirectories.Any(f => result.Contains(f)));
            }
        }

        [Test]
        public void ListFilesRecursive_GivenPath_ShouldReturnOnlyFilesFromPathAndBelow()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var expected = GetRandomString();
                var unexpected = GetAnother(expected);
                Directory.CreateDirectory(Path.Combine(folder.Path, unexpected));
                var expected2 = Path.Combine(unexpected, GetRandomString());
                File.WriteAllBytes(Path.Combine(folder.Path, expected), GetRandomBytes());
                File.WriteAllBytes(Path.Combine(folder.Path, expected2), GetRandomBytes());
                var sut = Create();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.ListFilesRecursive(folder.Path);

                //---------------Test Result -----------------------
                CollectionAssert.Contains(result, expected);
                CollectionAssert.Contains(result, expected2);
                CollectionAssert.DoesNotContain(result, unexpected);
            }
        }

        [Test]
        public void ListFilesRecursive_GivenPathAndSearchPattern_ShouldReturnListOfMatchingFilesInPath()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var ext = GetRandomString(3, 3);
                var otherExt = GetAnother(ext, () => GetRandomString(3, 3));
                var matchFiles = GetRandomCollection(() => GetRandomString(2, 4) + "." + ext, 3, 5);
                var nonMatchFiles = GetRandomCollection(() => GetRandomString(2, 4) + "." + otherExt, 3, 5);
                var subMatchFiles = new List<string>();
                matchFiles.ForEach(f =>
                {
                    var subDirectory = GetRandomString();
                    Directory.CreateDirectory(Path.Combine(folder.Path, subDirectory));
                    var subPath = Path.Combine(subDirectory, f);
                    subMatchFiles.Add(subPath);
                    File.WriteAllBytes(Path.Combine(folder.Path, subPath), GetRandomBytes());
                });
                var subNonMatchFiles = new List<string>();
                nonMatchFiles.ForEach(f =>
                {
                    var subDirectory = GetRandomString();
                    Directory.CreateDirectory(Path.Combine(folder.Path, subDirectory));
                    var subPath = Path.Combine(subDirectory, f);
                    subNonMatchFiles.Add(subPath);
                    File.WriteAllBytes(Path.Combine(folder.Path, subPath), GetRandomBytes());
                });
                var unexpected = GetAnother(matchFiles, () => GetRandomString(2, 4) + "." + ext);
                Directory.CreateDirectory(Path.Combine(folder.Path, unexpected));
                var sut = Create();
                //---------------Assert Precondition----------------
                Assert.IsTrue(subMatchFiles.Union(subNonMatchFiles)
                                .All(f => File.Exists(Path.Combine(folder.Path, f))));

                //---------------Execute Test ----------------------
                var result = sut.ListFilesRecursive(folder.Path, "*." + ext);

                //---------------Test Result -----------------------
                Assert.IsTrue(subMatchFiles.All(f => result.Contains(f)));
                Assert.IsFalse(result.Contains(unexpected));
                Assert.IsFalse(subNonMatchFiles.Any(f => result.Contains(f)));
            }
        }

        [Test]
        public void ListDirectoriesRecursive_GivenPath_ShouldReturnAllDirectoriesUnderThatPath()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var fileTree = CreateRandomFileTreeIn(folder.Path);
                var expected = fileTree
                                    .Where(p => Directory.Exists(Path.Combine(folder.Path, p)));
                var sut = Create();
                //---------------Assert Precondition----------------
                Assert.IsTrue(expected.All(f => Directory.Exists(Path.Combine(folder.Path, f))));

                //---------------Execute Test ----------------------
                var result = sut.ListDirectoriesRecursive(folder.Path);

                //---------------Test Result -----------------------
                CollectionAssert.AreEquivalent(expected, result);
            }
        }

        private IFileSystem Create()
        {
            return new LocalFileSystem();
        }
    }
}
