using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using PeanutButter.SimpleTcpServer;
using PeanutButter.Utils;
using NExpect;
using static NExpect.Expectations;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable PossibleNullReferenceException
// ReSharper disable AssignNullToNotNullAttribute

namespace PeanutButter.SimpleHTTPServer.Tests
{
    [TestFixture]
    public class TestHttpServer
    {
        [TestFixture]
        public class Construction
        {
            [Test]
            public void WhenPortIsSpecified_ShouldUseThatPort()
            {
                const int maxTries = 20;
                for (var i = 0; i < maxTries; i++)
                {
                    var port = GetRandomInt(2000, 3000);
                    try
                    {
                        //---------------Set up test pack-------------------
                        using (var server = Create(port))
                        {
                            //---------------Assert Precondition----------------

                            //---------------Execute Test ----------------------

                            //---------------Test Result -----------------------
                            Expect(server.Port)
                                .To.Equal(port);
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
            [Repeat(10)]
            public void WhenPortIsNotSpecified_ShouldChooseRandomPortBetween_5000_And_50000()
            {
                //---------------Set up test pack-------------------
                using var server = Create();
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------

                //---------------Test Result -----------------------
                Expect(server.Port)
                    .To.Be.Greater.Than.Or.Equal.To(5000)
                    .And
                    .To.Be.Less.Than.Or.Equal.To(32768);
                Console.WriteLine("Server started on port: " + server.Port);
            }
        }

        [TestFixture]
        [Parallelizable]
        public class AddHtmlDocumentHandler
        {
            [Test]
            public void ShouldServeDocument()
            {
                //---------------Set up test pack-------------------
                var doc = $"<html><head></head><body><p>{GetRandomAlphaNumericString()}</p></body></html>";
                const string theDocName = "index.html";
                using var server = GlobalSetup.Pool.Take();
                server.Instance.AddHtmlDocumentHandler(
                    (p, _) => p.Path == "/" + theDocName
                        ? doc
                        : null);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = DownloadResultFrom(
                    server.Instance, theDocName).ToUTF8String();

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(doc);
            }
        }

        [TestFixture]
        [Parallelizable]
        public class AddDocumentHandler
        {
            [Test]
            public void ShouldSetContentTypeFromResult()
            {
                // Arrange
                using var poolItem = GlobalSetup.Pool.Take();
                var server = poolItem.Instance;
                var html1 = $"<html><head></head><body></body>{GetRandomAlphaNumericString()}</html>";
                var html2 = $"<!DOCTYPE html><html><head></head><body></body>{GetRandomAlphaNumericString()}</html>";
                var json = $"{{ id: 1, name: \"{GetRandomAlphaNumericString()}\" }}";
                var text = GetRandomAlphaNumericString();
                var xml1 = "<?xml version=\"1.0\" encoding=\"UTF-9\"?><doc><item>moo</item></doc>";
                var xml2 = "<doc><item>moo</item></doc>";
                // Pre-Assert

                // Act
                server.AddDocumentHandler(
                    (p, _) => p.Path == "/html1"
                        ? html1
                        : null);
                server.AddDocumentHandler(
                    (p, _) => p.Path == "/html2"
                        ? html2
                        : null);
                server.AddDocumentHandler(
                    (p, _) => p.Path == "/json"
                        ? json
                        : null);
                server.AddDocumentHandler(
                    (p, _) => p.Path == "/text"
                        ? text
                        : null);
                server.AddDocumentHandler(
                    (p, _) => p.Path == "/xml1"
                        ? xml1
                        : null);
                server.AddDocumentHandler(
                    (p, _) => p.Path == "/xml2"
                        ? xml2
                        : null);
                DownloadResultFrom(
                    server,
                    HttpMethods.Get,
                    "/html1",
                    null,
                    out var htmlContentType1);
                DownloadResultFrom(
                    server,
                    HttpMethods.Get,
                    "/html2",
                    null,
                    out var htmlContentType2);
                DownloadResultFrom(server, HttpMethods.Get, "/json", null, out var jsonContentType);
                DownloadResultFrom(server, HttpMethods.Get, "/text", null, out var textContentType);
                DownloadResultFrom(
                    server,
                    HttpMethods.Get,
                    "/xml1",
                    null,
                    out var xmlContentType1);
                DownloadResultFrom(
                    server,
                    HttpMethods.Get,
                    "/xml2",
                    null,
                    out var xmlContentType2);

                // Assert
                Expect(htmlContentType1).To.Equal("text/html");
                Expect(htmlContentType2).To.Equal("text/html");
                Expect(xmlContentType1).To.Equal("text/xml");
                Expect(xmlContentType2).To.Equal("text/xml");
                Expect(jsonContentType).To.Equal("application/json");
                Expect(textContentType).To.Equal("application/octet-stream");
            }
        }

        [TestFixture]
        [Parallelizable]
        public class AddJsonDocumentHandler
        {
            public class PostBody
            {
                public int Id { get; set; }
                public string Name { get; set; }
            }

            [TestCase("POST")]
            [TestCase("PUT")]
            [TestCase("DELETE")]
            [TestCase("PATCH")]
            public async Task GettingRequestBody(string method)
            {
                // Arrange
                using var poolItem = GlobalSetup.Pool.Take();
                var server = poolItem.Instance;
                server.LogAction = Console.WriteLine;
                var poco = new
                {
                    id = 1,
                    name = "moo"
                };
                var path = $"/{method.ToLower()}";
                var expectedBody = JsonConvert.SerializeObject(poco);
                string body = null;
                PostBody bodyObject = null;
                server.AddJsonDocumentHandler(
                    (processor, stream) =>
                    {
                        if (processor.Path != path)
                            return null;
                        body = stream.AsString();
                        bodyObject = stream.As<PostBody>();
                        return new
                        {
                            id = 1,
                            body
                        };
                    });
                // Pre-assert
                // Act
                var client = new HttpClient();
                var message = new HttpRequestMessage(
                    new HttpMethod(method),
                    server.GetFullUrlFor(path))
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(poco)
                    )
                };
                await client.SendAsync(message);
                // Assert
                Expect(body).To.Equal(expectedBody);
                Expect(bodyObject).Not.To.Be.Null();
                Expect(bodyObject.Id).To.Equal(poco.id);
                Expect(bodyObject.Name).To.Equal(poco.name);
            }

            [TestCase("POST")]
            [TestCase("PUT")]
            [TestCase("DELETE")]
            [TestCase("PATCH")]
            public async Task GettingRequestBodyWhenChunked(string method)
            {
                // HttpClient in net6.0 (netstandard perhaps?) will 
                // send the body chunked when using ObjectContent - where net462
                // will not
                // Arrange
                var poolItem = GlobalSetup.Pool.Take();
                var server = poolItem.Instance;
                server.LogAction = Console.WriteLine;
                var poco = new
                {
                    id = 1,
                    name = "moo"
                };
                var path = $"/{method.ToLower()}";
                var expectedBody = JsonConvert.SerializeObject(poco);
                string body = null;
                PostBody bodyObject = null;
                server.AddJsonDocumentHandler(
                    (processor, stream) =>
                    {
                        if (processor.Path != path)
                            return null;
                        body = stream.AsString();
                        bodyObject = stream.As<PostBody>();
                        return new
                        {
                            id = 1,
                            body
                        };
                    });
                // Pre-assert
                // Act
                var client = new HttpClient();
                var message = new HttpRequestMessage(
                    new HttpMethod(method),
                    server.GetFullUrlFor(path))
                {
                    Content = new ObjectContent<object>(
                        poco,
                        new JsonMediaTypeFormatter())
                };
                await client.SendAsync(message);
                // Assert
                Expect(body).To.Equal(expectedBody);
                Expect(bodyObject).Not.To.Be.Null();
                Expect(bodyObject.Id).To.Equal(poco.id);
                Expect(bodyObject.Name).To.Equal(poco.name);
            }


            [Test]
            public void ShouldServeJsonDocument()
            {
                //---------------Set up test pack-------------------
                var expected = GetRandomString();
                var data = new SimpleData { SomeProperty = expected };
                var route = "api/foo";
                using var server = GlobalSetup.Pool.Take();
                server.Instance.AddJsonDocumentHandler(
                    (p, _) => p.Path == "/" + route
                        ? data
                        : null);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------

                var stringResult = DownloadResultFrom(
                    server.Instance,
                    route,
                    null,
                    out string _
                ).ToUTF8String();

                //---------------Test Result -----------------------
                var resultAsObject = JsonConvert.DeserializeObject<SimpleData>(stringResult);
                Expect(resultAsObject)
                    .Not.To.Be.Null();
                Expect(resultAsObject.SomeProperty)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        [Parallelizable]
        public class ServeFile
        {
            [Test]
            public void WhenConfiguredToServeFile_ShouldReturnTheFileContents()
            {
                //---------------Set up test pack-------------------
                var theFile = GetRandomBytes(100, 200);
                const string theFileName = "somefile.bin";
                using var server = GlobalSetup.Pool.Take();
                //---------------Assert Precondition----------------
                server.Instance.AddFileHandler(
                    (p, _) => p.Path == "/" + theFileName
                        ? theFile
                        : null);
                //---------------Execute Test ----------------------
                var result = DownloadResultFrom(server, theFileName);
                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(theFile);
            }

            [Test]
            public void WhenConfiguredToServeFileViaFunc_ShouldReturnTheFileContents()
            {
                //---------------Set up test pack-------------------
                var theFile = GetRandomBytes(100, 200);
                const string theFileName = "somefile.bin";
                using var server = GlobalSetup.Pool.Take();
                //---------------Assert Precondition----------------
                server.Instance.ServeFile("/" + theFileName, () => theFile, contentType: "application/octet-stream");
                //---------------Execute Test ----------------------
                var result = DownloadResultFrom(server, theFileName);
                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(theFile);
            }

            [Test]
            public void GivenPathAndDataAndContentType_ShouldServeForThatPath()
            {
                using var server = GlobalSetup.Pool.Take();
                //---------------Set up test pack-------------------
                var data = GetRandomBytes(10, 100);
                var contentType = "text/" + GetRandomFrom(new[] { "xml", "html", "javascript", "plain" });
                server.Instance.ServeFile("/file.bin", data, contentType);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = DownloadResultFrom(server, "/file.bin", null, out var servedType);

                //---------------Test Result -----------------------
                Expect(result)
                    .To.Equal(data);
                Expect(servedType)
                    .To.Equal(contentType);
            }

            [Test]
            public void GivenPathAndData_ShouldGive404ForOtherPaths()
            {
                using var server = GlobalSetup.Pool.Take();
                //---------------Set up test pack-------------------
                var data = GetRandomBytes(10, 100);
                var contentType = "text/" + GetRandomFrom(new[] { "xml", "html", "javascript", "plain" });
                server.Instance.ServeFile("/file.bin", data, contentType);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Expect(() => DownloadResultFrom(server, "/file1.bin"))
                    .To.Throw<WebException>()
                    .With.Property(e => e.Response as HttpWebResponse)
                    .Matched.By(r => r.StatusCode == HttpStatusCode.NotFound);

                //---------------Test Result -----------------------
            }
        }

        [TestFixture]
        public class FullUrl
        {
            [Test]
            public void ShouldProduceTheFullUrlForTheRequest()
            {
                // Arrange
                using var server = GlobalSetup.Pool.Take();
                var captured = null as string;
                server.Instance.AddJsonDocumentHandler((p, s) =>
                {
                    captured = p.FullUrl;
                    return new SimpleData()
                    {
                        SomeProperty = "Hello"
                    };
                });
                var path = "/foo/bar/quux?id=123";
                var expected = server.Instance.GetFullUrlFor(path);
                // Act
                DownloadResultFrom(server, path);
                // Assert
                Expect(captured)
                    .To.Equal(expected);
            }
        }

        [TestFixture]
        [Parallelizable]
        public class ServeDocument
        {
            [Test]
            public void ServeDocument_GivenPathAndDocument_ShouldServeForThatPath()
            {
                using var server = GlobalSetup.Pool.Take();
                //---------------Set up test pack-------------------
                var doc = new XDocument(
                    new XElement(
                        "html",
                        new XElement("body", new XElement("p", new XText(GetRandomString())))));
                server.Instance.ServeDocument("/index.html", doc);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = DownloadResultFrom(server, "/index.html", null, out var contentType);

                //---------------Test Result -----------------------
                Expect(result.ToUTF8String())
                    .To.Equal(doc.ToString());
                Expect(contentType)
                    .To.Equal("text/html");
            }

            [TestCase(HttpMethods.Get)]
            [TestCase(HttpMethods.Post)]
            public void GivenPathAndDocumentAndVerb_ShouldServeForThatPathAndDocumentAndVerb(
                HttpMethods serveMethod)
            {
                using var server = GlobalSetup.Pool.Take();
                //---------------Set up test pack-------------------
                var invalidMethod = serveMethod == HttpMethods.Get
                    ? HttpMethods.Post
                    : HttpMethods.Get;
                var doc = new XDocument(
                    new XElement(
                        "html",
                        new XElement("body", new XElement("p", new XText(GetRandomString())))));
                var path = "/index.html";
                server.Instance.ServeDocument(path, doc, serveMethod);

                //---------------Assert Precondition----------------
                Expect(serveMethod)
                    .Not.To.Equal(invalidMethod);

                //---------------Execute Test ----------------------
                Console.WriteLine("Attempt to download path: " + path);
                string contentType;
                var ex = Assert.Throws<WebException>(
                    () =>
                        DownloadResultFrom(server, invalidMethod, path, null, out contentType));
                var webResponse = ex.Response as HttpWebResponse;
                Expect(ex.Message)
                    .Not.To.Be.Null();
                var statusCode = webResponse.StatusCode;
                Expect(statusCode)
                    .To.Equal(HttpStatusCode.NotFound);
                var result = DownloadResultFrom(server, serveMethod, path, null, out contentType);

                //---------------Test Result -----------------------
                Expect(result.ToUTF8String())
                    .To.Equal(doc.ToString());
                Expect(contentType)
                    .To.Equal("text/html");
            }

            [Test]
            public void WhenConfiguredToServeXDocumentFromPathWithParameters_ShouldServeDocument()
            {
                //---------------Set up test pack-------------------
                var doc = XDocument.Parse(
                    "<html><head></head><body><p>" +
                    GetRandomAlphaNumericString() +
                    "</p></body></html>");
                const string theDocName = "/index?foo=bar";
                using var server = GlobalSetup.Pool.Take();
                server.Instance.ServeDocument(theDocName, doc, HttpMethods.Get);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = DownloadResultFrom(server, theDocName).ToUTF8String();

                //---------------Test Result -----------------------
                Expect(doc.ToString(SaveOptions.DisableFormatting))
                    .To.Equal(
                        XDocument.Parse(result)
                            .ToString(SaveOptions.DisableFormatting)
                    );
            }

            [Test]
            public void WhenConfiguredToServeXmlFromPathWithParameters_ShouldServeDocument()
            {
                //---------------Set up test pack-------------------
                var doc = XDocument.Parse(
                    "<html><head></head><body><p>" +
                    GetRandomAlphaNumericString() +
                    "</p></body></html>");
                const string theDocName = "/index?foo=bar";
                using var server = GlobalSetup.Pool.Take();
                server.Instance.ServeDocument(theDocName, doc.ToString(), HttpMethods.Get);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = DownloadResultFrom(server, theDocName).ToUTF8String();

                //---------------Test Result -----------------------
                Expect(doc.ToString(SaveOptions.DisableFormatting))
                    .To.Equal(
                        XDocument.Parse(result).ToString(SaveOptions.DisableFormatting)
                    );
            }

            [Test]
            public void WhenConfiguredToServeXmlViaFuncFromPathWithParameters_ShouldServeDocument()
            {
                //---------------Set up test pack-------------------
                var doc = XDocument.Parse(
                    "<html><head></head><body><p>" +
                    GetRandomAlphaNumericString() +
                    "</p></body></html>");
                const string theDocName = "/index?foo=bar";
                using var server = GlobalSetup.Pool.Take();
                server.Instance.ServeDocument(theDocName, () => doc.ToString(), HttpMethods.Get);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = DownloadResultFrom(server, theDocName).ToUTF8String();

                //---------------Test Result -----------------------
                Expect(doc.ToString(SaveOptions.DisableFormatting))
                    .To.Equal(
                        XDocument.Parse(result).ToString(SaveOptions.DisableFormatting)
                    );
            }

            [Test]
            public void WhenConfiguredToServeXDocumentFromPathWithParametersForFunc_ShouldServeDocument()
            {
                //---------------Set up test pack-------------------
                var doc = XDocument.Parse(
                    "<html><head></head><body><p>" +
                    GetRandomAlphaNumericString() +
                    "</p></body></html>");
                const string theDocName = "/index?foo=bar";
                using var server = GlobalSetup.Pool.Take();
                server.Instance.ServeDocument(theDocName, () => doc, HttpMethods.Get);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = DownloadResultFrom(server, theDocName).ToUTF8String();

                //---------------Test Result -----------------------
                Expect(doc.ToString(SaveOptions.DisableFormatting))
                    .To.Equal(
                        XDocument.Parse(result).ToString(SaveOptions.DisableFormatting)
                    );
            }

            [Test]
            public void GivenPathAndDocument_ShouldGive404ForOtherPaths()
            {
                using var server = GlobalSetup.Pool.Take();
                //---------------Set up test pack-------------------
                var doc = new XDocument(
                    new XElement(
                        "html",
                        new XElement("body", new XElement("p", new XText(GetRandomString())))));
                server.Instance.ServeDocument("/index.html", doc);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Expect(() => DownloadResultFrom(server, "/index1.html"))
                    .To.Throw<WebException>()
                    .With.Property(e => e.Response as HttpWebResponse)
                    .Matched.By(r => r.StatusCode == HttpStatusCode.NotFound);

                //---------------Test Result -----------------------
            }
        }


        [TestFixture]
        [Parallelizable]
        public class ServeJsonDocument
        {
            [Test]
            public void GivenPathAndDocument_ShouldServeForThatPath()
            {
                using var server = GlobalSetup.Pool.Take();
                //---------------Set up test pack-------------------
                var obj = GetRandom<SimpleData>();
                server.Instance.ServeJsonDocument("/api/query", obj);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = DownloadResultFrom(
                    server.Instance,
                    "/api/query",
                    null,
                    out var contentType
                );

                //---------------Test Result -----------------------
                var resultAsObject = JsonConvert.DeserializeObject<SimpleData>(result.ToUTF8String());
                Expect(result)
                    .Not.To.Be.Null();
                Expect(resultAsObject.SomeProperty)
                    .To.Equal(obj.SomeProperty);
                Expect(contentType)
                    .To.Equal("application/json");
            }

            [Test]
            public void ServeJsonDocument_GivenPathAndDocumentFactory_ShouldServeForThatPath()
            {
                using var server = GlobalSetup.Pool.Take();
                //---------------Set up test pack-------------------
                var obj = GetRandom<SimpleData>();
                server.Instance.ServeJsonDocument("/api/query", () => obj);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = DownloadResultFrom(
                    server.Instance, "/api/query", null, out var contentType);

                //---------------Test Result -----------------------
                var resultAsObject = JsonConvert.DeserializeObject<SimpleData>(result.ToUTF8String());
                Expect(result)
                    .Not.To.Be.Null();
                Expect(resultAsObject.SomeProperty)
                    .To.Equal(obj.SomeProperty);
                Expect(contentType)
                    .To.Equal("application/json");
            }

            [Test]
            public void GivenPathWithParametersAndDocument_ShouldServeForThatPathWithParameters()
            {
                using var server = GlobalSetup.Pool.Take();
                //---------------Set up test pack-------------------
                var obj = GetRandom<SimpleData>();
                var path = "/api/query?option=value";
                server.Instance.ServeJsonDocument(path, obj);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = DownloadResultFrom(
                    server.Instance, path, null, out var contentType);

                //---------------Test Result -----------------------
                var resultAsObject = JsonConvert.DeserializeObject<SimpleData>(result.ToUTF8String());
                Expect(result)
                    .Not.To.Be.Null();
                Expect(resultAsObject.SomeProperty)
                    .To.Equal(obj.SomeProperty);
                Expect(contentType)
                    .To.Equal("application/json");
            }

            [TestCase(HttpMethods.Get)]
            [TestCase(HttpMethods.Post)]
            public void GivenPathAndDocumentAndVerb_ShouldServeForThatPathAndVerbOnly(
                HttpMethods valid)
            {
                using var server = GlobalSetup.Pool.Take();
                //---------------Set up test pack-------------------
                var invalid = valid == HttpMethods.Get
                    ? HttpMethods.Post
                    : HttpMethods.Get;
                var obj = GetRandom<SimpleData>();
                var path = "/api/" + GetRandomString();
                server.Instance.ServeJsonDocument(path, obj, valid);

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                string contentType;
                Expect(() => DownloadResultFrom(
                        server.Instance, invalid, path, null, out contentType))
                    .To.Throw<WebException>();
                var result = DownloadResultFrom(
                    server.Instance, valid, path, null, out contentType);

                //---------------Test Result -----------------------
                var resultAsObject = JsonConvert.DeserializeObject<SimpleData>(result.ToUTF8String());
                Expect(result)
                    .Not.To.Be.Null();
                Expect(resultAsObject.SomeProperty)
                    .To.Equal(obj.SomeProperty);
                Expect(contentType)
                    .To.Equal("application/json");
            }
        }

        [Test]
        [Parallelizable]
        public void
            Download_GivenDownloadInfoAndOutputFolder_WhenDownloadUninterrupted_ShouldDownloadFileToOutputFolder()
        {
            using var disposer = new AutoDisposer();
            var deleter = disposer.Add(new AutoDeleter());
            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var fileName = GetRandomAlphaNumericString(5, 10) + ".exe";
            deleter.Add(tempFolder);
            var expectedFile = Path.Combine(tempFolder, fileName);
            var server = disposer.Add(Create());
            //---------------Set up test pack-------------------
            var url = server.GetFullUrlFor(fileName);
            var expectedBytes = GetRandomBytes(100, 200);
            server.AddFileHandler(
                (processor, _) =>
                {
                    if (processor.Path == "/" + fileName)
                    {
                        return expectedBytes;
                    }

                    processor.WriteStatusHeader(HttpStatusCode.NotFound, "File not found");
                    return null;
                });

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Download(url, fileName, tempFolder);

            //---------------Test Result -----------------------
            Expect(expectedFile)
                .To.Exist();
            Expect(File.ReadAllBytes(expectedFile))
                .To.Equal(expectedBytes);
        }

        [Test]
        [Parallelizable]
        public void WhenNoHandlerClaimsPath_ShouldReturn404()
        {
            //---------------Set up test pack-------------------
            using var server = GlobalSetup.Pool.Take();
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => DownloadResultFrom(server, "/index.html"))
                .To.Throw<WebException>()
                .With.Property(e => e.Response as HttpWebResponse)
                .Matched.By(r => r.StatusCode == HttpStatusCode.NotFound);

            //---------------Test Result -----------------------
        }

        [Test]
        [Parallelizable]
        public void WhenDocumentHandlerThrows_ShouldReturn500()
        {
            //---------------Set up test pack-------------------
            using var server = GlobalSetup.Pool.Take();
            var message = GetRandomString();
            var logs = new List<string>();
            server.Instance.LogAction = logs.Add;
            server.Instance.AddHtmlDocumentHandler(
                (_, _) => throw new Exception(message)
            );
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => DownloadResultFrom(server, "/index.html"))
                .To.Throw<WebException>()
                .With.Property(e => e.Response as HttpWebResponse)
                .Matched.By(r => r.StatusCode == HttpStatusCode.InternalServerError);

            //---------------Test Result -----------------------
            Expect(logs)
                .To.Contain.Any
                .Matched.By(l => l.Contains(message));
        }

        [Test]
        [Parallelizable]
        public void RequestLogAction_ShouldLogRequests()
        {
            //---------------Set up test pack-------------------
            using var server = GlobalSetup.Pool.Take();
            var requestLogs = new List<RequestLogItem>();
            server.Instance.RequestLogAction = requestLogs.Add;
            var path = "/index.html";
            server.Instance.ServeDocument(path, new XDocument());
            var key = "X-" + GetRandomString();
            var headers = new Dictionary<string, string>()
            {
                { key, GetRandomString() }
            };
            //---------------Assert Precondition----------------
            Expect(requestLogs)
                .To.Be.Empty();

            //---------------Execute Test ----------------------
            DownloadResultFrom(server, path, headers);

            //---------------Test Result -----------------------
            var log = requestLogs.Single();

            Expect(log.Path)
                .To.Equal(path);
            Expect(log.StatusCode)
                .To.Equal(HttpStatusCode.OK);
            Expect(log.Method)
                .To.Equal("GET");
            Expect(log.Message)
                .To.Equal("OK");
            Expect(log.Headers)
                .To.Contain.Key(key)
                .With.Value(headers[key]);
        }

        [Test]
        [Parallelizable]
        public async Task ShouldPopulateUrlParameters()
        {
            // Arrange
            using var server = GlobalSetup.Pool.Take();
            var capturedParams = new Dictionary<string, string>();
            server.Instance.AddJsonDocumentHandler(
                (processor, _) =>
                {
                    processor.UrlParameters.ForEach(
                        kvp => capturedParams.Add(kvp.Key, kvp.Value));
                    return new[] { 1, 2, 3 };
                });
            var client = new HttpClient()
            {
                BaseAddress = new Uri(server.Instance.BaseUrl)
            };
            // Pre-assert
            // Act
            var message = new HttpRequestMessage(HttpMethod.Get, "endpoint?param1=value1&param2=value2");
            await client.SendAsync(message);
            // Assert
            Expect(capturedParams).Not.To.Be.Empty();
            Expect(capturedParams).To.Contain.Key("param1")
                .With.Value("value1");
            Expect(capturedParams).To.Contain.Key("param2")
                .With.Value("value2");
        }

        [Test]
        [Parallelizable]
        public void ShouldReturn500WhenHandlerThrows()
        {
            // Arrange
            using var server = GlobalSetup.Pool.Take();
            var exceptionMessage = GetRandomString(10);
            server.Instance.AddHandler((_, _) => throw new Exception(exceptionMessage));

#pragma warning disable SYSLIB0014
            var req = WebRequest.Create(server.Instance.GetFullUrlFor("/moo.jpg"));
#pragma warning restore SYSLIB0014
            try
            {
                using var res = req.GetResponse() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse httpWebResponse)
                {
                    Expect(httpWebResponse.StatusCode)
                        .To.Equal(HttpStatusCode.InternalServerError);
                    using var res = httpWebResponse.GetResponseStream();
                    var body = Encoding.UTF8.GetString(res.ReadAllBytes());
                    Expect(body)
                        .To.Contain(exceptionMessage)
                        // should have some stack trace info, though what exactly
                        // will depend on the runtime
                        .Then(nameof(TestHttpServer));
                }
                else
                {
                    throw;
                }
            }

            // Assert
        }

