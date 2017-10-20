using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.Utils.Tests
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
            sut.ShouldImplement<IDisposable>();

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
            var expected = RandomValueGen.GetRandomBytes();
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
        public void BinaryData_get_ShouldReturnBytesInFile()
        {
            //---------------Set up test pack-------------------
            var expected = RandomValueGen.GetRandomBytes();
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
            var unexpected = RandomValueGen.GetRandomBytes();
            var expected = RandomValueGen.GetRandomBytes();
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
            var expected = RandomValueGen.GetRandomString();
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
            var unexpected = RandomValueGen.GetRandomString();
            var expected = RandomValueGen.GetRandomString();
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
            var unexpected = RandomValueGen.GetRandomString();
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
            var baseFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (var tempFile = new AutoTempFile(baseFolder, new byte[] { }))
            {
                //---------------Test Result -----------------------
                Assert.AreEqual(baseFolder, Path.GetDirectoryName(tempFile.Path));
            }
        }

        [Test]
        public void Construct_GivenBasePath_ShouldUseThatInsteadOfTempDirAndWriteBytes()
        {
            //---------------Set up test pack-------------------
            var baseFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            var expected = RandomValueGen.GetRandomBytes();

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
            var baseFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

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
            var baseFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

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
            var baseFolder = GetExecutingAssemblyFolder();
            var expected = RandomValueGen.GetRandomString();

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
            var baseFolder = GetExecutingAssemblyFolder();
            var fileName = RandomValueGen.GetRandomString(5, 10) + "." + RandomValueGen.GetRandomString(3,3);
            var data = RandomValueGen.GetRandomBytes();
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

        private static string GetExecutingAssemblyFolder()
        {
            return Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
        }
    }
}