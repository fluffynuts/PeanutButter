using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestAutoDeleter
    {
        [Test]
        public void Construct_WhenGivenFilePathForExistingFile_ShouldDeleteFileWhenDisposed()
        {
            //---------------Set up test pack-------------------
            var tempFile = Path.GetTempFileName();

            //---------------Assert Precondition----------------
            Assert.IsTrue(File.Exists(tempFile));

            //---------------Execute Test ----------------------
            using (new AutoDeleter(tempFile))
            {
            }

            //---------------Test Result -----------------------
            Assert.IsFalse(File.Exists(tempFile));
        }

        [Test]
        public void Add_WhenGivenFilePathForExistingFile_ShouldDeleteFileWhenDisposed()
        {
            //---------------Set up test pack-------------------
            var tempFile = Path.GetTempFileName();

            //---------------Assert Precondition----------------
            Assert.IsTrue(File.Exists(tempFile));

            //---------------Execute Test ----------------------
            using (var ad = new AutoDeleter())
            {
                ad.Add(tempFile);
            }

            //---------------Test Result -----------------------
            Assert.IsFalse(File.Exists(tempFile));
        }

        [Test]
        public void Add_WhenGivenEmptyFolder_ShouldDeleteFolderWhenDisposed()
        {
            //---------------Set up test pack-------------------
            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);
            //---------------Assert Precondition----------------
            Assert.IsTrue(Directory.Exists(tempFolder));

            //---------------Execute Test ----------------------
            using (var ad = new AutoDeleter())
            {
                ad.Add(tempFolder);
            }

            //---------------Test Result -----------------------
            Assert.IsFalse(Directory.Exists(tempFolder));
        }
     
        [Test]
        public void Add_WhenGivenNonEmptyFolder_ShouldDeleteFolderWhenDisposed()
        {
            //---------------Set up test pack-------------------
            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);
            var f1 = Path.Combine(tempFolder, Guid.NewGuid().ToString());
            File.WriteAllBytes(f1, RandomValueGen.GetRandomBytes(100, 200));
            var f2 = Path.Combine(tempFolder, RandomValueGen.GetRandomString(10, 20));
            File.WriteAllBytes(f2, Encoding.UTF8.GetBytes(RandomValueGen.GetRandomString(100, 200)));

            //---------------Assert Precondition----------------
            Assert.IsTrue(Directory.Exists(tempFolder));
            Assert.IsTrue(File.Exists(f1));
            Assert.IsTrue(File.Exists(f2));

            //---------------Execute Test ----------------------
            using (var ad = new AutoDeleter())
            {
                ad.Add(tempFolder);
            }

            //---------------Test Result -----------------------
            Assert.IsFalse(File.Exists(f1));
            Assert.IsFalse(File.Exists(f2));
            Assert.IsFalse(Directory.Exists(tempFolder));
        }
    }

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
            Assert.IsTrue(File.Exists(sut.FileName));
            var fileInfo = new FileInfo(sut.FileName);
            Assert.AreEqual(0, fileInfo.Length);
            
            try
            {
                File.Delete(sut.FileName);
            }
            catch (Exception)
            {
            }
        }

        [Test]
        public void Dispose_ShouldRemoveTempFile()
        {
            //---------------Set up test pack-------------------
            var sut = new AutoTempFile();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.IsTrue(File.Exists(sut.FileName));
            sut.Dispose();

            //---------------Test Result -----------------------
            Assert.IsFalse(File.Exists(sut.FileName));
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
                var result = File.ReadAllBytes(sut.FileName);

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
                var result = File.ReadAllBytes(sut.FileName);

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
                var result = File.ReadAllBytes(sut.FileName);

                //---------------Test Result -----------------------
                Assert.AreEqual(expected, result);
            }
        }
    }
}
