using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
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
                sut.CreateFolder(dirname);
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
                var result = sut.CreateFolder(dirname);
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

            [Test]
            public void ShouldReadFileWithAbsolutePathWithinTempFolder()
            {
                // Arrange
                var data = GetRandomString(32);
                var filename = GetRandomString(32);
                using var sut = Create();

                // Act
                var fullPath = sut.ResolvePath(filename);
                File.WriteAllText(fullPath, data);
                var result = sut.ReadTextFile(fullPath);
                // Assert
                Expect(result)
                    .To.Equal(data);
            }

            [Test]
            public void ShouldBeAbleToReadAnOpenFileWithTheCorrectAccess()
            {
                // Arrange
                using var sut = Create();
                var filename = sut.ResolvePath(GetRandomString(12));
                using var src = new FileStream(
                    filename,
                    FileMode.Create,
                    FileAccess.ReadWrite
                );
                var data = GetRandomBytes(100);
                src.Write(data, 0, data.Length);
                src.Flush();
                // Act
                using var other = sut.OpenFile(
                    filename,
                    FileAccess.Read,
                    FileShare.ReadWrite
                );
                var read = other.ReadAllBytes();

                // Assert
                Expect(read)
                    .To.Equal(data);
            }
        }

        [TestFixture]
        public class FileExists
        {
            [Test]
            public void ShouldReturnFalseWhenFileDoesNotExist()
            {
                // Arrange
                using var sut = Create();
                var filename = GetRandomString(10);
                var fullPath = sut.ResolvePath(filename);
                Expect(fullPath)
                    .Not.To.Exist();
                // Act
                var result = sut.FileExists(filename);
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnFalseWhenPathIsFolder()
            {
                // Arrange
                using var sut = Create();
                var path = GetRandomString(10);
                var fullPath = sut.CreateFolder(path);
                Expect(fullPath)
                    .To.Be.A.Folder();
                // Act
                var result = sut.FileExists(path);
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnTrueWhenPathIsAFile()
            {
                // Arrange
                using var sut = Create();
                var path = GetRandomString(10);
                var data = GetRandomBytes(100);
                var fullPath = sut.WriteFile(path, data); 
                Expect(fullPath)
                    .To.Be.A.File();
                // Act
                var result = sut.FileExists(path);
                // Assert
                Expect(result)
                    .To.Be.True();
            }
        }

        [TestFixture]
        public class FolderExists
        {
            [Test]
            public void ShouldReturnFalseWhenFolderDoesNotExist()
            {
                // Arrange
                using var sut = Create();
                var path = GetRandomString(10);
                var fullPath = sut.ResolvePath(path);
                Expect(fullPath)
                    .Not.To.Exist();
                // Act
                var result = sut.FolderExists(path);
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnFalseWhenPathIsFile()
            {
                // Arrange
                using var sut = Create();
                var path = GetRandomString(10);
                var fullPath = sut.WriteFile(path, GetRandomBytes(100));
                Expect(fullPath)
                    .To.Be.A.File();
                // Act
                var result = sut.FolderExists(path);
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnTrueWhenPathIsAFolder()
            {
                // Arrange
                using var sut = Create();
                var path = GetRandomString(10);
                var fullPath = sut.CreateFolder(path); 
                Expect(fullPath)
                    .To.Be.A.Folder();
                // Act
                var result = sut.FolderExists(path);
                // Assert
                Expect(result)
                    .To.Be.True();
            }
        }

        [TestFixture]
        public class Exists
        {
            [Test]
            public void ShouldReturnFalseWhenNoFileOrFolder()
            {
                // Arrange
                using var sut = Create();
                var path = GetRandomString(10);
                var fullPath = sut.ResolvePath(path);
                Expect(fullPath)
                    .Not.To.Exist();
                // Act
                var result = sut.Exists(path);
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnTrueWhenFileExists()
            {
                // Arrange
                using var sut = Create();
                var path = GetRandomString(10);
                var fullPath = sut.WriteFile(path, GetRandomBytes());
                Expect(fullPath)
                    .To.Be.A.File();
                // Act
                var result = sut.Exists(path);
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnTrueWhenFolderExists()
            {
                // Arrange
                using var sut = Create();
                var path = GetRandomString(10);
                var fullPath = sut.CreateFolder(path);
                Expect(fullPath)
                    .To.Be.A.Folder();
                // Act
                var result = sut.Exists(path);
                // Assert
                Expect(result)
                    .To.Be.True();
            }
        }

        [TestFixture]
        public class Contains
        {
            [Test]
            public void ShouldReturnFalseWhenFileIsOutsideTempFolder()
            {
                // Arrange
                using var sut = Create();
                using var other = Create();
                var fullPath = other.WriteFile(GetRandomString(), GetRandomBytes());
                // Act
                var result = sut.Contains(fullPath);
                // Assert
                Expect(result)
                    .To.Be.False();
            }
            
            [Test]
            public void ShouldReturnFalseWhenFolderIsOutsideTempFolder()
            {
                // Arrange
                using var sut = Create();
                using var other = Create();
                var fullPath = other.CreateFolder(GetRandomString());
                // Act
                var result = sut.Contains(fullPath);
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnTrueWhenFileIsInsideTempFolder()
            {
                // Arrange
                using var sut = Create();
                var fullPath = Path.Combine(sut.Path, GetRandomString());
                File.WriteAllBytes(fullPath, GetRandomBytes());
                // Act
                var result = sut.Contains(fullPath);
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnTrueWhenFolderIsInsideTempFolder()
            {
                // Arrange
                using var sut = Create();
                var fullPath = Path.Combine(sut.Path, GetRandomString());
                Directory.CreateDirectory(fullPath);
                // Act
                var result = sut.Contains(fullPath);
                // Assert
                Expect(result)
                    .To.Be.True();
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

        private static IAutoTempFolder Create()
        {
            return new AutoTempFolder();
        }
    }
}