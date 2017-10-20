using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
// ReSharper disable AssignNullToNotNullAttribute

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestAutoTempFolder
    {
        [Test]
        public void Type_ShouldImplement_IDisposable()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(AutoTempFolder);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldImplement<IDisposable>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_ShouldMakeNewEmptyFolderAvailable()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (var folder = new AutoTempFolder())
            {
                var result = folder.Path;
                //---------------Test Result -----------------------
                Assert.IsNotNull(result);
                Assert.IsTrue(Directory.Exists(result));
                var entries = Directory.EnumerateFileSystemEntries(result, "*", SearchOption.AllDirectories);
                CollectionAssert.IsEmpty(entries);

            }
        }

        [Test]
        public void Dispose_ShouldRemoveEmptyTempFolder()
        {
            //---------------Set up test pack-------------------

            string folderPath;
            using (var folder = new AutoTempFolder())
            {
                folderPath = folder.Path;
                //---------------Assert Precondition----------------
                Assert.IsNotNull(folderPath);
                Assert.IsTrue(Directory.Exists(folderPath));
                var entries = Directory.EnumerateFileSystemEntries(folderPath, "*", SearchOption.AllDirectories);
                CollectionAssert.IsEmpty(entries);
                //---------------Execute Test ----------------------
            }
            //---------------Test Result -----------------------
            Assert.IsFalse(Directory.Exists(folderPath));
        }

        [Test]
        public void Dispose_ShouldRemoveNonEmptyTempFolder()
        {
            //---------------Set up test pack-------------------
            string folderPath;
            using (var folder = new AutoTempFolder())
            {
                folderPath = folder.Path;
                Assert.IsNotNull(folderPath);


                //---------------Assert Precondition----------------
                Assert.IsTrue(Directory.Exists(folderPath));
                var entries = Directory.EnumerateFileSystemEntries(folderPath, "*", SearchOption.AllDirectories);
                CollectionAssert.IsEmpty(entries);

                //---------------Execute Test ----------------------
                File.WriteAllBytes(Path.Combine(folderPath, RandomValueGen.GetRandomString(2,10)), RandomValueGen.GetRandomBytes());
                File.WriteAllBytes(Path.Combine(folderPath, RandomValueGen.GetRandomString(11,20)), RandomValueGen.GetRandomBytes());
                entries = Directory.EnumerateFileSystemEntries(folderPath, "*", SearchOption.AllDirectories);
                CollectionAssert.IsNotEmpty(entries);

            }
            //---------------Test Result -----------------------
            Assert.IsFalse(Directory.Exists(folderPath));
        }

        [Test]
        public void Construct_GivenCustomBasePath_ShouldUseThatPathAsBaseForTempFolder()
        {
            //---------------Set up test pack-------------------
            var baseFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (var folder = new AutoTempFolder(baseFolder))
            {
                //---------------Test Result -----------------------
                Assert.AreEqual(baseFolder, Path.GetDirectoryName(folder.Path));
            }

        }

        [Test]
        public void Construct_GivenCustomBasePathWhichDoesNotExistYet_ShouldUseThatPathAsBaseForTempFolder()
        {
            //---------------Set up test pack-------------------
            var baseFolder = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            baseFolder = Path.Combine(baseFolder, RandomValueGen.GetRandomString(10, 15));

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            using (var folder = new AutoTempFolder(baseFolder))
            {
                //---------------Test Result -----------------------
                Assert.AreEqual(baseFolder, Path.GetDirectoryName(folder.Path));
            }

        }


    }
}