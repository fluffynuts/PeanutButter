using System.Reflection;
using System.Text;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.NetCore.Tests
{
    [TestFixture]
    public class TestAutoTempFile
    {
        [Test]
        public void Type_ShouldImplement_IDisposable()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(AutoTempFile);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(sut)
                .To.Implement<IDisposable>();

            //---------------Test Result -----------------------
        }

        [TestFixture]
        public class Construction
        {
            [Test]
            public void GivenNoParameters_ShouldCreateEmptyFile()
            {
                //---------------Set up test pack-------------------

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var sut = new AutoTempFile();

                //---------------Test Result -----------------------
                Expect(sut.Path)
                    .To.Be.A.File();
                var fileInfo = new FileInfo(sut.Path);
                Expect(fileInfo.Length)
                    .To.Equal(0);

                try
                {
                    File.Delete(sut.Path);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            [Test]
            public void GivenSomeBytes_ShouldPutThemInTheTempFile()
            {
                //---------------Set up test pack-------------------
                var expected = GetRandomBytes();
                using var sut = new AutoTempFile(expected);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = File.ReadAllBytes(sut.Path);

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(expected);
            }

            [Test]
            public void GivenNonExistentBaseFolder_ShouldCreateIt()
            {
                // Arrange
                using var folder = new AutoTempFolder();
                // Pre-assert
                // Act
                var basePath = Path.Combine(folder.Path, "moo", "cow", "duck");
                var data = GetRandomBytes(64);
                using var file = new AutoTempFile(basePath, "stuff.blob", data);
                var onDisk = File.ReadAllBytes(file.Path);
                // Assert
                Expect(onDisk).To.Equal(data);
            }

            [Test]
            public void GivenOneString_SetsContents()
            {
                // Arrange
                var expected = GetRandomString(128);
                // Pre-assert
                // Act
                using var file = new AutoTempFile(expected);
                // Assert
                Expect(file.StringData).To.Equal(expected);
                Expect(Encoding.UTF8.GetString(File.ReadAllBytes(file.Path)))
                    .To.Equal(expected);
            }


            [Test]
            public void GivenBasePath_ShouldUseThatInsteadOfTempDir()
            {
                //---------------Set up test pack-------------------
                var baseFolder = Path.GetDirectoryName(
                    new Uri(Assembly.GetExecutingAssembly().Location).LocalPath
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using var tempFile = new AutoTempFile(
                    baseFolder,
                    new byte[]
                    {
                    }
                );
                //---------------Test Result -----------------------
                Expect(Path.GetDirectoryName(tempFile.Path))
                    .To.Equal(baseFolder);
            }

            [Test]
            public void GivenBasePath_ShouldUseThatInsteadOfTempDirAndWriteBytes()
            {
                //---------------Set up test pack-------------------
                var baseFolder = Path.GetDirectoryName(
                    new Uri(
                        Assembly.GetExecutingAssembly().Location
                    ).LocalPath
                );
                var expected = GetRandomBytes();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using var tempFile = new AutoTempFile(baseFolder, expected);
                //---------------Test Result -----------------------
                Expect(Path.GetDirectoryName(tempFile.Path))
                    .To.Equal(baseFolder);
                Expect(File.ReadAllBytes(tempFile.Path))
                    .To.Equal(expected);
            }

            [Test]
            public void GivenBasePathAndStringData_ShouldUseThatInsteadOfTempDir()
            {
                //---------------Set up test pack-------------------
                var baseFolder = Path.GetDirectoryName(
                    new Uri(
                        Assembly.GetExecutingAssembly().Location
                    ).LocalPath
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using var tempFile = new AutoTempFile(baseFolder, "");
                //---------------Test Result -----------------------
                Expect(Path.GetDirectoryName(tempFile.Path))
                    .To.Equal(baseFolder);
            }

            [Test]
            public void GivenBasePathAndNullStringData_ShouldUseThatInsteadOfTempDir()
            {
                //---------------Set up test pack-------------------
                var baseFolder = Path.GetDirectoryName(
                    new Uri(
                        Assembly.GetExecutingAssembly().Location
                    ).LocalPath
                );

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using var tempFile = new AutoTempFile(baseFolder, (string)null);
                //---------------Test Result -----------------------
                Expect(Path.GetDirectoryName(tempFile.Path))
                    .To.Equal(baseFolder);
                Expect(File.ReadAllBytes(tempFile.Path))
                    .To.Be.Empty();
            }

            [Test]
            public void GivenBasePathAndStringData_ShouldUseThatInsteadOfTempDirAndWriteBytes()
            {
                //---------------Set up test pack-------------------
                var baseFolder = DetermineExecutingAssemblyFolder();
                var expected = GetRandomString();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using var tempFile = new AutoTempFile(baseFolder, expected);
                //---------------Test Result -----------------------
                Expect(Path.GetDirectoryName(tempFile.Path))
                    .To.Equal(baseFolder);
                Expect(File.ReadAllText(tempFile.Path))
                    .To.Equal(expected);
            }

            [Test]
            public void GivenTwoStringsAndBinaryData_ShouldUseStringsForFolderAndFileName()
            {
                //---------------Set up test pack-------------------
                var baseFolder = DetermineExecutingAssemblyFolder();
                var fileName = GetRandomString(5, 10) + "." + GetRandomString(3, 3);
                var data = GetRandomBytes();
                var expectedPath = Path.Combine(baseFolder, fileName);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                using (new AutoTempFile(baseFolder, fileName, data))
                {
                    //---------------Test Result -----------------------
                    Expect(expectedPath)
                        .To.Be.A.File();
                    Expect(File.ReadAllBytes(expectedPath))
                        .To.Equal(data);
                }
            }
        }

        [TestFixture]
        public class Disposal
        {
            [Test]
            public void ShouldRemoveTempFile()
            {
                //---------------Set up test pack-------------------
                var sut = new AutoTempFile();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Expect(sut.Path)
                    .To.Be.A.File();
                sut.Dispose();

                //---------------Test Result -----------------------
                Expect(sut.Path)
                    .Not.To.Exist();
            }
        }

        [TestFixture]
        public class BinaryData
        {
            [Test]
            public void ShouldExposeFileBinaryDataForRead()
            {
                // Arrange
                var data = GetRandomBytes(64);
                // Pre-assert
                // Act
                using var folder = new AutoTempFolder();
                using var file = new AutoTempFile(folder.Path, data);
                var result = file.BinaryData;
                // Assert
                Expect(result).To.Equal(data);
            }

            [Test]
            public void ShouldExposeFileBinaryDataForWrite()
            {
                // Arrange
                var original = GetRandomBytes(64);
                var expected = GetAnother(original, () => GetRandomBytes(64));
                // Pre-assert
                // Act
                using var folder = new AutoTempFolder();
                using var file = new AutoTempFile(folder.Path, original);
                file.BinaryData = expected;
                var result = File.ReadAllBytes(file.Path);
                // Assert
                Expect(result).To.Equal(expected);
            }
        }

        [TestFixture]
        public class StringData
        {
            [Test]
            public void ShouldExposeFileStringData()
            {
                // Arrange
                var data = GetRandomString(128);
                // Pre-assert
                // Act
                using var file = new AutoTempFile("stuff.blob", data);
                var result = file.StringData;
                // Assert
                Expect(result).To.Equal(data);
            }

            [Test]
            public void Setting_NULL_ShouldPutEmptyStringIntoFile()
            {
                //---------------Set up test pack-------------------
                var unexpected = GetRandomString();
                using var sut = new AutoTempFile(unexpected);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                sut.StringData = null;
                var result = File.ReadAllBytes(sut.Path);

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Be.Empty();
            }

            [Test]
            public void ShouldAllowSettingStringData()
            {
                // Arrange
                var original = GetRandomString(128);
                var expected = GetAnother(original, () => GetRandomString(128));
                // Pre-assert
                // Act
                using var folder = new AutoTempFolder();
                using var file = new AutoTempFile(folder.Path, original);
                file.StringData = expected;
                var result = Encoding.UTF8.GetString(File.ReadAllBytes(file.Path));
                // Assert
                Expect(result).To.Equal(expected);
            }
        }

        [TestFixture]
        public class Delete
        {
            [Test]
            public void ShouldDeleteExistingFile()
            {
                // Arrange
                var data = GetRandomWords();
                // ReSharper disable once ConvertToUsingDeclaration
                using (var tempFile = new AutoTempFile(data))
                {
                    Expect(tempFile.Path)
                        .To.Be.A.File();
                    Expect(File.ReadAllText(tempFile.Path))
                        .To.Equal(data);
                    // Act
                    tempFile.Delete();
                    // Assert
                    Expect(tempFile.Path)
                        .Not.To.Be.A.File();
                }
            }
        }

        private static string DetermineExecutingAssemblyFolder()
        {
            return Path.GetDirectoryName(
                new Uri(
                    Assembly.GetExecutingAssembly().Location
                ).LocalPath
            );
        }
    }
}