        [TestFixture]
        [Parallelizable]
        public class ReUsingServerByClearingAndReRegisteringHandlers
        {
            private HttpServer _server;

            [OneTimeSetUp]
            public void OnetimeSetup()
            {
                _server = new HttpServer();
            }

            [OneTimeTearDown]
            public void OnetimeTeardown()
            {
                _server.Dispose();
            }

            [TearDown]
            public void Teardown()
            {
                _server.Reset();
            }

            [Test]
            public void ShouldGetFile1()
            {
                // Arrange
                var expected = GetRandomString();
                _server.AddDocumentHandler((processor, _) =>
                    processor.Path == "/index.html"
                        ? expected
                        : null
                );
                // Act
                var result = DownloadResultFrom(_server, "/index.html");
                // Assert
                var asString = Encoding.UTF8.GetString(result);
                Expect(asString).To.Equal(expected);
            }

            [Test]
            public void ShouldGetFile2()
            {
                // Arrange
                var expected = GetRandomString();
                _server.AddDocumentHandler((processor, _) =>
                    processor.Path == "/index.html"
                        ? expected
                        : null
                );
                // Act
                var result = DownloadResultFrom(_server, "/index.html");
                // Assert
                var asString = Encoding.UTF8.GetString(result);
                Expect(asString).To.Equal(expected);
            }
        }

