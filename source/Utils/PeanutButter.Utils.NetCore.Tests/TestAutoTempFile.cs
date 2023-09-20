using System.Reflection;
using System.Text;
using NExpect;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static NExpect.Expectations;

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

        [Test]
        public void Construct_GivenNoParameters_ShouldCreateEmptyFile()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = new AutoTempFile();

            //---------------Test Result -----------------------
            Assert.IsTrue(File.Exists(sut.Path));
            var fileInfo = new FileInfo(sut.Path);
            Assert.AreEqual(0, fileInfo.Length);

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
        public void Dispose_ShouldRemoveTempFile()
        {
            //---------------Set up test pack-------------------
            var sut = new AutoTempFile();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.IsTrue(File.Exists(sut.Path));
            sut.Dispose();

            //---------------Test Result -----------------------
            Assert.IsFalse(File.Exists(sut.Path));
        }

        [Test]
        public void Construct_GivenSomeBytes_ShouldPutThemInTheTempFile()
        {
            //---------------Set up test pack-------------------
            var expected = GetRandomBytes();
            using (var sut = new AutoTempFile(expected))
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = File.ReadAllBytes(sut.Path);

                //---------------Test Result -----------------------
                CollectionAssert.AreEqual(expected, result);
            }
        }

        [Test]
        public void Construct_GivenNonExistentBaseFolder_ShouldCreateIt()
        {
            // Arrange
            using (var folder = new AutoTempFolder())
            {
                // Pre-assert
                // Act
                var basePath = Path.Combine(folder.Path, "moo", "cow", "duck");
                var data = GetRandomBytes(64);
                using (var file = new AutoTempFile(basePath, "stuff.blob", data))
                {
                    var onDisk = File.ReadAllBytes(file.Path);
                    // Assert
                    Expect(onDisk).To.Equal(data);
                }
            }
        }

        [Test]
        public void ShouldExposeFileBinaryDataForRead()
        {
            // Arrange
            var data = GetRandomBytes(64);
            // Pre-assert
            // Act
            using (var folder = new AutoTempFolder())
            using (var file = new AutoTempFile(folder.Path, data))
            {
                var result = file.BinaryData;
                // Assert
                Expect(result).To.Equal(data);
            }
        }

        [Test]
        public void ShouldExposeFileBinaryDataForWrite()
        {
            // Arrange
            var original = GetRandomBytes(64);
            var expected = GetAnother(original, () => GetRandomBytes(64));
            // Pre-assert
            // Act
            using (var folder = new AutoTempFolder())
            using (var file = new AutoTempFile(folder.Path, original))
            {
                file.BinaryData = expected;
                var result = File.ReadAllBytes(file.Path);
                // Assert
                Expect(result).To.Equal(expected);
            }
        }

        [Test]
        public void ShouldExposeFileStringData()
        {
            // Arrange
            var data = GetRandomString(128);
            // Pre-assert
            // Act
            using (var file = new AutoTempFile("stuff.blob", data))
            {
                var result = file.StringData;
                // Assert
                Expect(result).To.Equal(data);
            }
        }

        [Test]
        public void ShouldAllowSettingStringData()
        {
            // Arrange
            var original = GetRandomString(128);
            var expected = GetAnother(original, () => GetRandomString(128));
            // Pre-assert
            // Act
            using (var folder = new AutoTempFolder())
            using (var file = new AutoTempFile(folder.Path, original))
            {
                file.StringData = expected;
                var result = Encoding.UTF8.GetString(File.ReadAllBytes(file.Path));
                // Assert
                Expect(result).To.Equal(expected);
            }
        }

        [Test]
        public void Construct_GivenOneString_SetsContents()
        {
            // Arrange
            var expected = GetRandomString(128);
            // Pre-assert
            // Act
            using (var file = new AutoTempFile(expected))
            {
                // Assert
                Expect(file.StringData).To.Equal(expected);
                Expect(Encoding.UTF8.GetString(File.ReadAllBytes(file.Path)))
                    .To.Equal(expected);
            }
        }

        [Test]
        public void BinaryData_get_ShouldReturnBytesInFile()
        {
            //---------------Set up test pack-------------------
            var expected = GetRandomBytes();
            using (var sut = new AutoTempFile(expected))
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.BinaryData;

                //---------------Test Result -----------------------
                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        public void BinaryData_set_ShouldOverwriteDataInFile()
        {
            //---------------Set up test pack-------------------
            var unexpected = GetRandomBytes();
            var expected = GetRandomBytes();
            using (var sut = new AutoTempFile(unexpected))
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                sut.BinaryData = expected;
                var result = File.ReadAllBytes(sut.Path);

                //---------------Test Result -----------------------
                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        public void StringData_get_WhenDataInFileIsText_ShouldReturnBytesInFileAsUtf8EncodedString()
        {
            //---------------Set up test pack-------------------
            var expected = GetRandomString();
            using (var sut = new AutoTempFile(expected))
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.StringData;

                //---------------Test Result -----------------------
                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        public void StringData_set_ShouldPutStringIntoFile()
        {
            //---------------Set up test pack-------------------
            var unexpected = GetRandomString();
            var expected = GetRandomString();
            using (var sut = new AutoTempFile(unexpected))
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                sut.StringData = expected;
                var result = File.ReadAllBytes(sut.Path);

                //---------------Test Result -----------------------
                Assert.AreEqual(expected, result);
            }
        }

        [Test]
        public void StringData_set_NULL_ShouldPutStringIntoFile()
        {
            //---------------Set up test pack-------------------
            var unexpected = GetRandomString();
            using (var sut = new AutoTempFile(unexpected))
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                sut.StringData = null;
                var result = File.ReadAllBytes(sut.Path);

                //---------------Test Result -----------------------
                CollectionAssert.IsEmpty(result);
            }
        }

        [Test]
        public void Construct_GivenBasePath_ShouldUseThatInsteadOfTempDir()
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
            Assert.AreEqual(baseFolder, Path.GetDirectoryName(tempFile.Path));
        }

        [Test]
        public void Construct_GivenBasePath_ShouldUseThatInsteadOfTempDirAndWriteBytes()
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
            using (var tempFile = new AutoTempFile(baseFolder, expected))
            {
                //---------------Test Result -----------------------
                Assert.AreEqual(baseFolder, Path.GetDirectoryName(tempFile.Path));
                CollectionAssert.AreEqual(expected, File.ReadAllBytes(tempFile.Path));
            }
        }

        [Test]
        public void Construct_GivenBasePathAndStringData_ShouldUseThatInsteadOfTempDir()
        {
            //---------------Set up test pack-------------------
            var baseFolder = Path.GetDirectoryName(
                new Uri(
                    Assembly.GetExecutingAssembly().Location
                ).LocalPath
            );

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (var tempFile = new AutoTempFile(baseFolder, ""))
            {
                //---------------Test Result -----------------------
                Assert.AreEqual(baseFolder, Path.GetDirectoryName(tempFile.Path));
            }
        }

        [Test]
        public void Construct_GivenBasePathAndNullStringData_ShouldUseThatInsteadOfTempDir()
        {
            //---------------Set up test pack-------------------
            var baseFolder = Path.GetDirectoryName(
                new Uri(
                    Assembly.GetExecutingAssembly().Location
                ).LocalPath
            );

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (var tempFile = new AutoTempFile(baseFolder, (string)null))
            {
                //---------------Test Result -----------------------
                Assert.AreEqual(baseFolder, Path.GetDirectoryName(tempFile.Path));
                CollectionAssert.IsEmpty(File.ReadAllBytes(tempFile.Path));
            }
        }

        [Test]
        public void Construct_GivenBasePathAndStringData_ShouldUseThatInsteadOfTempDirAndWriteBytes()
        {
            //---------------Set up test pack-------------------
            var baseFolder = DetermineExecutingAssemblyFolder();
            var expected = GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (var tempFile = new AutoTempFile(baseFolder, expected))
            {
                //---------------Test Result -----------------------
                Assert.AreEqual(baseFolder, Path.GetDirectoryName(tempFile.Path));
                CollectionAssert.AreEqual(expected, File.ReadAllBytes(tempFile.Path));
            }
        }

        [Test]
        public void Construct_GivenTwoStringsAndBinaryData_ShouldUseStringsForFolderAndFileName()
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
                Assert.IsTrue(File.Exists(expectedPath));
                CollectionAssert.AreEqual(data, File.ReadAllBytes(expectedPath));
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