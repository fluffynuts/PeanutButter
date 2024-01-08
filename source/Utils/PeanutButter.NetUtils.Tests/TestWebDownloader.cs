using System;
using System.IO;
using System.Threading.Tasks;
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
            Expect(() => new WebDownloader())
                .Not.To.Throw();

            //---------------Test Result -----------------------
        }

        [Test]
        public async Task DownloadFile_GivenAvailableDownload_WhenNoExistingFile_ShouldDownloadEntireFile()
        {
            //---------------Set up test pack-------------------
            using var http = new HttpServer();
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
            var result = await sut.Download(linkUrl, expectedFile);

            //---------------Test Result -----------------------
            Expect(result)
                .To.Be.True();
            Expect(expectedFile)
                .To.Be.A.File();
            Expect(await File.ReadAllBytesAsync(expectedFile))
                .To.Equal(someBytes);
        }

        // TODO: implement resume

        private WebDownloader Create()
        {
            return new WebDownloader();
        }
    }
}
