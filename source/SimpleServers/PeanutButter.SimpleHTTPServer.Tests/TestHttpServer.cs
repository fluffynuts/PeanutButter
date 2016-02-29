using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Linq;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.SimpleTcpServer;
using PeanutButter.Utils;

namespace PeanutButter.SimpleHTTPServer.Tests
{
    [TestFixture]
    public class TestHttpServer
    {
        [Test]
        public void Construct_WhenPortIsSpecified_ShouldUseThatPort()
        {
            const int maxTries = 20;
            for (var i = 0; i < maxTries; i++)
            {
                var port = RandomValueGen.GetRandomInt(2000, 3000);
                try
                {
                    //---------------Set up test pack-------------------
                    using (var server = Create(port))
                    {
                        //---------------Assert Precondition----------------

                        //---------------Execute Test ----------------------

                        //---------------Test Result -----------------------
                        Assert.AreEqual(port, server.Port);
                    }
                    return;
                }
                catch (PortUnavailableException)
                {
                    Console.WriteLine($"Port {port} is currently not available");
                }
            }
            Assert.Fail($"Unable to bind to any specified port in {maxTries} attempts");
        }

        [Test]
        public void Construct_WhenPortIsNotSpecified_ShouldChooseRandomPortBetween_5000_And_50000()
        {
            //---------------Set up test pack-------------------
            for (var i = 0; i < 50; i++)
                using (var server = Create())
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
            using (var server = Create())
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
            using (var server = Create())
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
            using (var server = Create())
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
            using (var server = Create())
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
            using (var server = Create())
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
            using (var server = Create())
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

        [Test]
        public void Download_GivenDownloadInfoAndOutputFolder_WhenDownloadUninterrupted_ShouldDownloadFileToOutputFolder()
        {
            using (var disposer = new AutoDisposer())
            {
                var deleter = disposer.Add(new AutoDeleter());
                var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                var fileName = RandomValueGen.GetRandomAlphaNumericString(5, 10) + ".exe";
                deleter.Add(tempFolder);
                var expectedFile = Path.Combine(tempFolder, fileName);
                var server = Create();
                //---------------Set up test pack-------------------
                var url = server.GetFullUrlFor(fileName);
                var expectedBytes = RandomValueGen.GetRandomBytes(100, 200);
                server.AddFileHandler((processor, stream) =>
                {
                    if (processor.FullUrl == "/" + fileName)
                    {
                        return expectedBytes;
                    }
                    processor.WriteStatusHeader(404, "File not found");
                    return null;
                });

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Download(url, fileName, tempFolder);

                //---------------Test Result -----------------------
                Assert.IsTrue(File.Exists(expectedFile));
                CollectionAssert.AreEquivalent(expectedBytes, File.ReadAllBytes(expectedFile));
            }
        }

        private HttpServer Create(int? port = null)
        {
            var result = CreateWithPort(port);
            return result;
        }

        private HttpServer CreateWithPort(int? port)
        {
            return port.HasValue
                ? new HttpServer(port.Value, logAction: Console.WriteLine)
                : new HttpServer(logAction: Console.WriteLine);
        }

        private const string CONTENT_LENGTH_HEADER = "Content-Length";

        public string Download(string url, string fileName, string destPath)
        {
            var outFile = Path.Combine(destPath, fileName);
            var req = WebRequest.Create(url) as HttpWebRequest;
            long existingSize = 0;
            if (File.Exists(outFile))
            {
                var fullDownloadSize = GetContentLengthFor(url);
                var finfo = new FileInfo(outFile);
                existingSize = finfo.Length;
                if (fullDownloadSize == existingSize)
                {
                    Console.WriteLine("Already fully downloaded");
                    return outFile;
                }
                Console.WriteLine("Resuming download from byte: {0}", finfo.Length);
                req.AddRange(finfo.Length);
            }
            req.Timeout = 90000;
            using (var response = req.GetResponse())
            {
                var expectedSize = long.Parse(response.Headers[CONTENT_LENGTH_HEADER]);
                Console.WriteLine("Should get {0} bytes to {1}", expectedSize, outFile);
                DownloadFile(response, outFile, expectedSize, expectedSize + existingSize, existingSize);
                return outFile;
            }
        }

        private long GetContentLengthFor(string downloadUrl)
        {
            var req = WebRequest.Create(downloadUrl) as HttpWebRequest;
            using (var response = req.GetResponse())
            {
                return long.Parse(response.Headers[CONTENT_LENGTH_HEADER]);
            }
        }
        private void DownloadFile(WebResponse response, string outFile, long expectedSize, long totalSize, long offset)
        {
            var parentFolder = Path.GetDirectoryName(outFile);
            if (!Directory.Exists(parentFolder))
                Directory.CreateDirectory(parentFolder);
            var started = DateTime.Now;
            using (var reader = new BinaryReader(response.GetResponseStream()))
            {
                using (var outStream = new FileStream(outFile, FileMode.Append))
                {
                    var haveRead = 0;
                    using (var writer = new BinaryWriter(outStream))
                    {
                        while (haveRead < expectedSize)
                        {
                            var toRead = expectedSize - haveRead;
                            if (toRead > 8192)
                                toRead = 8192;
                            var readBuf = reader.ReadBytes((int)toRead);
                            haveRead += readBuf.Length;
                            writer.Write(readBuf, 0, readBuf.Length);
                            writer.Flush();
                        }
                    }
                }
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
