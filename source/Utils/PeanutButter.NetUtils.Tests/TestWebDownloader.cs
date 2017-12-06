using System;
using System.IO;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.SimpleHTTPServer;

namespace PeanutButter.NetUtils.Tests
{
    [TestFixture]
    public class TestWebDownloader
    {
        [Test]
        public void Construct_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            // ReSharper disable once ObjectCreationAsStatement
            Assert.DoesNotThrow(() => new WebDownloader());

            //---------------Test Result -----------------------
        }

        [Test]
        public void DownloadFile_GivenAvailableDownload_WhenNoExistingFile_ShouldDownloadEntireFile()
        {
            //---------------Set up test pack-------------------
            using (var http = new HttpServer())
            {
                var folder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(folder);
                var someBytes = RandomValueGen.GetRandomBytes(100, 200);
                var srcPath = RandomValueGen.GetRandomString(10, 20);
                http.AddFileHandler((processor, stream) => processor.Path == "/" + srcPath ? someBytes : null);
                var sut = Create();
                var linkUrl = http.GetFullUrlFor(srcPath);
                var expectedFile = Path.Combine(folder, srcPath);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = sut.Download(linkUrl, expectedFile).Result;

                //---------------Test Result -----------------------
                Assert.IsTrue(result);
                Assert.IsTrue(File.Exists(expectedFile));
                CollectionAssert.AreEqual(someBytes, File.ReadAllBytes(expectedFile));
            }
        }

        // TODO: implement resume

        private WebDownloader Create()
        {
            return new WebDownloader();
        }
    }
}
