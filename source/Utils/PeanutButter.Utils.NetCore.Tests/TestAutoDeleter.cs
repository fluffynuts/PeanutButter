using System.Text;
using PeanutButter.RandomGenerators;

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
            Expect(tempFile)
                .To.Be.A.File();

            //---------------Execute Test ----------------------
            using (new AutoDeleter(tempFile))
            {
            }

            //---------------Test Result -----------------------
            Expect(tempFile)
                .Not.To.Exist();
        }

        [Test]
        public void Add_WhenGivenFilePathForExistingFile_ShouldDeleteFileWhenDisposed()
        {
            //---------------Set up test pack-------------------
            var tempFile = Path.GetTempFileName();

            //---------------Assert Precondition----------------
            Expect(tempFile)
                .To.Be.A.File();

            //---------------Execute Test ----------------------
            using (var ad = new AutoDeleter())
            {
                ad.Add(tempFile);
            }

            //---------------Test Result -----------------------
            Expect(tempFile)
                .Not.To.Exist();
        }

        [Test]
        public void Add_WhenGivenEmptyFolder_ShouldDeleteFolderWhenDisposed()
        {
            //---------------Set up test pack-------------------
            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);
            //---------------Assert Precondition----------------
            Expect(tempFolder)
                .To.Be.A.Folder();

            //---------------Execute Test ----------------------
            using (var ad = new AutoDeleter())
            {
                ad.Add(tempFolder);
            }

            //---------------Test Result -----------------------
            Expect(tempFolder)
                .Not.To.Exist();
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
            Expect(tempFolder)
                .To.Be.A.Folder();
            Expect(f1)
                .To.Be.A.File();
            Expect(f2)
                .To.Be.A.File();

            //---------------Execute Test ----------------------
            using (var ad = new AutoDeleter())
            {
                ad.Add(tempFolder);
            }

            //---------------Test Result -----------------------
            Expect(f1)
                .Not.To.Exist();
            Expect(f2)
                .Not.To.Exist();
            Expect(tempFolder)
                .Not.To.Exist();
        }
    }
}
