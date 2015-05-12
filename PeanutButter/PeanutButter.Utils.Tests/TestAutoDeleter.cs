using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using PeanutButter.RandomGenerators;
using PeanutButter.Utils;

namespace PenautButter.Utils.Tests
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
}
