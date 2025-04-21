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
using static PeanutButter.RandomGenerators.RandomValueGen;
using PeanutButter.SimpleTcpServer;
using PeanutButter.Utils;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AccessToDisposedClosure
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable PossibleNullReferenceException
// ReSharper disable AssignNullToNotNullAttribute

namespace PeanutButter.SimpleHTTPServer.Tests;

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
                    using var server = Create(port);
                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------

                    //---------------Test Result -----------------------
                    Expect(server.Port)
                        .To.Equal(port);

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
            using var server = GlobalSetup.Pool.Borrow();
            server.Instance.AddHtmlDocumentHandler(
                (p, _) => p.Path == "/" + theDocName
                    ? doc
                    : null
            );
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = DownloadResultFrom(
                server.Instance,
                theDocName
            ).ToUtf8String();

            //---------------Test Result -----------------------
            Expect(result)
                .To.Equal(doc);
        }
    }
    
    [TestCase(HttpVersion.Version11, ExpectedResult = 1)]
    [TestCase(HttpVersion.Version10, ExpectedResult = 0)]
    public int ShouldRespondWithSelectedVersion(HttpVersion selectedVersion)
    {
        //---------------Set up test pack-------------------
        var doc = $"<html><head></head><body><p>{GetRandomAlphaNumericString()}</p></body></html>";
        const string theDocName = "index.html";
        using var server = GlobalSetup.Pool.Borrow();
        server.Instance.Version = selectedVersion;
        server.Instance.AddHtmlDocumentHandler(
            (p, _) => p.Path == "/" + theDocName
                ? doc
                : null
        );
        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        var result = DownloadResultFrom(
            server.Instance,
            HttpMethods.Get,
            theDocName,
            null,
            out _,
            out var frameworkHttpVersion
        ).ToUtf8String();

        //---------------Test Result -----------------------
        return frameworkHttpVersion.Minor;
    }

    [Test]
    public async Task ShouldBeAbleToRemoveHandler()
    {
        // Arrange
        using var sut = Create();
        var expected = GetRandom<Person>();
        var payload = JsonConvert.SerializeObject(expected);
        // Act
        var id = sut.AddJsonDocumentHandler(
            (p, _) =>
            {
                if (p.Path == "/doc")
                {
                    return payload;
                }

                return null;
            }
        );
        var client = new HttpClient();
        var response = await client.GetAsync(sut.GetFullUrlFor("/doc"));
        var raw = await response.Content.ReadAsStringAsync();
        var parsed = JsonConvert.DeserializeObject<Person>(raw);
        Expect(parsed)
            .To.Deep.Equal(expected);
        sut.RemoveHandler(id);

        var result = await client.GetAsync(sut.GetFullUrlFor("/doc"));
        // Assert
        Expect(result.StatusCode)
            .To.Equal(HttpStatusCode.NotFound);
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    [TestFixture]
    [Parallelizable]
    public class AddDocumentHandler
    {
        [Test]
        public void ShouldSetContentTypeFromResult()
        {
            // Arrange
            using var poolItem = GlobalSetup.Pool.Borrow();
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
                    : null
            );
            server.AddDocumentHandler(
                (p, _) => p.Path == "/html2"
                    ? html2
                    : null
            );
            server.AddDocumentHandler(
                (p, _) => p.Path == "/json"
                    ? json
                    : null
            );
            server.AddDocumentHandler(
                (p, _) => p.Path == "/text"
                    ? text
                    : null
            );
            server.AddDocumentHandler(
                (p, _) => p.Path == "/xml1"
                    ? xml1
                    : null
            );
            server.AddDocumentHandler(
                (p, _) => p.Path == "/xml2"
                    ? xml2
                    : null
            );
            DownloadResultFrom(
                server,
                HttpMethods.Get,
                "/html1",
                null,
                out var htmlContentType1,
                out _
            );
            DownloadResultFrom(
                server,
                HttpMethods.Get,
                "/html2",
                null,
                out var htmlContentType2,
                out _
            );
            DownloadResultFrom(server, HttpMethods.Get, "/json", null, out var jsonContentType, out _);
            DownloadResultFrom(server, HttpMethods.Get, "/text", null, out var textContentType, out _);
            DownloadResultFrom(
                server,
                HttpMethods.Get,
                "/xml1",
                null,
                out var xmlContentType1,
                out _
            );
            DownloadResultFrom(
                server,
                HttpMethods.Get,
                "/xml2",
                null,
                out var xmlContentType2,
                out _
            );

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
            using var poolItem = GlobalSetup.Pool.Borrow();
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
                }
            );
            // Pre-assert
            // Act
            var client = new HttpClient();
            var message = new HttpRequestMessage(
                new HttpMethod(method),
                server.GetFullUrlFor(path)
            )
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
            var poolItem = GlobalSetup.Pool.Borrow();
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
                }
            );
            // Pre-assert
            // Act
            var client = new HttpClient();
            var message = new HttpRequestMessage(
                new HttpMethod(method),
                server.GetFullUrlFor(path)
            )
            {
                Content = new ObjectContent<object>(
                    poco,
                    new JsonMediaTypeFormatter()
                )
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
            var data = new SimpleData
            {
                SomeProperty = expected
            };
            var route = "api/foo";
            using var server = GlobalSetup.Pool.Borrow();
            server.Instance.AddJsonDocumentHandler(
                (p, _) => p.Path == "/" + route
                    ? data
                    : null
            );
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------

            var stringResult = DownloadResultFrom(
                server.Instance,
                route,
                null,
                out string _
            ).ToUtf8String();

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
            const string theFileName = "some-file.bin";
            using var server = GlobalSetup.Pool.Borrow();
            //---------------Assert Precondition----------------
            server.Instance.AddFileHandler(
                (p, _) => p.Path == "/" + theFileName
                    ? theFile
                    : null
            );
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
            const string theFileName = "some-file.bin";
            using var server = GlobalSetup.Pool.Borrow();
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
            using var server = GlobalSetup.Pool.Borrow();
            //---------------Set up test pack-------------------
            var data = GetRandomBytes(10, 100);
            var contentType = "text/" + GetRandomFrom(
                new[]
                {
                    "xml",
                    "html",
                    "javascript",
                    "plain"
                }
            );
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
            using var server = GlobalSetup.Pool.Borrow();
            //---------------Set up test pack-------------------
            var data = GetRandomBytes(10, 100);
            var contentType = "text/" + GetRandomFrom(
                new[]
                {
                    "xml",
                    "html",
                    "javascript",
                    "plain"
                }
            );
            server.Instance.ServeFile("/file.bin", data, contentType);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => DownloadResultFrom(server, "/file1.bin"))
                .To.Throw<HttpException>()
                .With.Property(e => e.StatusCode)
                .Equal.To(HttpStatusCode.NotFound);

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
            using var server = GlobalSetup.Pool.Borrow();
            var captured = null as string;
            server.Instance.AddJsonDocumentHandler(
                (p, _) =>
                {
                    captured = p.FullUrl;
                    return new SimpleData()
                    {
                        SomeProperty = "Hello"
                    };
                }
            );
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
            using var server = GlobalSetup.Pool.Borrow();
            //---------------Set up test pack-------------------
            var doc = new XDocument(
                new XElement(
                    "html",
                    new XElement("body", new XElement("p", new XText(GetRandomString())))
                )
            );
            server.Instance.ServeDocument("/index.html", doc);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = DownloadResultFrom(server, "/index.html", null, out var contentType);

            //---------------Test Result -----------------------
            Expect(result.ToUtf8String())
                .To.Equal(doc.ToString());
            Expect(contentType)
                .To.Equal("text/html");
        }

        [TestCase(HttpMethods.Get)]
        [TestCase(HttpMethods.Post)]
        public void GivenPathAndDocumentAndVerb_ShouldServeForThatPathAndDocumentAndVerb(
            HttpMethods serveMethod
        )
        {
            using var server = GlobalSetup.Pool.Borrow();
            //---------------Set up test pack-------------------
            var invalidMethod = serveMethod == HttpMethods.Get
                ? HttpMethods.Post
                : HttpMethods.Get;
            var doc = new XDocument(
                new XElement(
                    "html",
                    new XElement("body", new XElement("p", new XText(GetRandomString())))
                )
            );
            var path = "/index.html";
            server.Instance.ServeDocument(path, doc, serveMethod);

            //---------------Assert Precondition----------------
            Expect(serveMethod)
                .Not.To.Equal(invalidMethod);

            //---------------Execute Test ----------------------
            Console.WriteLine("Attempt to download path: " + path);
            string contentType;
            Expect(() => DownloadResultFrom(server, invalidMethod, path, null, out contentType))
                .To.Throw<HttpException>()
                .With.Property(e => e.StatusCode)
                .Equal.To(HttpStatusCode.NotFound);
            var result = DownloadResultFrom(server, serveMethod, path, null, out contentType);

            //---------------Test Result -----------------------
            Expect(result.ToUtf8String())
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
                "</p></body></html>"
            );
            const string theDocName = "/index?foo=bar";
            using var server = GlobalSetup.Pool.Borrow();
            server.Instance.ServeDocument(theDocName, doc, HttpMethods.Get);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = DownloadResultFrom(server, theDocName).ToUtf8String();

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
                "</p></body></html>"
            );
            const string theDocName = "/index?foo=bar";
            using var server = GlobalSetup.Pool.Borrow();
            server.Instance.ServeDocument(theDocName, doc.ToString(), HttpMethods.Get);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = DownloadResultFrom(server, theDocName).ToUtf8String();

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
                "</p></body></html>"
            );
            const string theDocName = "/index?foo=bar";
            using var server = GlobalSetup.Pool.Borrow();
            server.Instance.ServeDocument(theDocName, () => doc.ToString(), HttpMethods.Get);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = DownloadResultFrom(server, theDocName).ToUtf8String();

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
                "</p></body></html>"
            );
            const string theDocName = "/index?foo=bar";
            using var server = GlobalSetup.Pool.Borrow();
            server.Instance.ServeDocument(theDocName, () => doc, HttpMethods.Get);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = DownloadResultFrom(server, theDocName).ToUtf8String();

            //---------------Test Result -----------------------
            Expect(doc.ToString(SaveOptions.DisableFormatting))
                .To.Equal(
                    XDocument.Parse(result).ToString(SaveOptions.DisableFormatting)
                );
        }

        [Test]
        public void GivenPathAndDocument_ShouldGive404ForOtherPaths()
        {
            using var server = GlobalSetup.Pool.Borrow();
            //---------------Set up test pack-------------------
            var doc = new XDocument(
                new XElement(
                    "html",
                    new XElement("body", new XElement("p", new XText(GetRandomString())))
                )
            );
            server.Instance.ServeDocument("/index.html", doc);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Expect(() => DownloadResultFrom(server, "/index1.html"))
                .To.Throw<HttpException>()
                .With.Property(e => e.StatusCode)
                .Equal.To(HttpStatusCode.NotFound);

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
            using var server = GlobalSetup.Pool.Borrow();
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
            var resultAsObject = JsonConvert.DeserializeObject<SimpleData>(result.ToUtf8String());
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
            using var server = GlobalSetup.Pool.Borrow();
            //---------------Set up test pack-------------------
            var obj = GetRandom<SimpleData>();
            server.Instance.ServeJsonDocument("/api/query", () => obj);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = DownloadResultFrom(
                server.Instance,
                "/api/query",
                null,
                out var contentType
            );

            //---------------Test Result -----------------------
            var resultAsObject = JsonConvert.DeserializeObject<SimpleData>(result.ToUtf8String());
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
            using var server = GlobalSetup.Pool.Borrow();
            //---------------Set up test pack-------------------
            var obj = GetRandom<SimpleData>();
            var path = "/api/query?option=value";
            server.Instance.ServeJsonDocument(path, obj);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = DownloadResultFrom(
                server.Instance,
                path,
                null,
                out var contentType
            );

            //---------------Test Result -----------------------
            var resultAsObject = JsonConvert.DeserializeObject<SimpleData>(result.ToUtf8String());
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
            HttpMethods valid
        )
        {
            using var server = GlobalSetup.Pool.Borrow();
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
            Expect(
                    () => DownloadResultFrom(
                        server.Instance,
                        invalid,
                        path,
                        null,
                        out contentType,
                        out _
                    )
                )
                .To.Throw<HttpException>();
            var result = DownloadResultFrom(
                server.Instance,
                valid,
                path,
                null,
                out contentType,
                out _
            );

            //---------------Test Result -----------------------
            var resultAsObject = JsonConvert.DeserializeObject<SimpleData>(result.ToUtf8String());
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
            }
        );

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
        using var server = GlobalSetup.Pool.Borrow();
        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        Expect(() => DownloadResultFrom(server, "/index.html"))
            .To.Throw<HttpException>()
            .With.Property(e => e.StatusCode)
            .Equal.To(HttpStatusCode.NotFound);

        //---------------Test Result -----------------------
    }

    [Test]
    [Parallelizable]
    public void WhenDocumentHandlerThrows_ShouldReturn500()
    {
        //---------------Set up test pack-------------------
        using var server = GlobalSetup.Pool.Borrow();
        var message = GetRandomString();
        var logs = new List<string>();
        server.Instance.LogAction = logs.Add;
        server.Instance.AddHtmlDocumentHandler(
            (_, _) => throw new Exception(message)
        );
        //---------------Assert Precondition----------------

        //---------------Execute Test ----------------------
        Expect(() => DownloadResultFrom(server, "/index.html"))
            .To.Throw<HttpException>()
            .With.Property(e => e.Response)
            .Matched.By(
                r =>
                    r.StatusCode == HttpStatusCode.InternalServerError &&
                    r.Content
                        .ReadAsStream()
                        .AsString(Encoding.UTF8)
                        .Contains(message)
            );

        //---------------Test Result -----------------------
        Expect(logs)
            .To.Contain.Any
            .Matched.By(l => l.Contains(message));
        Expect(logs)
            .Not.To.Contain.Any
            .Matched.By(l => l.Contains("No handlers"));
    }

    [Test]
    [Parallelizable]
    public void RequestLogAction_ShouldLogRequests()
    {
        //---------------Set up test pack-------------------
        using var server = GlobalSetup.Pool.Borrow();
        var requestLogs = new List<RequestLogItem>();
        server.Instance.RequestLogAction = requestLogs.Add;
        var path = "/index.html";
        server.Instance.ServeDocument(path, new XDocument());
        var key = "X-" + GetRandomString();
        var headers = new Dictionary<string, string>()
        {
            {
                key, GetRandomString()
            }
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
        using var server = GlobalSetup.Pool.Borrow();
        var capturedParams = new Dictionary<string, string>();
        server.Instance.AddJsonDocumentHandler(
            (processor, _) =>
            {
                processor.UrlParameters.ForEach(
                    kvp => capturedParams.Add(kvp.Key, kvp.Value)
                );
                return new[]
                {
                    1,
                    2,
                    3
                };
            }
        );
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
        using var server = GlobalSetup.Pool.Borrow();
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
            _server.AddDocumentHandler(
                (processor, _) =>
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
            _server.AddDocumentHandler(
                (processor, _) =>
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

    private static HttpServer Create(int? port = null)
    {
        return CreateWithPort(port);
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
            var fileInfo = new FileInfo(outFile);
            existingSize = fileInfo.Length;
            if (fullDownloadSize == existingSize)
            {
                Console.WriteLine("Already fully downloaded");
                return outFile;
            }

            Console.WriteLine("Resuming download from byte: {0}", fileInfo.Length);
            req.AddRange(fileInfo.Length);
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

    private void DownloadFile(
        WebResponse response,
        string outFile,
        long expectedSize,
        long totalSize
    )
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
            var readBuf = reader.ReadBytes((int)toRead);
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

    private static byte[] DownloadResultFrom(
        HttpServer server,
        string path,
        Dictionary<string, string> addHeaders = null
    )
    {
        return DownloadResultFrom(server, path, addHeaders, out var _);
    }

    private static byte[] DownloadResultFrom(
        IPoolItem<HttpServer> server,
        HttpMethods method,
        string path,
        Dictionary<string, string> addHeaders,
        out string contentType
    )
    {
        return DownloadResultFrom(
            server.Instance,
            method,
            path,
            addHeaders,
            out contentType,
            out _
        );
    }

    private static byte[] DownloadResultFrom(
        HttpServer server,
        HttpMethods method,
        string path,
        Dictionary<string, string> addHeaders,
        out string contentType,
        out Version versionInResponse
    )
    {
        var message = new HttpRequestMessage()
        {
            Method = HttpMethodLookup[method],
            RequestUri = new Uri(
                server.GetFullUrlFor(path)
            )
        };
        foreach (var kvp in addHeaders ?? new Dictionary<string, string>())
        {
            message.Headers.Add(kvp.Key, kvp.Value);
        }

        var response = HttpClient.Send(message);
        versionInResponse = response.Version;
        contentType = response.Content.Headers.ContentType?.MediaType;
        using var s = response.Content.ReadAsStream();
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new HttpException(
                response
            );
        }

        var memStream = new MemoryStream();
        s.CopyTo(memStream);
        return memStream.ToArray();
    }

    public class HttpException : Exception
    {
        public HttpStatusCode StatusCode => Response.StatusCode;
        public HttpResponseMessage Response { get; set; }

        public HttpException(
            HttpResponseMessage response
        ) : base($"http error: {response.StatusCode}")
        {
            Response = response;
        }
    }

    private static readonly Dictionary<HttpMethods, HttpMethod> HttpMethodLookup = new()
    {
        [HttpMethods.Delete] = HttpMethod.Delete,
        [HttpMethods.Get] = HttpMethod.Get,
        [HttpMethods.Head] = HttpMethod.Head,
        [HttpMethods.Options] = HttpMethod.Options,
        [HttpMethods.Patch] = HttpMethod.Patch,
        [HttpMethods.Post] = HttpMethod.Post,
        [HttpMethods.Put] = HttpMethod.Put
    };

    private static HttpClient HttpClient
        => _httpClient ??= new HttpClient();

    private static HttpClient _httpClient;


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
        out string contentType
    )
    {
        return DownloadResultFrom(server, HttpMethods.Get, path, addHeaders, out contentType, out _);
    }

    public class SimpleData
    {
        public string SomeProperty { get; set; }
    }
}