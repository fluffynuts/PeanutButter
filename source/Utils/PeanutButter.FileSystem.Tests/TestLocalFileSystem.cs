using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;
// ReSharper disable ObjectCreationAsStatement
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable AccessToDisposedClosure

namespace PeanutButter.FileSystem.Tests
{
    [TestFixture]
    public class TestLocalFileSystem
    {
        [Test]
        public void GetCurrentDirectory_WhenNoDirectorySet_ShouldReturnSameDirectoryAsOuterScope()
        {
            //---------------Set up test pack-------------------
            var expected = Directory.GetCurrentDirectory();
            var sut = Create();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.GetCurrentDirectory();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SetCurrentDirectory_ShouldStoreCurrentDirectoryAndReturnViaGetCurrentDirectory()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var sut = Create();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                sut.SetCurrentDirectory(folder.Path);
                var result = sut.GetCurrentDirectory();

                //---------------Test Result -----------------------
                Assert.AreEqual(folder.Path, result);
            }
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
        public void List_GivenNoArguments_ShouldListAllFilesAndDirectoriesInCurrentPath()
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
                sut.SetCurrentDirectory(folder.Path);

                //---------------Assert Precondition----------------
                Assert.IsTrue(Directory.Exists(Path.Combine(folder.Path, sub1)));
                Assert.IsTrue(Directory.Exists(Path.Combine(folder.Path, sub1)));
                Assert.IsTrue(File.Exists(Path.Combine(folder.Path, file1)));
                Assert.IsTrue(File.Exists(Path.Combine(folder.Path, file2)));

                //---------------Execute Test ----------------------
                var result = sut.List();

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
                sut.SetCurrentDirectory(folder.Path);

                //---------------Assert Precondition----------------
                Assert.IsTrue(Directory.Exists(Path.Combine(folder.Path, matchDirectory)));
                Assert.IsTrue(Directory.Exists(Path.Combine(folder.Path, nonMatchDirectory)));
                Assert.IsTrue(File.Exists(Path.Combine(folder.Path, matchFile)));
                Assert.IsTrue(File.Exists(Path.Combine(folder.Path, nonMatchFile)));

                //---------------Execute Test ----------------------
                var result = sut.List(matchPrefix + "*");