        // [TestFixture]
        // public class WildIssues
        // {
        //     [TestFixture]
        //     public class SuddenEndOfStreamFail
        //     {
        //         [TestCase("POST")]
        //         [TestCase("GET")]
        //         public void ShouldSendRequestWithMethod_(string expectedMethod)
        //         {
        //             // Arrange
        //             var requestPathAndParameters = GetRandomHttpPathAndParameters();
        //             var expectedResult = GetRandomWords();
        //             var capturedPath = null as string;
        //             var capturedParameters = new Dictionary<string, string>();
        //             var capturedMethod = null as string;
        //             var capturedHeaders = new Dictionary<string, string>();
        //             using var server = Create();
        //             server.AddHandler((processor, _) =>
        //             {
        //                 capturedMethod = processor.Method;
        //                 capturedParameters = processor.UrlParameters;
        //                 capturedPath = processor.Path;
        //                 capturedHeaders = processor.HttpHeaders;
        //                 processor.WriteDocument(expectedResult);
        //                 return HttpServerPipelineResult.HandledExclusively;
        //             });
        //             var url = server.GetFullUrlFor(requestPathAndParameters);
        //             var uri = new Uri(url);
        //             var expectedParameters = uri.ParseQueryString().ToDictionary();
        //             var expectedPath = uri.AbsolutePath;
        //
        //             var sut = new HttpRequestExecutor();
        //             // Act
        //             var result = sut.ExecuteRequest(url, expectedMethod);
        //
        //             // Assert
        //             Expect(capturedParameters)
        //                 .To.Equal(expectedParameters);
        //             Expect(capturedPath)
        //                 .To.Equal(expectedPath);
        //             Expect(result.StatusCode)
        //                 .To.Equal(HttpStatusCode.OK);
        //             Expect(result.RawResponse)
        //                 .To.Equal(expectedResult);
        //             Expect(capturedHeaders)
        //                 .To.Contain.Key("Connection")
        //                 .With.Value("Keep-Alive");
        //             Expect(capturedHeaders)
        //                 .To.Contain.Key("Accept-Encoding")
        //                 .With.Value("gzip, deflate");
        //         }
        //
        //         /// <summary>
        //         /// The request result
        //         /// </summary>
        //         public class RequestResult
        //         {
        //             public HttpStatusCode StatusCode { get; }
        //
        //             public string RawResponse { get; }
        //
        //             public RequestResult(HttpStatusCode statusCode, string rawResponse)
        //             {
        //                 StatusCode = statusCode;
        //                 RawResponse = rawResponse;
        //             }
        //         }
        //
        //         public interface IHttpRequestExecutor
        //         {
        //             RequestResult ExecuteRequest(string url, string method = "POST");
        //         }
        //
        //         public class HttpRequestExecutor
        //             : IHttpRequestExecutor
        //         {
        //             private static readonly HttpClient HttpClient;
        //
        //             static HttpRequestExecutor()
        //             {
        //                 HttpClient = new HttpClient()
        //                 {
        //                     DefaultRequestHeaders =
        //                     {
        //                         { "Connection", "Keep-Alive" },
        //                         { "Accept-Encoding", "gzip, deflate" }
        //                     }
        //                 };
        //             }
        //
        //             public RequestResult ExecuteRequest(string url, string method = "POST")
        //             {
        //                 var message = new HttpRequestMessage()
        //                 {
        //                     RequestUri = new Uri(url),
        //                     Method = ResolveHttpMethodFor(method),
        //                     Content = null
        //                 };
        //                 var response = HttpClient.Send(message);
        //                 using var contentStream = response.Content.ReadAsStream();
        //                 return new RequestResult(response.StatusCode, contentStream.ReadAllText());
        //             }
        //
        //             private HttpMethod ResolveHttpMethodFor(string method)
        //             {
        //                 return method?.ToLower() switch
        //                 {
        //                     "get" => HttpMethod.Get,
        //                     "post" => HttpMethod.Post,
        //                     "put" => HttpMethod.Put,
        //                     "patch" => HttpMethod.Patch,
        //                     "delete" => HttpMethod.Delete,
        //                     _ => throw new ArgumentException($"'{method}' is not a known http method")
        //                 };
        //             }
        //         }
        //     }
        // }

