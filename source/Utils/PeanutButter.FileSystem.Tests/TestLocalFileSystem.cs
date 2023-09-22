using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using NSubstitute;
using static NExpect.Expectations;

// ReSharper disable ObjectCreationAsStatement
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable AccessToDisposedClosure
// ReSharper disable AssignNullToNotNullAttribute

namespace PeanutButter.FileSystem.Tests
{
    [TestFixture]
    public class TestLocalFileSystem
    {
        [TestFixture]
        public class GetCurrentDirectory
        {
            [TestFixture]
            public class WhenNoDirectorySet
            {
                [Test]
                public void ShouldReturnSameDirectoryAsOuterScope()
                {
                    //---------------Set up test pack-------------------
                    var expected = Directory.GetCurrentDirectory();
                    var sut = Create();

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = sut.GetCurrentDirectory();

                    //---------------Test Result -----------------------
                    Expect(result)
                        .To.Equal(expected);
                }
            }
        }

        [TestFixture]
        public class SetCurrentDirectory
        {
            [Test]
            public void ShouldStoreCurrentDirectoryAndReturnViaGetCurrentDirectory()
            {
                //---------------Set up test pack-------------------
                using var folder = new AutoTempFolder();
                var sut = Create();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                sut.SetCurrentDirectory(folder.Path);
                var result = sut.GetCurrentDirectory();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(folder.Path);
            }
        }


        [Test]
        public void Type_ShouldImplement_IFileSystem()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(LocalFileSystem);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(sut)
                .To.Implement<IFileSystem>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => new LocalFileSystem())
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        [TestFixture]
        public class List
        {
            [TestFixture]
            public class GivenNoArguments
            {
                [Test]
                public void ShouldListAllFilesAndDirectoriesInCurrentPath()
                {
                    //---------------Set up test pack-------------------
                    using var folder = new AutoTempFolder();
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
                    Expect(Path.Combine(folder.Path, sub1))
                        .To.Be.A.Folder();
                    Expect(Path.Combine(folder.Path, sub2))
                        .To.Be.A.Folder();
                    Expect(Path.Combine(folder.Path, file1))
                        .To.Be.A.File();
                    Expect(Path.Combine(folder.Path, file2))
                        .To.Be.A.File();

                    //---------------Execute Test ----------------------
                    var result = sut.List();

                    //---------------Test Result -----------------------

                    Expect(result)
                        .To.Be.Equivalent.To(
                            new[]
                            {
                                sub1,
                                sub2,
                                file1,
                                file2
                            }
                        );
                }
            }

            [TestFixture]
            public class GivenSearchPattern
            {
                [Test]
                public void ShouldReturnOnlyMatchingDirectoriesAndFiles()
                {
                    //---------------Set up test pack-------------------
                    using var folder = new AutoTempFolder();
                    var matchPrefix = GetRandomString(3, 5);
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
                    Expect(Path.Combine(folder.Path, matchDirectory))
                        .To.Be.A.Folder();
                    Expect(Path.Combine(folder.Path, nonMatchDirectory))
                        .To.Be.A.Folder();
                    Expect(Path.Combine(folder.Path, matchFile))
                        .To.Be.A.File();
                    Expect(Path.Combine(folder.Path, nonMatchFile))
                        .To.Be.A.File();

                    //---------------Execute Test ----------------------
                    var result = sut.List(matchPrefix + "*");

                    //---------------Test Result -----------------------
                    Expect(result)
                        .Not.To.Contain(nonMatchDirectory)
                        .And
                        .Not.To.Contain(nonMatchFile)
                        .And
                        .To.Contain(matchDirectory)
                        .And
                        .To.Contain(matchFile);
                }
            }

            [TestFixture]
            public class WhenHaveFileExtensionMatch
            {
                [Test]
                public void ShouldFindMatchingFiles()
                {
                    //---------------Set up test pack-------------------
                    using var folder = new AutoTempFolder();
                    var ext = GetRandomString(3, 3);
                    var otherExt = GetAnother(ext, () => GetRandomString(3, 3));
                    var matchFiles = GetRandomCollection(() => GetRandomString(2, 4) + "." + ext, 3, 5);
                    var nonMatchFiles = GetRandomCollection(() => GetRandomString(2, 4) + "." + otherExt, 3, 5);
                    matchFiles.Union(nonMatchFiles).ForEach(
                        f =>
                            File.WriteAllBytes(Path.Combine(folder.Path, f), GetRandomBytes())
                    );
                    var sut = Create();
                    sut.SetCurrentDirectory(folder.Path);
                    //---------------Assert Precondition----------------
                    Assert.IsTrue(matchFiles.Union(nonMatchFiles).All(f => File.Exists(Path.Combine(folder.Path, f))));

                    //---------------Execute Test ----------------------
                    var result = sut.List("*." + ext);

                    //---------------Test Result -----------------------
                    Expect(result)
                        .To.Contain.All.Of(matchFiles);
                    Expect(result)
                        .Not.To.Contain.Any.Of(nonMatchFiles);
                }
            }
        }