                //---------------Test Result -----------------------
                CollectionAssert.Contains(result, matchDirectory);
                CollectionAssert.Contains(result, matchFile);
                CollectionAssert.DoesNotContain(result, nonMatchDirectory);
                CollectionAssert.DoesNotContain(result, nonMatchFile);
            }
        }

        [Test]
        public void List_FileExtensionMatch_ShouldFindMatchingFiles()
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
                sut.SetCurrentDirectory(folder.Path);
                //---------------Assert Precondition----------------
                Assert.IsTrue(matchFiles.Union(nonMatchFiles).All(f => File.Exists(Path.Combine(folder.Path, f))));

                //---------------Execute Test ----------------------
                var result = sut.List("*." + ext);

                //---------------Test Result -----------------------
                Assert.IsTrue(matchFiles.All(f => result.Contains(f)));
                Assert.IsFalse(nonMatchFiles.Any(f => result.Contains(f)));
            }
        }

        [Test]
        public void ListRecursive_ShouldReturnAllFilesAndDirectoriesUnderThatPath()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var expected = CreateRandomFileTreeIn(folder.Path);
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.ListRecursive();

                //---------------Test Result -----------------------
                CollectionAssert.AreEquivalent(expected, result);
            }
        }

        [Test]
        public void ListRecursive_AndSearchPattern_ShouldReturnAllFilesAndDirectoriesUnderThatPathWhichMatchTheSearchPattern()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                CreateRandomFileTreeIn(folder.Path);
                var search = "*a*";
                var expected = Directory.EnumerateFileSystemEntries(folder.Path, "*a*", SearchOption.AllDirectories)
                                    .Select(p => p.Substring(folder.Path.Length + 1));
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.ListRecursive(search);

                //---------------Test Result -----------------------
                CollectionAssert.AreEquivalent(expected, result);
            }
        }

        [Test]
        public void ListFiles_GivenNoArguments_ShouldReturnOnlyFilesFromPath()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var expected = GetRandomString();
                var unexpected = GetAnother(expected);
                Directory.CreateDirectory(Path.Combine(folder.Path, unexpected));
                File.WriteAllBytes(Path.Combine(folder.Path, expected), GetRandomBytes());
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.ListFiles();

                //---------------Test Result -----------------------
                CollectionAssert.Contains(result, expected);
                CollectionAssert.DoesNotContain(result, unexpected);
            }
        }

        [Test]
        public void ListFiles_SearchPattern_ShouldReturnListOfMatchingFilesInPath()
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
                sut.SetCurrentDirectory(folder.Path);
                //---------------Assert Precondition----------------
                Assert.IsTrue(matchFiles.Union(nonMatchFiles).All(f => File.Exists(Path.Combine(folder.Path, f))));

                //---------------Execute Test ----------------------
                var result = sut.ListFiles("*." + ext);

                //---------------Test Result -----------------------
                Assert.IsTrue(matchFiles.All(f => result.Contains(f)));
                Assert.IsFalse(result.Contains(unexpected));
                Assert.IsFalse(nonMatchFiles.Any(f => result.Contains(f)));
            }
        }

        [Test]
        public void ListDirectories_GivenNoArguments_ShouldReturnOnlyDirectoriesFromPath()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var expected = GetRandomString();
                var unexpected = GetAnother(expected);
                Directory.CreateDirectory(Path.Combine(folder.Path, expected));
                File.WriteAllBytes(Path.Combine(folder.Path, unexpected), GetRandomBytes());
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.ListDirectories();

                //---------------Test Result -----------------------
                CollectionAssert.Contains(result, expected);
                CollectionAssert.DoesNotContain(result, unexpected);
            }
        }

        [Test]
        public void ListDirectories_GivenSearchPattern_ShouldReturnListOfMatchingDirectoriesInPath()
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
                sut.SetCurrentDirectory(folder.Path);
                //---------------Assert Precondition----------------
                Assert.IsTrue(matchDirectories.Union(nonMatchDirectories).All(f => Directory.Exists(Path.Combine(folder.Path, f))));

                //---------------Execute Test ----------------------
                var result = sut.ListDirectories("*." + ext);

                //---------------Test Result -----------------------
                Assert.IsTrue(matchDirectories.All(f => result.Contains(f)));
                Assert.IsFalse(result.Contains(unexpected));
                Assert.IsFalse(nonMatchDirectories.Any(f => result.Contains(f)));
            }
        }

        [Test]
        public void ListFilesRecursive_ShouldReturnOnlyFilesFromPathAndBelow()
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
                sut.SetCurrentDirectory(folder.Path);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.ListFilesRecursive();

                //---------------Test Result -----------------------
                CollectionAssert.Contains(result, expected);
                CollectionAssert.Contains(result, expected2);
                CollectionAssert.DoesNotContain(result, unexpected);
            }
        }

        [Test]
        public void ListFilesRecursive_SearchPattern_ShouldReturnListOfMatchingFilesInPath()
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
                sut.SetCurrentDirectory(folder.Path);
                //---------------Assert Precondition----------------
                Assert.IsTrue(subMatchFiles.Union(subNonMatchFiles)
                                .All(f => File.Exists(Path.Combine(folder.Path, f))));

                //---------------Execute Test ----------------------
                var result = sut.ListFilesRecursive("*." + ext);

                //---------------Test Result -----------------------
                Assert.IsTrue(subMatchFiles.All(f => result.Contains(f)));
                Assert.IsFalse(result.Contains(unexpected));
                Assert.IsFalse(subNonMatchFiles.Any(f => result.Contains(f)));
            }
        }

        [Test]
        public void ListDirectoriesRecursive_GivenNoArguments_ShouldReturnAllDirectoriesUnderThatPath()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var fileTree = CreateRandomFileTreeIn(folder.Path);
                var expected = fileTree
                                    .Where(p => Directory.Exists(Path.Combine(folder.Path, p)));
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);
                //---------------Assert Precondition----------------
                Assert.IsTrue(expected.All(f => Directory.Exists(Path.Combine(folder.Path, f))));

                //---------------Execute Test ----------------------
                var result = sut.ListDirectoriesRecursive();

                //---------------Test Result -----------------------
                CollectionAssert.AreEquivalent(expected, result);
            }
        }

        [Test]
        public void Delete_GivenUnknownPath_ShouldDoNothing()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                //---------------Assert Precondition----------------
                var fileName = GetRandomString();
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);
                //---------------Execute Test ----------------------

                Assert.DoesNotThrow(() => sut.Delete(fileName));

                //---------------Test Result -----------------------
            }
        }

        [Test]
        public void Delete_GivenExistingFileName_ShouldDeleteIt()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var fileName = CreateRandomFileIn(folder.Path);
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);
                //---------------Assert Precondition----------------
                var fullPath = Path.Combine(folder.Path, fileName);
                Assert.IsTrue(File.Exists(fullPath));

                //---------------Execute Test ----------------------
                sut.Delete(fileName);

                //---------------Test Result -----------------------
                Assert.IsFalse(File.Exists(fullPath));
            }
        }

        [Test]
        public void Delete_GivenExistingEmptyFolderPath_ShouldDeleteIt()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var folderName = CreateRandomFolderIn(folder.Path);
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);

                //---------------Assert Precondition----------------
                var fullPath = Path.Combine(folder.Path, folderName);
                Assert.IsTrue(Directory.Exists(fullPath));

                //---------------Execute Test ----------------------
                sut.Delete(folderName);

                //---------------Test Result -----------------------
                Assert.IsFalse(Directory.Exists(fullPath));
            }
        }

        [Test]
        public void Delete_GivenFolderPathContainingFile_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var folderName = CreateRandomFolderIn(folder.Path);
                CreateRandomFileIn(Path.Combine(folder.Path, folderName));
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);
                var fullPath = Path.Combine(folder.Path, folderName);

                //---------------Assert Precondition----------------
                Assert.IsTrue(Directory.Exists(fullPath));
                Assert.IsTrue(Directory.EnumerateFiles(fullPath).Any());

                //---------------Execute Test ----------------------
                Assert.Throws<IOException>(() => sut.Delete(folderName));

                //---------------Test Result -----------------------
                Assert.IsTrue(Directory.Exists(fullPath));
                Assert.IsTrue(Directory.EnumerateFiles(fullPath).Any());
            }
        }

        [Test]
        public void Delete_GivenFolderPathContainingFolder_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var folderName = CreateRandomFolderIn(folder.Path);
                CreateRandomFolderIn(Path.Combine(folder.Path, folderName));
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);
                var fullPath = Path.Combine(folder.Path, folderName);

                //---------------Assert Precondition----------------
                Assert.IsTrue(Directory.Exists(fullPath));
                Assert.IsTrue(Directory.EnumerateDirectories(fullPath).Any());

                //---------------Execute Test ----------------------
                Assert.Throws<IOException>(() => sut.Delete(folderName));

                //---------------Test Result -----------------------
                Assert.IsTrue(Directory.Exists(fullPath));
                Assert.IsTrue(Directory.EnumerateDirectories(fullPath).Any());
            }
        }

        [Test]
        public void DeleteRecursive_GivenUnknownPath_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var toDelete = GetRandomString();
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);

                //---------------Assert Precondition----------------
                var fullPath = Path.Combine(folder.Path, toDelete);
                Assert.IsFalse(File.Exists(fullPath));
                Assert.IsFalse(Directory.Exists(fullPath));

                //---------------Execute Test ----------------------
                Assert.DoesNotThrow(() => sut.DeleteRecursive(toDelete));

                //---------------Test Result -----------------------
            }
        }

        [Test]
        public void DeleteRecursive_GivenPathOfFile_ShouldDeleteIt()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var toDelete = CreateRandomFileIn(folder.Path);
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);

                //---------------Assert Precondition----------------
                var fullPath = Path.Combine(folder.Path, toDelete);
                Assert.IsTrue(File.Exists(fullPath));

                //---------------Execute Test ----------------------
                Assert.DoesNotThrow(() => sut.DeleteRecursive(toDelete));

                //---------------Test Result -----------------------
                Assert.IsFalse(File.Exists(fullPath));
            }
        }


        [Test]
        public void DeleteRecursive_GivenPathOfEmptyFolder_ShouldDeleteIt()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var toDelete = CreateRandomFolderIn(folder.Path);
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);

                //---------------Assert Precondition----------------
                var fullPath = Path.Combine(folder.Path, toDelete);
                Assert.IsTrue(Directory.Exists(fullPath));

                //---------------Execute Test ----------------------
                Assert.DoesNotThrow(() => sut.DeleteRecursive(toDelete));

                //---------------Test Result -----------------------
                Assert.IsFalse(Directory.Exists(fullPath));
            }
        }


        [Test]
        public void DeleteRecursive_GivenPathOfNonEmptyFolder_ShouldDeleteIt()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var toDelete = CreateRandomFolderIn(folder.Path);
                CreateRandomFileTreeIn(Path.Combine(folder.Path, toDelete));
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);

                //---------------Assert Precondition----------------
                var fullPath = Path.Combine(folder.Path, toDelete);
                Assert.IsTrue(Directory.Exists(fullPath));

                //---------------Execute Test ----------------------
                Assert.DoesNotThrow(() => sut.DeleteRecursive(toDelete));

                //---------------Test Result -----------------------
                Assert.IsFalse(Directory.Exists(fullPath));
            }
        }


        private IFileSystem Create()
        {
            return new LocalFileSystem();
        }
    }
}
