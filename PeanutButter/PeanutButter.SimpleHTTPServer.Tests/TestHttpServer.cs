using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Linq;
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
            for (var i = 0; i < 100; i++)
                using (var server = new HttpServer())
                {

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------

                    //---------------Test Result -----------------------
                    Assert.That(server.Port, Is.GreaterThanOrEqualTo(5000));
                    Assert.That(server.Port, Is.LessThanOrEqualTo(50000));
                    Console.WriteLine("Server started on port: " + server.Port);
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
                //---------------Execute Test ----------------------
                var result = DownloadResultFrom(server, theFileName);
                //---------------Test Result -----------------------
                CollectionAssert.AreEqual(theFile, result);
            }
        }

        [Test]
        public void ServeDocument_GivenPathAndDocument_ShouldServeForThatPath()
        {
            using (var server = new HttpServer())
            {
                //---------------Set up test pack-------------------
                var doc = new XDocument(new XElement("html", new XElement("body", new XElement("p", new XText(RandomValueGen.GetRandomString())))));
                server.ServeDocument("/index.html", doc);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = DownloadResultFrom(server, "/index.html");

                //---------------Test Result -----------------------
                Assert.AreEqual(doc.ToString(), result.ToUTF8String());
            }
        }

        [Test]
        public void ServeDocument_GivenPathAndDocument_ShouldGive404ForOtherPaths()
        {
            using (var server = new HttpServer())
            {
                //---------------Set up test pack-------------------
                var doc = new XDocument(new XElement("html", new XElement("body", new XElement("p", new XText(RandomValueGen.GetRandomString())))));
                server.ServeDocument("/index.html", doc);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var ex = Assert.Throws<WebException>(() => DownloadResultFrom(server, "/index1.html"));

                //---------------Test Result -----------------------
                var response = ex.Response as HttpWebResponse;
                Assert.IsNotNull(response);
                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        [Test]
        public void ServeFile_GivenPathAndData_ShouldServeForThatPath()
        {
            using (var server = new HttpServer())
            {
                //---------------Set up test pack-------------------
                var data = RandomValueGen.GetRandomBytes(10, 100);
                var contentType = "text/" + RandomValueGen.GetRandomFrom(new[] { "xml", "html", "javascript", "plain" });
                server.ServeFile("/file.bin", data, contentType);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                string servedType = null;
                var result = DownloadResultFrom(server, "/file.bin", out servedType);

                //---------------Test Result -----------------------
                CollectionAssert.AreEqual(data, result);
                Assert.AreEqual(contentType, servedType);
            }
        }

        [Test]
        public void ServeFile_GivenPathAndData_ShouldGive404ForOtherPaths()
        {
            using (var server = new HttpServer())
            {
                //---------------Set up test pack-------------------
                var data = RandomValueGen.GetRandomBytes(10, 100);
                var contentType = "text/" + RandomValueGen.GetRandomFrom(new[] { "xml", "html", "javascript", "plain" });
                server.ServeFile("/file.bin", data, contentType);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var ex = Assert.Throws<WebException>(() => DownloadResultFrom(server, "/file1.bin"));

                //---------------Test Result -----------------------
                var response = ex.Response as HttpWebResponse;
                Assert.IsNotNull(response);
                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
            }
        }

        private byte[] DownloadResultFrom(HttpServer server, string path)
        {
            string contentType;
            return DownloadResultFrom(server, path, out contentType);
        }

        private byte[] DownloadResultFrom(HttpServer server, string path, out string contentType)
        {
            var request = WebRequest.Create(server.GetFullUrlFor(path));
            var response = request.GetResponse();
            const string contentTypeHeader = "Content-Type";
            var hasContentTypeHeader = response.Headers.AllKeys.Contains(contentTypeHeader);
            contentType = hasContentTypeHeader ? response.Headers[contentTypeHeader] : null;
            using (var s = response.GetResponseStream())
            {
                var memStream = new MemoryStream();
                s.CopyTo(memStream);
                return memStream.ToArray();
            }
        }
    }

}