        private static HttpServer Create(int? port = null)
        {
            var result = CreateWithPort(port);
            return result;
        }

        private static HttpServer CreateWithPort(int? port)
        {
            return port.HasValue
                ? new HttpServer(port.Value, true, Console.WriteLine)
                : new HttpServer(Console.WriteLine);
        }

        private const string CONTENT_LENGTH_HEADER = "Content-Length";

        public string Download(string url, string fileName, string destPath)
        {
            var outFile = Path.Combine(destPath, fileName);
#pragma warning disable SYSLIB0014
            var req = WebRequest.Create(url) as HttpWebRequest;
#pragma warning restore SYSLIB0014
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
            using var response = req.GetResponse();
            var expectedSize = long.Parse(response.Headers[CONTENT_LENGTH_HEADER]);
            Console.WriteLine("Should get {0} bytes to {1}", expectedSize, outFile);
            DownloadFile(response, outFile, expectedSize, expectedSize + existingSize);
            return outFile;
        }

        private long GetContentLengthFor(string downloadUrl)
        {
#pragma warning disable SYSLIB0014
            var req = WebRequest.Create(downloadUrl) as HttpWebRequest;
#pragma warning restore SYSLIB0014
            using var response = req.GetResponse();
            return long.Parse(response.Headers[CONTENT_LENGTH_HEADER]);
        }

