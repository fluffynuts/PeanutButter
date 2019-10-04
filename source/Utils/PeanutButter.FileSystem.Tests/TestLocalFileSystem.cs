using System;
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
// ReSharper disable AssignNullToNotNullAttribute

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

        [Test]
        public void Copy_GivenUnknownSource_ShouldThrowException()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var srcFolder = CreateRandomFolderIn(folder.Path);
                var dstFolder = CreateRandomFolderIn(folder.Path);
                var srcFile = Path.Combine(folder.Path, srcFolder, GetRandomString());
                var sut = Create();
                sut.SetCurrentDirectory(Path.Combine(folder.Path, dstFolder));

                //---------------Assert Precondition----------------
                var fullSourcePath = Path.Combine(folder.Path, srcFile);
                Assert.IsFalse(File.Exists(fullSourcePath));

                //---------------Execute Test ----------------------
                Assert.Throws<FileNotFoundException>(() => sut.Copy(fullSourcePath));

                //---------------Test Result -----------------------
            }
        }

        [Test]
        public void Copy_GivenSourceOnly_ShouldCopyIntoFileSystemCurrentFolder()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var srcFolder = CreateRandomFolderIn(folder.Path);
                var srcFile = CreateRandomFileIn(Path.Combine(folder.Path, srcFolder));
                var dstFolder = CreateRandomFolderIn(folder.Path);
                var sut = Create();
                sut.SetCurrentDirectory(Path.Combine(folder.Path, dstFolder));
                var expected = Path.Combine(folder.Path, dstFolder, Path.GetFileName(srcFile));
                var fullSourcePath = Path.Combine(folder.Path, srcFolder, srcFile);

                //---------------Assert Precondition----------------
                Assert.IsFalse(File.Exists(expected));

                //---------------Execute Test ----------------------
                sut.Copy(fullSourcePath);

                //---------------Test Result -----------------------
                Assert.IsTrue(File.Exists(expected), "File not found after copy");
            }
        }

        [Test]
        public void Copy_GivenSourceAndRelativeDestination_WhenDestinationDoesNotExistButDestinationFolderDoesExist_ShouldCopyIntoFolderWithProvidedName()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var srcFolder = CreateRandomFolderIn(folder.Path);
                var srcFile = CreateRandomFileIn(Path.Combine(folder.Path, srcFolder));
                var dstFolder = CreateRandomFolderIn(folder.Path);
                var dstFile = GetRandomString();
                var sut = Create();
                sut.SetCurrentDirectory(Path.Combine(folder.Path));
                var expected = Path.Combine(folder.Path, dstFolder, dstFile);
                var fullSourcePath = Path.Combine(folder.Path, srcFolder, srcFile);

                //---------------Assert Precondition----------------
                Assert.IsFalse(File.Exists(expected));

                //---------------Execute Test ----------------------
                sut.Copy(fullSourcePath, Path.Combine(dstFolder, dstFile));

                //---------------Test Result -----------------------
                Assert.IsTrue(File.Exists(expected), "File not found after copy");
                CollectionAssert.AreEqual(File.ReadAllBytes(fullSourcePath),
                                          File.ReadAllBytes(expected));
            }
        }

        [Test]
        public void Copy_GivenSourceFile_WhenDestinationDoesNotExistAndSupportingFoldersAreMissing_ShouldCreateThemAndCopy()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var srcFolder = CreateRandomFolderIn(folder.Path);
                var srcFile = CreateRandomFileIn(Path.Combine(folder.Path, srcFolder));
                var dstPath = Path.Combine(GetRandomCollection<string>(3,5).ToArray());
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var sourceFileFullPath = Path.Combine(folder.Path, srcFolder, srcFile);
                sut.Copy(sourceFileFullPath, dstPath);

                //---------------Test Result -----------------------
                Assert.IsTrue(File.Exists(Path.Combine(folder.Path, dstPath)));
                CollectionAssert.AreEqual(File.ReadAllBytes(sourceFileFullPath),
                                            File.ReadAllBytes(Path.Combine(folder.Path, dstPath)));
            }
        }

        [Test]
        public void CopyFile_GivenOnlySource_WhenPathIsRelativeInAnotherFolder_ShouldCopyToLocalFolder()
        {
            //---------------Set up test pack-------------------
            using (var root = new AutoTempFolder())
            {
                var localFolder = Path.Combine(root.Path, CreateRandomFolderIn(root.Path));
                var otherSub = CreateRandomFolderIn(root.Path);
                var otherFolder = Path.Combine(root.Path, otherSub);
                var fileName = CreateRandomFileIn(otherFolder);
                var source = Path.Combine("..", otherSub, fileName);
                var sut = Create();
                sut.SetCurrentDirectory(localFolder);
                var expectedPath = Path.Combine(localFolder, fileName);

                //---------------Assert Precondition----------------
                Assert.IsFalse(File.Exists(expectedPath));

                //---------------Execute Test ----------------------
                sut.Copy(source);

                //---------------Test Result -----------------------
                Assert.IsTrue(File.Exists(expectedPath));
                CollectionAssert.AreEqual(File.ReadAllBytes(Path.Combine(otherFolder, fileName)),
                    File.ReadAllBytes(expectedPath));
            }
        }

        [Test]
        public void OpenReader_GivenRelativePath_WhenFileDoesNotExist_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var fileName = GetRandomFileName();
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);

                //---------------Assert Precondition----------------
                Assert.IsFalse(File.Exists(Path.Combine(folder.Path, fileName)));

                //---------------Execute Test ----------------------
                var result = sut.OpenReader(fileName);

                //---------------Test Result -----------------------
                Assert.IsNull(result);
            }
        }

        [Test]
        public void OpenReader_GivenRelativePath_WhenFileExists_ShouldReturnStreamThatCanReadFileContents()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var fileName = CreateRandomFileIn(folder.Path);
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);
                var expected = File.ReadAllBytes(Path.Combine(folder.Path, fileName));

                //---------------Assert Precondition----------------
                Assert.IsTrue(File.Exists(Path.Combine(folder.Path, fileName)));

                //---------------Execute Test ----------------------
                using (var result = sut.OpenReader(fileName))
                {
                    //---------------Test Result -----------------------
                    var buffer = new byte[result.Length];
                    result.Read(buffer, 0, buffer.Length);
                    CollectionAssert.AreEqual(expected, buffer);
                }
            }
        }

        [Test]
        public void OpenReader_GivenAbsoluteRelativePath_WhenFileDoesNotExist_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            using (var otherFolder = new AutoTempFolder())
            {
                var fileName = GetRandomFileName();
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);

                //---------------Assert Precondition----------------
                Assert.IsFalse(File.Exists(Path.Combine(folder.Path, fileName)));

                //---------------Execute Test ----------------------
                var result = sut.OpenReader(Path.Combine(otherFolder.Path, fileName));

                //---------------Test Result -----------------------
                Assert.IsNull(result);
            }
        }

        [Test]
        public void OpenReader_GivenAbsolutePath_WhenFileExists_ShouldReturnStreamThatCanReadFileContents()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            using (var otherFolder = new AutoTempFolder())
            {
                var fileName = CreateRandomFileIn(folder.Path);
                var sut = Create();
                sut.SetCurrentDirectory(otherFolder.Path);
                var expected = File.ReadAllBytes(Path.Combine(folder.Path, fileName));

                //---------------Assert Precondition----------------
                Assert.IsTrue(File.Exists(Path.Combine(folder.Path, fileName)));

                //---------------Execute Test ----------------------
                using (var result = sut.OpenReader(Path.Combine(folder.Path, fileName)))
                {
                    //---------------Test Result -----------------------
                    var buffer = new byte[result.Length];
                    result.Read(buffer, 0, buffer.Length);
                    CollectionAssert.AreEqual(expected, buffer);
                }
            }
        }

        [Test]
        public void OpenWriter_GivenRelativePath_WhenOutputFolderExists_ShouldReturnWritableStreamToFile()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var targetFolder = CreateRandomFolderIn(folder.Path);
                var targetPath = Path.Combine(targetFolder, GetRandomFileName());
                var absolutePath = Path.Combine(folder.Path, targetPath);
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);
                var expected = GetRandomBytes();

                //---------------Assert Precondition----------------
                Assert.IsFalse(File.Exists(absolutePath));

                //---------------Execute Test ----------------------
                using (var stream = sut.OpenWriter(targetPath))
                {
                    stream.Write(expected, 0, expected.Length);
                }
                //---------------Test Result -----------------------
                var persisted = File.ReadAllBytes(absolutePath);
                CollectionAssert.AreEqual(expected, persisted);
            }
        }

        [Test]
        public void Open_GivenRelativePath_ShouldOpenReadWrite()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var targetFolder = CreateRandomFolderIn(folder.Path);
                var targetPath = Path.Combine(targetFolder, GetRandomFileName());
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);
                var original = GetRandomBytes();
                var append = GetRandomBytes();
                var absolutePath = Path.Combine(folder.Path, targetPath);
                File.WriteAllBytes(absolutePath, original);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using (var stream = sut.Open(targetPath))
                {
                    stream.ReadAllBytes();
                    stream.Position = stream.Length;
                    stream.Append(append);
                }

                //---------------Test Result -----------------------
                using (var stream = File.Open(absolutePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    var expected = original.And(append);
                    var inFile = stream.ReadAllBytes();
                    CollectionAssert.AreEqual(expected, inFile);
                }
            }
        }

        [Test]
        public void Move_GivenRelativeSourceAndDestination_WhenDestinationDoesNotExist_CanRenameToMove_ShouldRename()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var sourceFolder = CreateRandomFolderIn(folder.Path);
                var sourceFileName = CreateRandomFileIn(Path.Combine(folder.Path, sourceFolder));
                var sourceRelativePath = Path.Combine(sourceFolder, sourceFileName);
                var sourceAbsolutePath = Path.Combine(folder.Path, sourceRelativePath);
                var sourceData = File.ReadAllBytes(sourceAbsolutePath);
                var destinationFolder = GetAnother(sourceFolder);
                var target = Path.Combine(destinationFolder, GetRandomString());
                var targetAbsolutePath = Path.Combine(folder.Path, target);
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);

                //---------------Assert Precondition----------------
                Assert.IsFalse(File.Exists(targetAbsolutePath));

                //---------------Execute Test ----------------------
                sut.Move(sourceRelativePath, target);

                //---------------Test Result -----------------------
                Assert.IsFalse(File.Exists(sourceAbsolutePath));
                Assert.IsTrue(File.Exists(targetAbsolutePath));
                var resultData = File.ReadAllBytes(targetAbsolutePath);
                CollectionAssert.AreEqual(sourceData, resultData);
            }
        }

        [Test]
        public void Move_GivenRelativeSourceAndDestination_WhenDestinationExists_AndOverwriteLeftAsFalse_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var sourceFolder = CreateRandomFolderIn(folder.Path);
                var sourceFileName = CreateRandomFileIn(Path.Combine(folder.Path, sourceFolder));
                var sourceRelativePath = Path.Combine(sourceFolder, sourceFileName);
                var sourceAbsolutePath = Path.Combine(folder.Path, sourceRelativePath);
                File.ReadAllBytes(sourceAbsolutePath);
                var destinationFolder = GetAnother(sourceFolder);
                var target = Path.Combine(destinationFolder, GetRandomString());
                var targetAbsolutePath = Path.Combine(folder.Path, target);
                Directory.CreateDirectory(Path.GetDirectoryName(targetAbsolutePath));
                File.WriteAllBytes(targetAbsolutePath, GetRandomBytes());
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);

                //---------------Assert Precondition----------------
                Assert.IsTrue(File.Exists(targetAbsolutePath));

                //---------------Execute Test ----------------------
                Assert.Throws<IOException>(() => sut.Move(sourceRelativePath, target));

                //---------------Test Result -----------------------
            }
        }


        [Test]
        public void Move_GivenRelativeSourceAndDestination_WhenDestinationExists_AndOverwriteSetTrue_ShouldOverwrite()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var sourceFolder = CreateRandomFolderIn(folder.Path);
                var sourceFileName = CreateRandomFileIn(Path.Combine(folder.Path, sourceFolder));
                var sourceRelativePath = Path.Combine(sourceFolder, sourceFileName);
                var sourceAbsolutePath = Path.Combine(folder.Path, sourceRelativePath);
                var sourceData = File.ReadAllBytes(sourceAbsolutePath);
                var destinationFolder = GetAnother(sourceFolder);
                var target = Path.Combine(destinationFolder, GetRandomString());
                var targetAbsolutePath = Path.Combine(folder.Path, target);
                Directory.CreateDirectory(Path.GetDirectoryName(targetAbsolutePath));
                File.WriteAllBytes(targetAbsolutePath, GetRandomBytes());
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);

                //---------------Assert Precondition----------------
                Assert.IsTrue(File.Exists(targetAbsolutePath));

                //---------------Execute Test ----------------------
                Assert.DoesNotThrow(() => sut.Move(sourceRelativePath, target, true));

                //---------------Test Result -----------------------
                Assert.IsFalse(File.Exists(sourceAbsolutePath));
                Assert.IsTrue(File.Exists(targetAbsolutePath));
                var resultData = File.ReadAllBytes(targetAbsolutePath);
                CollectionAssert.AreEqual(sourceData, resultData);
            }
        }


        [Test]
        [Explicit("Environment-specific: works if you have a remote machine called 'speedy' with a share called 'move_test' :D")]
        public void Move_GivenRelativeSourceAndDestination_WhenDestinationExistsOnAnotherVolume_AndOverwriteSetTrue_ShouldOverwrite()
        {
            //---------------Set up test pack-------------------
            using (var folder = new AutoTempFolder())
            {
                var sourceFolder = CreateRandomFolderIn(folder.Path);
                var sourceFileName = CreateRandomFileIn(Path.Combine(folder.Path, sourceFolder));
                var sourceRelativePath = Path.Combine(sourceFolder, sourceFileName);
                var sourceAbsolutePath = Path.Combine(folder.Path, sourceRelativePath);
                var sourceData = File.ReadAllBytes(sourceAbsolutePath);
                var destinationFolder = "\\\\speedy\\move_test";
                var target = Path.Combine(destinationFolder, GetRandomString());
                var targetAbsolutePath = Path.Combine(folder.Path, target);
                Directory.CreateDirectory(Path.GetDirectoryName(targetAbsolutePath));
                File.WriteAllBytes(targetAbsolutePath, GetRandomBytes());
                var sut = Create();
                sut.SetCurrentDirectory(folder.Path);

                //---------------Assert Precondition----------------
                Assert.IsTrue(File.Exists(targetAbsolutePath));

                //---------------Execute Test ----------------------
                Assert.DoesNotThrow(() => sut.Move(sourceRelativePath, target, true));

                //---------------Test Result -----------------------
                Assert.IsFalse(File.Exists(sourceAbsolutePath));
                Assert.IsTrue(File.Exists(targetAbsolutePath));
                var resultData = File.ReadAllBytes(targetAbsolutePath);
                CollectionAssert.AreEqual(sourceData, resultData);
            }
        }

        [Test]
        [Explicit("Discovery: Testing share code")]
        public void CreateShare()
        {
            //---------------Set up test pack-------------------
            var folder = Path.Combine("C:\\tmp", GetRandomString());
            Directory.CreateDirectory(folder);
            var shareName = CreateTemporaryShareForFolder(folder);
            Console.WriteLine(shareName);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
        }

        [Test]
        [Explicit("Discovery: Testing share code")]
        public void DeleteShare()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var share = WindowsShare.GetShareByName("otno5553");
            share.Delete();

            //---------------Test Result -----------------------
        }



        private string CreateTemporaryShareForFolder(string folder)
        {
            var shareName = GetRandomString(5,10);
            WindowsShare.Create(folder, shareName, WindowsShare.ShareType.DiskDrive, 2, "Temporary share for testing", null);
            return shareName;
        }



        private IFileSystem Create()
        {
            return new LocalFileSystem();
        }
    }
}