        [TestFixture]
        public class ListRecursive
        {
            [Test]
            public void ShouldReturnAllFilesAndDirectoriesUnderCurrentPath()
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
                    Expect(result)
                        .To.Be.Equivalent.To(expected);
                }
            }

            [TestFixture]
            public class GivenSearchPattern
            {
                [Test]
                public void ShouldReturnAllFilesAndDirectoriesUnderThatPathWhichMatchTheSearchPattern()
                {
                    //---------------Set up test pack-------------------
                    using var folder = new AutoTempFolder();
                    CreateRandomFileTreeIn(folder.Path);
                    var search = "*a*";
                    var expected = Directory.EnumerateFileSystemEntries(
                            folder.Path,
                            "*a*",
                            SearchOption.AllDirectories
                        )
                        .Select(p => p.Substring(folder.Path.Length + 1));
                    var sut = Create();
                    sut.SetCurrentDirectory(folder.Path);
                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = sut.ListRecursive(search);

                    //---------------Test Result -----------------------
                    Expect(result)
                        .To.Be.Equivalent.To(expected);
                }
            }
        }

        [TestFixture]
        public class ListFiles
        {
            [Test]
            public void ShouldReturnOnlyFilesFromPath()
            {
                //---------------Set up test pack-------------------
                using var folder = new AutoTempFolder();
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
                Expect(result)
                    .To.Contain(expected)
                    .And
                    .Not.To.Contain(unexpected);
            }

            [TestFixture]
            public class GivenSearchPattern
            {
                [Test]
                public void ShouldReturnListOfMatchingFilesInPath()
                {
                    //---------------Set up test pack-------------------
                    using var folder = new AutoTempFolder();
                    var ext = GetRandomString(3, 3);
                    var otherExt = GetAnother(ext, () => GetRandomString(3, 3));
                    var matchFiles = GetRandomCollection(() => GetRandomString(2, 4) + "." + ext, 3, 5);
                    var nonMatchFiles = GetRandomCollection(() => GetRandomString(2, 4) + "." + otherExt, 3, 5);
                    matchFiles.Union(nonMatchFiles).ForEach(
                        f =>
                            File.WriteAllBytes(Path.Combine(folder.Path, f), GetRandomBytes())
                    );
                    var unexpected = GetAnother(matchFiles, () => GetRandomString(2, 4) + "." + ext);
                    Directory.CreateDirectory(Path.Combine(folder.Path, unexpected));
                    var sut = Create();
                    sut.SetCurrentDirectory(folder.Path);
                    //---------------Assert Precondition----------------
                    Assert.IsTrue(matchFiles.Union(nonMatchFiles).All(f => File.Exists(Path.Combine(folder.Path, f))));

                    //---------------Execute Test ----------------------
                    var result = sut.ListFiles("*." + ext);

                    //---------------Test Result -----------------------
                    Expect(result)
                        .To.Contain.All.Of(matchFiles);
                    Expect(result)
                        .Not.To.Contain(unexpected);
                    Expect(result)
                        .To.Contain.None.Of(nonMatchFiles);
                }
            }
        }

        [TestFixture]
        public class ListDirectories
        {
            [Test]
            public void ShouldReturnOnlyDirectoriesFromPath()
            {
                //---------------Set up test pack-------------------
                using var folder = new AutoTempFolder();
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
                Expect(result)
                    .To.Contain(expected);
                Expect(result)
                    .Not.To.Contain(unexpected);
            }

            [TestFixture]
            public class GivenSearchPattern
            {
                [Test]
                public void ShouldReturnListOfMatchingDirectoriesInPath()
                {
                    //---------------Set up test pack-------------------
                    using var folder = new AutoTempFolder();
                    var ext = GetRandomString(3, 3);
                    var otherExt = GetAnother(ext, () => GetRandomString(3, 3));
                    var matchDirectories = GetRandomCollection(() => GetRandomString(2, 4) + "." + ext, 3, 5);
                    var nonMatchDirectories = GetRandomCollection(
                        () => GetRandomString(2, 4) + "." + otherExt,
                        3,
                        5
                    );
                    matchDirectories.Union(nonMatchDirectories).ForEach(
                        f =>
                            Directory.CreateDirectory(Path.Combine(folder.Path, f))
                    );
                    var unexpected = GetAnother(matchDirectories, () => GetRandomString(2, 4) + "." + ext);
                    File.WriteAllBytes(Path.Combine(folder.Path, unexpected), GetRandomBytes());
                    var sut = Create();
                    sut.SetCurrentDirectory(folder.Path);
                    //---------------Assert Precondition----------------
                    var absolutePaths = matchDirectories.Union(nonMatchDirectories)
                        .Select(p => Path.Combine(folder.Path, p))
                        .ToArray();
                    foreach (var p in absolutePaths)
                    {
                        Expect(p)
                            .To.Be.A.Folder();
                    }

                    //---------------Execute Test ----------------------
                    var result = sut.ListDirectories("*." + ext);

                    //---------------Test Result -----------------------

                    Expect(result)
                        .To.Contain.All.Of(matchDirectories);
                    Expect(result)
                        .Not.To.Contain(unexpected);
                    Expect(result)
                        .Not.To.Contain.Any.Of(nonMatchDirectories);
                }
            }
        }

        [TestFixture]
        public class ListFilesRecursive
        {
            [Test]
            public void ShouldReturnOnlyFilesFromPathAndBelow()
            {
                //---------------Set up test pack-------------------
                using var folder = new AutoTempFolder();
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
                Expect(result)
                    .To.Be.Equivalent.To(
                        new[]
                        {
                            expected,
                            expected2
                        }
                    );
            }

            [TestFixture]
            public class GivenSearchPattern
            {
                [Test]
                public void ShouldReturnListOfMatchingFilesInPath()
                {
                    //---------------Set up test pack-------------------
                    using (var folder = new AutoTempFolder())
                    {
                        var ext = GetRandomString(3, 3);
                        var otherExt = GetAnother(ext, () => GetRandomString(3, 3));
                        var matchFiles = GetRandomCollection(() => GetRandomString(2, 4) + "." + ext, 3, 5);
                        var nonMatchFiles = GetRandomCollection(() => GetRandomString(2, 4) + "." + otherExt, 3, 5);
                        var subMatchFiles = new List<string>();
                        matchFiles.ForEach(
                            f =>
                            {
                                var subDirectory = GetRandomString();
                                Directory.CreateDirectory(Path.Combine(folder.Path, subDirectory));
                                var subPath = Path.Combine(subDirectory, f);
                                subMatchFiles.Add(subPath);
                                File.WriteAllBytes(Path.Combine(folder.Path, subPath), GetRandomBytes());
                            }
                        );
                        var subNonMatchFiles = new List<string>();
                        nonMatchFiles.ForEach(
                            f =>
                            {
                                var subDirectory = GetRandomString();
                                Directory.CreateDirectory(Path.Combine(folder.Path, subDirectory));
                                var subPath = Path.Combine(subDirectory, f);
                                subNonMatchFiles.Add(subPath);
                                File.WriteAllBytes(Path.Combine(folder.Path, subPath), GetRandomBytes());
                            }
                        );
                        var unexpected = GetAnother(matchFiles, () => GetRandomString(2, 4) + "." + ext);
                        Directory.CreateDirectory(Path.Combine(folder.Path, unexpected));
                        var sut = Create();
                        sut.SetCurrentDirectory(folder.Path);
                        //---------------Assert Precondition----------------
                        var absolutePaths = subMatchFiles.Union(subNonMatchFiles)
                            .Select(f => File.Exists(Path.Combine(folder.Path, f)))
                            .ToArray();

                        //---------------Execute Test ----------------------
                        var result = sut.ListFilesRecursive("*." + ext);


                        Expect(result)
                            .To.Be.Equivalent.To(subMatchFiles);
                    }
                }
            }
        }

        [TestFixture]
        public class ListDirectoriesRecursive
        {
            [Test]
            public void ShouldReturnAllDirectoriesUnderThatPath()
            {
                //---------------Set up test pack-------------------
                using var folder = new AutoTempFolder();
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

        [TestFixture]
        public class Delete
        {
            [TestFixture]
            public class GivenUnknownPath
            {
                [Test]
                public void ShouldDoNothing()
                {
                    //---------------Set up test pack-------------------
                    using var folder = new AutoTempFolder();
                    //---------------Assert Precondition----------------
                    var fileName = GetRandomString();
                    var sut = Create();
                    sut.SetCurrentDirectory(folder.Path);
                    //---------------Execute Test ----------------------

                    Assert.DoesNotThrow(() => sut.Delete(fileName));

                    //---------------Test Result -----------------------
                }
            }

            [TestFixture]
            public class GivenExistingFileName
            {
                [Test]
                public void ShouldDeleteIt()
                {
                    //---------------Set up test pack-------------------
                    using var folder = new AutoTempFolder();
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

            [TestFixture]
            public class GivenExistingEmptyFolderPath
            {
                [Test]
                public void ShouldDeleteIt()
                {
                    //---------------Set up test pack-------------------
                    using var folder = new AutoTempFolder();
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

            [TestFixture]
            public class GivenFolderPathContainingFile
            {
                [Test]
                public void ShouldThrow()
                {
                    //---------------Set up test pack-------------------
                    using var folder = new AutoTempFolder();
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

            [TestFixture]
            public class GivenPathContainingFolder
            {
                [Test]
                public void ShouldThrow()
                {
                    //---------------Set up test pack-------------------
                    using var folder = new AutoTempFolder();
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
        }

        [TestFixture]
        public class DeleteRecursive
        {
            [TestFixture]
            public class GivenUnknownPath
            {
                [Test]
                public void ShouldNotThrow()
                {
                    //---------------Set up test pack-------------------
                    using var folder = new AutoTempFolder();
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

            [TestFixture]
            public class GivenPathToFile
            {
                [Test]
                public void ShouldDeleteIt()
                {
                    //---------------Set up test pack-------------------
                    using var folder = new AutoTempFolder();
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


            [TestFixture]
            public class GivenPathToEmptyFolder
            {
                [Test]
                public void ShouldDeleteIt()
                {
                    //---------------Set up test pack-------------------
                    using var folder = new AutoTempFolder();
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


            [TestFixture]
            public class GivenPathOfNonEmptyFolder
            {
                [Test]
                public void ShouldDeleteIt()
                {
                    //---------------Set up test pack-------------------
                    using var folder = new AutoTempFolder();
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
        }

        [TestFixture]
        public class Copy
        {
            [TestFixture]
            public class GivenUnknownSource
            {
                [Test]
                public void ShouldThrowException()
                {
                    //---------------Set up test pack-------------------
                    using var folder = new AutoTempFolder();
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

            [TestFixture]
            public class GivenSourceOnly
            {
                [Test]
                public void ShouldCopyIntoFileSystemCurrentFolder()
                {
                    //---------------Set up test pack-------------------
                    using var folder = new AutoTempFolder();
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

                [TestFixture]
                public class WhenPathIsRelativeInAnotherFolder
                {
                    [Test]
                    public void ShouldCopyToLocalFolder()
                    {
                        //---------------Set up test pack-------------------
                        using var root = new AutoTempFolder();
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
                        CollectionAssert.AreEqual(
                            File.ReadAllBytes(Path.Combine(otherFolder, fileName)),
                            File.ReadAllBytes(expectedPath)
                        );
                    }
                }
            }

            [TestFixture]
            public class GivenSourceAndDestination
            {
                [TestFixture]
                public class WhenDestinationFileDoesNotExist
                {
                    [TestFixture]
                    public class ButDestinationFolderDoesExist
                    {
                        [Test]
                        public void ShouldCopyIntoFolderWithProvidedName()
                        {
                            //---------------Set up test pack-------------------
                            using var folder = new AutoTempFolder();
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
                            CollectionAssert.AreEqual(
                                File.ReadAllBytes(fullSourcePath),
                                File.ReadAllBytes(expected)
                            );
                        }
                    }
                }

                [TestFixture]
                public class WhenDestinationDoesNotExist
                {
                    [TestFixture]
                    public class AndSupportingFoldersAreMissing
                    {
                        [Test]
                        public void ShouldCreateThemAndCopy()
                        {
                            //---------------Set up test pack-------------------
                            using var folder = new AutoTempFolder();
                            var srcFolder = CreateRandomFolderIn(folder.Path);
                            var srcFile = CreateRandomFileIn(Path.Combine(folder.Path, srcFolder));
                            var dstPath = Path.Combine(GetRandomCollection<string>(3, 5).ToArray());
                            var sut = Create();
                            sut.SetCurrentDirectory(folder.Path);

                            //---------------Assert Precondition----------------

                            //---------------Execute Test ----------------------
                            var sourceFileFullPath = Path.Combine(folder.Path, srcFolder, srcFile);
                            sut.Copy(sourceFileFullPath, dstPath);

                            //---------------Test Result -----------------------
                            Assert.IsTrue(File.Exists(Path.Combine(folder.Path, dstPath)));
                            CollectionAssert.AreEqual(
                                File.ReadAllBytes(sourceFileFullPath),
                                File.ReadAllBytes(Path.Combine(folder.Path, dstPath))
                            );
                        }
                    }
                }

                [TestFixture]
                public class WhenSourceIsFolder
                {
                    [Test]
                    public void ShouldCopyRecursively()
                    {
                        // Arrange
                        using var tempFolder = new AutoTempFolder();
                        tempFolder.CreateFolder("source/level1/level2");
                        tempFolder.WriteFile("source/readme.md", "# README");
                        tempFolder.WriteFile("source/level1/index.js", "console.log('foo');");
                        tempFolder.WriteFile("source/level2/data.json", "{ \"id\": 1, \"name\": \"bob\" }");
                        // Act
                        // Assert
                    }
                }
            }

            [TestFixture]
            public class WhenSourceIsFolder
            {
                [TestFixture]
                public class AndTargetDoesNotExist
                {
                    [Test]
                    public void ShouldCopyRecursively()
                    {
                        // Arrange
                        using var tempFolder = new AutoTempFolder();
                        tempFolder.CreateFolder("src/foo");
                        tempFolder.CreateFolder("src/bar");
                        tempFolder.WriteFile("src/README.md", "# READ ME");
                        tempFolder.WriteFile("src/foo/index.js", "console.log('index');");
                        var srcFs = Create(tempFolder.ResolvePath("src"));
                        var expectedDirs = srcFs.ListDirectoriesRecursive();
                        var expectedFiles = srcFs.ListFilesRecursive();
                        var sut = Create(tempFolder.Path);
                        
                        Expect(tempFolder.ResolvePath("target"))
                            .Not.To.Exist();
                        // Act
                        sut.Copy("src", tempFolder.ResolvePath("target"));
                        // Assert
                        var targetFs = Create(tempFolder.ResolvePath("target"));
                        var targetDirs = targetFs.ListDirectoriesRecursive();
                        var targetFiles = targetFs.ListFilesRecursive();

                        Expect(targetDirs)
                            .To.Be.Equivalent.To(expectedDirs);
                        Expect(targetFiles)
                            .To.Be.Equivalent.To(expectedFiles);
                    }
                }

                [TestFixture]
                public class AndTargetExists
                {
                    [Test]
                    public void ShouldCopyRecursively()
                    {
                        // Arrange
                        using var tempFolder = new AutoTempFolder();
                        tempFolder.CreateFolder("src/foo");
                        tempFolder.CreateFolder("src/bar");
                        tempFolder.WriteFile("src/README.md", "# READ ME");
                        tempFolder.WriteFile("src/foo/index.js", "console.log('index');");
                        tempFolder.CreateFolder("target");
                        var srcFs = Create(tempFolder.ResolvePath("src"));
                        var expectedDirs = srcFs.ListDirectoriesRecursive()
                            .Map(p => Path.Combine("src", p))
                            .And("src"); // the base src folder will appear as we're listing 1-up
                        var expectedFiles = srcFs.ListFilesRecursive()
                            .Map(p => Path.Combine("src", p));
                        Expect(tempFolder.ResolvePath("target"))
                            .To.Exist();
                        // Act
                        var sut = Create(tempFolder.Path);
                        sut.Copy("src", tempFolder.ResolvePath("target"));
                        // Assert
                        var targetFs = Create(tempFolder.ResolvePath("target"));
                        var targetDirs = targetFs.ListDirectoriesRecursive();
                        var targetFiles = targetFs.ListFilesRecursive();

                        Expect(targetDirs)
                            .To.Be.Equivalent.To(expectedDirs);
                        Expect(targetFiles)
                            .To.Be.Equivalent.To(expectedFiles);
                    }
                }
            }
        }

        [TestFixture]
        public class OpenReader
        {
            [TestFixture]
            public class GivenRelativePath
            {
                [TestFixture]
                public class WhenFileNotFound
                {
                    [Test]
                    public void ShouldReturnNull()
                    {
                        //---------------Set up test pack-------------------
                        using var folder = new AutoTempFolder();
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

                [TestFixture]
                public class WhenFileFound
                {
                    [Test]
                    public void ShouldReturnStreamThatCanReadFileContents()
                    {
                        //---------------Set up test pack-------------------
                        using var folder = new AutoTempFolder();
                        var fileName = CreateRandomFileIn(folder.Path);
                        var sut = Create();
                        sut.SetCurrentDirectory(folder.Path);
                        var expected = File.ReadAllBytes(Path.Combine(folder.Path, fileName));

                        //---------------Assert Precondition----------------
                        Assert.IsTrue(File.Exists(Path.Combine(folder.Path, fileName)));

                        //---------------Execute Test ----------------------
                        using var result = sut.OpenReader(fileName);
                        //---------------Test Result -----------------------
                        var buffer = new byte[result.Length];
                        result.Read(buffer, 0, buffer.Length);
                        CollectionAssert.AreEqual(expected, buffer);
                    }
                }
            }

            [TestFixture]
            public class GivenAbsolutePath
            {
                [TestFixture]
                public class WhenFileNotFound
                {
                    [Test]
                    public void ShouldReturnNull()
                    {
                        //---------------Set up test pack-------------------
                        using var folder = new AutoTempFolder();
                        using var otherFolder = new AutoTempFolder();
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

                [TestFixture]
                public class WhenFileExists
                {
                    [Test]
                    public void ShouldReturnStreamThatCanReadFileContents()
                    {
                        //---------------Set up test pack-------------------
                        using var folder = new AutoTempFolder();
                        using var otherFolder = new AutoTempFolder();
                        var fileName = CreateRandomFileIn(folder.Path);
                        var sut = Create();
                        sut.SetCurrentDirectory(otherFolder.Path);
                        var expected = File.ReadAllBytes(Path.Combine(folder.Path, fileName));

                        //---------------Assert Precondition----------------
                        Assert.IsTrue(File.Exists(Path.Combine(folder.Path, fileName)));

                        //---------------Execute Test ----------------------
                        using var result = sut.OpenReader(Path.Combine(folder.Path, fileName));
                        //---------------Test Result -----------------------
                        var buffer = new byte[result.Length];
                        result.Read(buffer, 0, buffer.Length);
                        CollectionAssert.AreEqual(expected, buffer);
                    }
                }
            }
        }

        [TestFixture]
        public class OpenWriter
        {
            [TestFixture]
            public class WhenTargetFolderExists
            {
                [Test]
                public void ShouldReturnWritableStreamToFile()
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
            }
        }

        [TestFixture]
        public class Open
        {
            [Test]
            public void ShouldOpenReadWrite()
            {
                //---------------Set up test pack-------------------
                using var folder = new AutoTempFolder();
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

        [TestFixture]
        public class Move
        {
            [TestFixture]
            public class GivenSourceAndDestination
            {
                [TestFixture]
                public class WhenDestinationDoesNotExist
                {
                    [Test]
                    public void CanRenameToMove_ShouldRename()
                    {
                        //---------------Set up test pack-------------------
                        using var folder = new AutoTempFolder();
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
            }

            [TestFixture]
            public class WhenDestinationFileExists
            {
                [TestFixture]
                public class AndOverwriteIsFalse
                {
                    [Test]
                    public void ShouldThrow()
                    {
                        //---------------Set up test pack-------------------
                        using var folder = new AutoTempFolder();
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

                [TestFixture]
                public class AndOverwriteIsTrue
                {
                    [Test]
                    public void ShouldOverwrite()
                    {
                        //---------------Set up test pack-------------------
                        using var folder = new AutoTempFolder();
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
            }

            [TestFixture]
            public class WhenDestinationExistsOnAnotherVolume
            {
                [TestFixture]
                public class AndOverwriteIsTrue
                {
                    [Test]
                    [Explicit(
                        "Environment-specific: works if you have a remote machine called 'speedy' with a share called 'move_test' :D"
                    )]
                    public void ShouldOverwrite()
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
                }
            }
        }

        [Test]
        [Explicit("Discovery: Testing programmatic windows share creation")]
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
        [Explicit("Discovery: Testing programmatic windows share creation")]
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
            var shareName = GetRandomString(5, 10);
            WindowsShare.Create(
                folder,
                shareName,
                WindowsShare.ShareType.DiskDrive,
                2,
                "Temporary share for testing",
                null
            );
            return shareName;
        }


        private static IFileSystem Create(
            string startDir = null
        )
        {
            var result = new LocalFileSystem();
            if (startDir is not null)
            {
                result.SetCurrentDirectory(startDir);
            }

            return result;
        }
    }
}