        private void DownloadFile(WebResponse response, string outFile, long expectedSize, long totalSize)
        {
            if (totalSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(totalSize));
            var parentFolder = Path.GetDirectoryName(outFile);
            if (!Directory.Exists(parentFolder))
                Directory.CreateDirectory(parentFolder);
            using var reader = new BinaryReader(response.GetResponseStream());
            using var outStream = new FileStream(outFile, FileMode.Append);
            var haveRead = 0;
            using var writer = new BinaryWriter(outStream);
            while (haveRead < expectedSize)
            {
                var toRead = expectedSize - haveRead;
                if (toRead > 8192)
                    toRead = 8192;
                var readBuf = reader.ReadBytes((int) toRead);
                haveRead += readBuf.Length;
                writer.Write(readBuf, 0, readBuf.Length);
                writer.Flush();
            }
        }

        private static byte[] DownloadResultFrom(
            IPoolItem<HttpServer> poolItem,
            string path,
            Dictionary<string, string> addHeaders = null
        )
        {
            return DownloadResultFrom(
                poolItem.Instance,
                path,
                addHeaders
            );
        }

        private static byte[] DownloadResultFrom(HttpServer server,
            string path,
            Dictionary<string, string> addHeaders = null)
        {
            return DownloadResultFrom(server, path, addHeaders, out var _);
        }

