using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
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
    }
}
