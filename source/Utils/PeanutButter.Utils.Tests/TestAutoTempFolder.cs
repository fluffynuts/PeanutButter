using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;

// ReSharper disable AssignNullToNotNullAttribute

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestAutoTempFolder
    {
        [Test]
        public void ShouldImplement_IDisposable()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(AutoTempFolder);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(sut).To.Implement<IDisposable>();

            //---------------Test Result -----------------------
        }

        [TestFixture]
        public class Construction
        {
            [Test]
            public void ShouldMakeNewEmptyFolderAvailable()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using var folder = new AutoTempFolder();
                var result = folder.Path;
                //---------------Test Result -----------------------

                Expect(result)
                    .Not.To.Be.Null();
                Expect(result)
                    .To.Be.A.Folder();
                var entries = Directory.EnumerateFileSystemEntries(
                    result,
                    "*",
                    SearchOption.AllDirectories
                );
                Expect(entries)
                    .To.Be.Empty();
            }

            [TestFixture]
            public class GivenBasePath
            {
                [Test]
                public void ShouldUseProvidedCustomBasePath()
                {
                    //---------------Set up test pack-------------------
                    var baseFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    using var folder = new AutoTempFolder(baseFolder);
                    //---------------Test Result -----------------------
                    Expect(Path.GetDirectoryName(
                            folder.Path
                        ))
                        .To.Equal(
                            baseFolder
                        );
                }

                [Test]
                public void ShouldCreateCustomBasePathIfNotFound()
                {
                    //---------------Set up test pack-------------------
                    var baseFolder = Path.GetDirectoryName(
                        new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath
                    );
                    baseFolder = Path.Combine(baseFolder, GetRandomString(10, 15));

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    using var folder = new AutoTempFolder(baseFolder);
                    //---------------Test Result -----------------------
                    Expect(Path.GetDirectoryName(folder.Path))
                        .To.Equal(baseFolder);
                }
            }
        }

        [TestFixture]
        public class FilePath
        {
            [Test]
            public void ShouldReturnFilePathWithinTempFolder()
            {
                // Arrange
                var fileName = GetRandomString(32);
                using var sut = Create();
                var expected = Path.Combine(sut.Path, fileName);
                // Act
                var result = sut.ResolvePath(fileName);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldReturnCombinedPathAllTheWayDown()
            {
                // Arrange
                var fileName = GetRandomString(32);
                var sub1 = GetRandomString(32);
                var sub2 = GetRandomString(32);
                using var sut = Create();
                var expected = Path.Combine(sut.Path, sub1, sub2, fileName);
                // Act
                var result = sut.ResolvePath(sub1, sub2, fileName);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class CreateDirectory
        {
            [Test]
            public void ShouldCreateTheRelativeDirectory()
            {
                // Arrange
                var dirname = GetRandomString(32);
                using var sut = Create();
                var expected = Path.Combine(sut.Path, dirname);
                // Act
                sut.CreateDirectory(dirname);
                // Assert
                Expect(expected)
                    .To.Be.A.Folder();
            }

            [Test]
            public void ShouldReturnTheAbsolutePathForTheNewDirectory()
            {
                // Arrange
                var dirname = GetRandomString(32);
                using var sut = Create();
                var expected = Path.Combine(sut.Path, dirname);
                // Act
                var result = sut.CreateDirectory(dirname);
                // Assert
                Expect(result)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        public class WritingFiles
        {
            [Test]
            public void StringOverloadShouldWriteTextToFile()
            {
                // Arrange
                var data = GetRandomString(32);
                var filename = GetRandomString(32);
                using var sut = Create();

                // Act
                var fullPath = sut.WriteFile(filename, data);
                // Assert
                Expect(fullPath)
                    .To.Be.A.File();
                var written = File.ReadAllText(fullPath);
                Expect(written)
                    .To.Equal(data);
            }

            [Test]
            public void BytesOverloadShouldWriteDataToFile()
            {
                // Arrange
                var data = GetRandomBytes(32);
                var filename = GetRandomString(32);
                using var sut = Create();

                // Act
                var fullPath = sut.WriteFile(filename, data);
                // Assert
                Expect(fullPath)
                    .To.Be.A.File();
                var written = File.ReadAllBytes(fullPath);
                Expect(written)
                    .To.Equal(data);
            }

            [Test]
            public void StreamOverloadShouldWriteDataToFile()
            {
                // Arrange
                var data = GetRandomBytes(32);
                var stream = new MemoryStream(data);
                var filename = GetRandomString(32);
                using var sut = Create();

                // Act
                var fullPath = sut.WriteFile(filename, stream);
                // Assert
                Expect(fullPath)
                    .To.Be.A.File();
                var written = File.ReadAllBytes(fullPath);
                Expect(written)
                    .To.Equal(data);
            }

            [Test]
            public void ShouldCreateSupportingDirectoryStructures()
            {
                // Arrange
                var data = GetRandomString(32);
                var sub1 = GetRandomString(12);
                var sub2 = GetRandomString(12);
                var filename = GetRandomString(12);
                using var sut = Create();
                var relpath = Path.Combine(sub1, sub2, filename);

                // Act
                var fullPath = sut.WriteFile(relpath, data);
                // Assert

                Expect(fullPath)
                    .To.Be.A.File();
                var written = File.ReadAllText(fullPath);
                Expect(written)
                    .To.Equal(data);
            }
        }

        [TestFixture]
        public class ReadingFiles
        {
            [Test]
            public void ShouldReadTextFile()
            {
                // Arrange
                var data = GetRandomString(32);
                var filename = GetRandomString(32);
                using var sut = Create();
                // Act
                var fullPath = sut.ResolvePath(filename);
                File.WriteAllText(fullPath, data);
                var result = sut.ReadTextFile(filename);
                // Assert
                Expect(result)
                    .To.Equal(data);
            }
        }

        [TestFixture]
        public class Dispose
        {
            [Test]
            public void ShouldRemoveEmptyTempFolder()
            {
                //---------------Set up test pack-------------------

                string folderPath;
                using (var folder = Create())
                {
                    folderPath = folder.Path;
                    //---------------Assert Precondition----------------
                    Expect(folderPath)
                        .Not.To.Be.Null();
                    Expect(folderPath)
                        .To.Be.A.Folder();
                    var entries = Directory.EnumerateFileSystemEntries(folderPath, "*", SearchOption.AllDirectories);
                    Expect(entries)
                        .To.Be.Empty();
                    //---------------Execute Test ----------------------
                }

                //---------------Test Result -----------------------
                Expect(folderPath)
                    .Not.To.Exist();
            }

            [Test]
            public void ShouldRemoveNonEmptyTempFolder()
            {
                //---------------Set up test pack-------------------
                string folderPath;
                using (var folder = Create())
                {
                    folderPath = folder.Path;
                    Assert.IsNotNull(folderPath);


                    //---------------Assert Precondition----------------
                    Expect(folderPath)
                        .To.Be.A.Folder();
                    var entries = Directory.EnumerateFileSystemEntries(folderPath, "*", SearchOption.AllDirectories);
                    Expect(entries)
                        .To.Be.Empty();

                    //---------------Execute Test ----------------------
                    File.WriteAllBytes(
                        Path.Combine(folderPath, GetRandomString(2, 10)),
                        GetRandomBytes());
                    File.WriteAllBytes(
                        Path.Combine(folderPath, GetRandomString(11, 20)),
                        GetRandomBytes());
                    entries = Directory.EnumerateFileSystemEntries(folderPath, "*", SearchOption.AllDirectories);
                    Expect(entries)
                        .Not.To.Be.Empty();
                }

                //---------------Test Result -----------------------
                Expect(folderPath)
                    .Not.To.Be.A.Folder();
            }

            [Test]
            public void ShouldDisposeUndisposedFileStreams()
            {
                // Arrange
                var filename = GetRandomString(32);
                string folderPath;
                using (var sut = Create())
                {
                    // Act
                    folderPath = sut.Path;
                    var fp = sut.OpenFile(filename, FileAccess.ReadWrite);
                    fp.Write(GetRandomBytes(100), 0, 100);
                }
                // Assert
                Expect(folderPath)
                    .Not.To.Exist();
            }

            [Test]
            public void ShouldNoBarfOnDisposedFileStreams()
            {
                // Arrange
                var filename = GetRandomString(32);
                string folderPath;
                using (var sut = Create())
                {
                    // Act
                    folderPath = sut.Path;
                    using var fp = sut.OpenFile(filename, FileAccess.ReadWrite);
                    fp.Write(GetRandomBytes(100), 0, 100);
                }
                // Assert
                Expect(folderPath)
                    .Not.To.Exist();
            }
        }

        private static AutoTempFolder Create()
        {
            return new AutoTempFolder();
        }
    }
}