using System.IO;
using System.Net;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.Utils;

namespace PeanutButter.SimpleHTTPServer.Tests
{
    [TestFixture]
    public class TestHttpServer
    {
        [Test]
        public void Construct_WhenPortIsSpecified_ShouldUseThatPort()
        {
            var port = RandomValueGen.GetRandomInt(2000, 3000);
            //---------------Set up test pack-------------------
            using (var server = new HttpServer(port))
            {

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------

                //---------------Test Result -----------------------
                Assert.AreEqual(port, server.Port);
            }
        }

        [Test]
        public void Construct_WhenPortIsNotSpecified_ShouldChooseRandomPortBetween_5000_And_50000()
        {
            //---------------Set up test pack-------------------
            for (var i = 0; i < 10; i++)
                using (var server = new HttpServer())
                {

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------

                    //---------------Test Result -----------------------
                    Assert.That(server.Port, Is.GreaterThanOrEqualTo(5000));
                    Assert.That(server.Port, Is.LessThanOrEqualTo(50000));
                }
        }

        [Test]
        public void Start_WhenConfiguredToServeXDocument_ShouldServeDocument()
        {
            //---------------Set up test pack-------------------
            var doc = "<html><head></head><body><p>" + RandomValueGen.GetRandomAlphaNumericString() + "</p></body></html>";
            const string theDocName = "index.html";
            using (var server = new HttpServer())
            {
                //---------------Assert Precondition----------------
                server.AddDocumentHandler((p, s) => p.Path == "/" + theDocName ? doc : null);
                server.Start();

                //---------------Execute Test ----------------------
                var result = DownloadResultFrom(server, theDocName).ToUTF8String();

                //---------------Test Result -----------------------
                Assert.AreEqual(doc, result);
            }
        }

        [Test]
        public void Start_WhenConfiguredToServeFile_ShouldReturnTheFileContents()
        {
            //---------------Set up test pack-------------------
            var theFile = RandomValueGen.GetRandomBytes(100, 200);
            const string theFileName = "somefile.bin";
            using (var server = new HttpServer())
            {
                //---------------Assert Precondition----------------
                server.AddFileHandler((p, s) => p.Path == "/" + theFileName ? theFile : null);
                server.Start();
                //---------------Execute Test ----------------------
                var result = DownloadResultFrom(server, theFileName);
                //---------------Test Result -----------------------
                CollectionAssert.AreEqual(theFile, result);
            }
        }

        private byte[] DownloadResultFrom(HttpServer server, string path)
        {
            var request = WebRequest.Create(server.GetFullUrlFor(path));
            var response = request.GetResponse();
            using (var s = response.GetResponseStream())
            {
                var memStream = new MemoryStream();
                s.CopyTo(memStream);
                return memStream.ToArray();
            }
        }
    }

}