        private static byte[] DownloadResultFrom(
            IPoolItem<HttpServer> server,
            HttpMethods method,
            string path,
            Dictionary<string, string> addHeaders,
            out string contentType)
        {
            return DownloadResultFrom(
                server.Instance,
                method,
                path,
                addHeaders,
                out contentType
            );
        }

        private static byte[] DownloadResultFrom(
            HttpServer server,
            HttpMethods method,
            string path,
            Dictionary<string, string> addHeaders,
            out string contentType)
        {
#pragma warning disable SYSLIB0014
            var request = WebRequest.Create(server.GetFullUrlFor(path));
#pragma warning restore SYSLIB0014
            (request as HttpWebRequest).KeepAlive = false;
            request.Method = method.ToString().ToUpper();
            addHeaders?.ForEach(kvp => request.Headers[kvp.Key] = kvp.Value);
            var response = request.GetResponse();
            const string contentTypeHeader = "Content-Type";
            var hasContentTypeHeader = response.Headers.AllKeys.Contains(contentTypeHeader);
            contentType = hasContentTypeHeader
                ? response.Headers[contentTypeHeader]
                : null;
            using var s = response.GetResponseStream();
            var memStream = new MemoryStream();
            s.CopyTo(memStream);
            return memStream.ToArray();
        }


        private static byte[] DownloadResultFrom(
            IPoolItem<HttpServer> poolItem,
            string path,
            Dictionary<string, string> addHeaders,
            out string contentType
        )
        {
            return DownloadResultFrom(
                poolItem.Instance,
                path,
                addHeaders,
                out contentType
            );
        }

        private static byte[] DownloadResultFrom(
            HttpServer server,
            string path,
            Dictionary<string, string> addHeaders,
            out string contentType)
        {
            return DownloadResultFrom(server, HttpMethods.Get, path, addHeaders, out contentType);
        }

        public class SimpleData
        {
            public string SomeProperty { get; set; }
        }
    }
}