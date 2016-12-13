using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.SimpleHTTPServer.Testability.Tests
{
    [TestFixture]
    public class TestHttpServerExtensions
    {
        [Test]
        public void GetRequestLogsMatching_OperatingOnHttpServer_WhenHaveNotEnabledLogging_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            using (var server = Create(false))
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.Throws<Exception>(() => server.GetRequestLogsMatching(l => true));

                //---------------Test Result -----------------------
            }
        }

        [Test]
        public void GetRequestLogsMatching_OperatingOnHttpServer_GivenMatchingFunc_WhenHaveEnabledLogging_WhenNoLogs_ShouldReturnEmptyCollection()
        {
            //---------------Set up test pack-------------------
            using (var server = Create())
            {
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = server.GetRequestLogsMatching(l => true);

                //---------------Test Result -----------------------
                CollectionAssert.IsEmpty(result);
            }
        }

        [Test]
        public void GetRequestLogsMatching_OperatingOnHttpServer_ShouldReturnMatchingLogs()
        {
            //---------------Set up test pack-------------------
            using (var server = Create())
            {
                var requests = GetRandomCollection<string>(3,3);
                var skip = requests.Skip(1).First();
                RequestSuppressed(server, requests.First());
                RequestSuppressed(server, requests.Skip(1).First());
                RequestSuppressed(server, requests.Skip(2).First());

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var result = server.GetRequestLogsMatching(l => l.Path != "/" + skip);

                //---------------Test Result -----------------------
                Assert.IsNotNull(result);
                CollectionAssert.IsNotEmpty(result);
                Assert.AreEqual(2, result.Count());
                Assert.IsTrue(result.Any(r => r.Path == "/" + requests.First()));
                Assert.IsTrue(result.Any(r => r.Path == "/" + requests.Last()));
            }
        }

        [Test]
        public void ShouldHaveReceivedRequestFor_GivenPath_WhenHaveNotReceivedRequest_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            using (var server = Create())
            {

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.Throws<AssertionException>(() => server.ShouldHaveReceivedRequestFor(GetRandomString()));

                //---------------Test Result -----------------------
            }
        }

        [TestCase("GET")]
        [TestCase("POST")]
        public void ShouldHaveReceivedRequestFor_GivenPath_WhenHaveNotReceivedMatchingRequest_ShouldThrow(string method)
        {
            //---------------Set up test pack-------------------
            using (var server = Create())
            {
                var path = "/" + GetRandomString();
                var expected = $"{method} {path}";
                RequestSuppressed(server, path, method);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var ex = Assert.Throws<AssertionException>(() => server.ShouldHaveReceivedRequestFor(GetRandomString()));

                //---------------Test Result -----------------------
                StringAssert.Contains(expected, ex.Message, ex.Message);
            }
        }

        [TestCase("GET")]
        [TestCase("POST")]
        public void ShouldHaveReceivedRequestFor_GivenPath_ShouldNotThrowWhenHaveReceivedRequestWithMethod_(string method)
        {
            //---------------Set up test pack-------------------
            using (var server = Create())
            {
                var path = "/" + GetRandomString();
                RequestSuppressed(server, path, method);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.DoesNotThrow(() => server.ShouldHaveReceivedRequestFor(path));

                //---------------Test Result -----------------------
            }
        }

        [TestCase("GET", HttpMethods.Get)]
        [TestCase("POST", HttpMethods.Post)]
        public void ShouldHaveReceivedRequestFor_GivenPathAndMethod_ShouldNotThrowWhenHaveReceivedMatchingRequest_(string method, HttpMethods httpMethod)
        {
            //---------------Set up test pack-------------------
            using (var server = Create())
            {
                var path = "/" + GetRandomString();
                RequestSuppressed(server, path, method);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.DoesNotThrow(() => server.ShouldHaveReceivedRequestFor(path, httpMethod));

                //---------------Test Result -----------------------
            }
        }

        [TestCase("GET", HttpMethods.Get)]
        [TestCase("POST", HttpMethods.Post)]
        public void ShouldHaveReceivedRequestFor_GivenPathAndMethod_ShouldNotThrowWhenHaveReceivedMatchingRequestWithParameters_(string method, HttpMethods httpMethod)
        {
            //---------------Set up test pack-------------------
            using (var server = Create())
            {
                var path = "/" + GetRandomString() + "?" + GetRandomString() + "=" + GetRandomString();
                RequestSuppressed(server, path, method);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.DoesNotThrow(() => server.ShouldHaveReceivedRequestFor(path, httpMethod));

                //---------------Test Result -----------------------
            }
        }

        [TestCase("GET", HttpMethods.Post)]
        [TestCase("POST", HttpMethods.Get)]
        public void ShouldHaveReceivedRequestFor_GivenPathAndMethod_ShouldThrowWhenHaveNotReceivedMatchingRequest_(string method, HttpMethods httpMethod)
        {
            //---------------Set up test pack-------------------
            using (var server = Create())
            {
                var path = "/" + GetRandomString();
                RequestSuppressed(server, path, method);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.Throws<AssertionException>(() => server.ShouldHaveReceivedRequestFor(path, httpMethod));

                //---------------Test Result -----------------------
            }
        }

        [TestCase("GET", HttpMethods.Post)]
        [TestCase("POST", HttpMethods.Get)]
        public void ShouldNotHaveReceivedRequestFor_GivenPathAndMethod_ShouldNotThrowWhenHaveNotReceivedMatchingRequest_(string method, HttpMethods httpMethod)
        {
            //---------------Set up test pack-------------------
            using (var server = Create())
            {
                var path = "/" + GetRandomString();
                RequestSuppressed(server, path, method);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.DoesNotThrow(() => server.ShouldNotHaveReceivedRequestFor(path, httpMethod));

                //---------------Test Result -----------------------
            }
        }

        [TestCase("GET", HttpMethods.Get)]
        [TestCase("POST", HttpMethods.Post)]
        public void ShouldNotHaveReceivedRequestFor_GivenPathAndMethod_ShouldThrowWhenHaveReceivedMatchingRequestWithParameters_(string method, HttpMethods httpMethod)
        {
            //---------------Set up test pack-------------------
            using (var server = Create())
            {
                var path = "/" + GetRandomString() + "?" + GetRandomString() + "=" + GetRandomString();
                RequestSuppressed(server, path, method);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.Throws<AssertionException>(() => server.ShouldNotHaveReceivedRequestFor(path, httpMethod));

                //---------------Test Result -----------------------
            }
        }


        [Test]
        public void ShouldHaveHeaderFor_GivenHeaderAndValue_WhenHaveMatch_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            using (var server = Create())
            {
                var path = "/" + GetRandomString();
                var expectedHeader1 = "X-" + GetRandomString();
                var expectedValue1 = GetRandomString();
                var headers = new Dictionary<string, string>()
                {
                    { expectedHeader1, expectedValue1 }
                };
                RequestSuppressed(server, path, "GET", headers);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.DoesNotThrow(() => server.ShouldHaveHadHeaderFor(path, HttpMethods.Any, expectedHeader1, expectedValue1));

                //---------------Test Result -----------------------
            }
        }

        [Test]
        public void ShouldHaveHeaderFor_GivenHeaderAndValue_WhenNoHeaderMatch_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            using (var server = Create())
            {
                var path = "/" + GetRandomString();
                var expectedHeader1 = "X-" + GetRandomString();
                var expectedValue1 = GetRandomString();
                var headers = new Dictionary<string, string>()
                {
                    { "X-" + GetAnother(expectedHeader1), expectedValue1 }
                };
                RequestSuppressed(server, path, "GET", headers);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.Throws<AssertionException>(() => server.ShouldHaveHadHeaderFor(path, HttpMethods.Any, expectedHeader1, expectedValue1));

                //---------------Test Result -----------------------
            }
        }

        [Test]
        public void ShouldHaveHeaderFor_GivenHeaderAndValue_WhenNoValueMatch_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            using (var server = Create())
            {
                var path = "/" + GetRandomString();
                var expectedHeader1 = "X-" + GetRandomString();
                var expectedValue1 = GetRandomString();
                var headers = new Dictionary<string, string>()
                {
                    { expectedHeader1, GetAnother(expectedValue1) }
                };
                RequestSuppressed(server, path, "GET", headers);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.Throws<AssertionException>(() => server.ShouldHaveHadHeaderFor(path, HttpMethods.Any, expectedHeader1, expectedValue1));

                //---------------Test Result -----------------------
            }
        }

        [Test]
        public void ShouldNotHaveHeaderFor_GivenHeaderAndValue_WhenDontHaveThatHeaderAndValue_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            using (var server = Create())
            {
                var path = "/" + GetRandomString();
                var expectedHeader1 = "X-" + GetRandomString();
                var expectedValue1 = GetRandomString();
                var headers = new Dictionary<string, string>()
                {
                    { expectedHeader1 + GetRandomString(), GetAnother(expectedValue1) + GetRandomString() }
                };
                RequestSuppressed(server, path, "GET", headers);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.DoesNotThrow(() => server.ShouldNotHaveHadHeaderFor(path, HttpMethods.Any, expectedHeader1, expectedValue1));

                //---------------Test Result -----------------------
            }
        }

        [Test]
        public void ShouldNotHaveHeaderFor_GivenHeaderAndValue_WhenDontHaveThatHeader_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            using (var server = Create())
            {
                var path = "/" + GetRandomString();
                var expectedHeader1 = "X-" + GetRandomString();
                var expectedValue1 = GetRandomString();
                var headers = new Dictionary<string, string>()
                {
                    { expectedHeader1 + GetRandomString(), GetAnother(expectedValue1) }
                };
                RequestSuppressed(server, path, "GET", headers);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.DoesNotThrow(() => server.ShouldNotHaveHadHeaderFor(path, HttpMethods.Any, expectedHeader1, expectedValue1));

                //---------------Test Result -----------------------
            }
        }

        [Test]
        public void ShouldNotHaveHeaderFor_GivenHeaderAndValue_WhenDontHaveThatHeaderValue_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            using (var server = Create())
            {
                var path = "/" + GetRandomString();
                var expectedHeader1 = "X-" + GetRandomString();
                var expectedValue1 = GetRandomString();
                var headers = new Dictionary<string, string>()
                {
                    { expectedHeader1, GetAnother(expectedValue1) + GetRandomString() }
                };
                RequestSuppressed(server, path, "GET", headers);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.DoesNotThrow(() => server.ShouldNotHaveHadHeaderFor(path, HttpMethods.Any, expectedHeader1, expectedValue1));

                //---------------Test Result -----------------------
            }
        }


        [Test]
        public void ShouldNotHaveHeaderFor_GivenHeaderAndValue_WhenHaveHeaderAndValue_ShouldThrow()
        {
            //---------------Set up test pack-------------------
            using (var server = Create())
            {
                var path = "/" + GetRandomString();
                var expectedHeader1 = "X-" + GetRandomString();
                var expectedValue1 = GetRandomString();
                var headers = new Dictionary<string, string>()
                {
                    { expectedHeader1, expectedValue1 }
                };
                RequestSuppressed(server, path, "GET", headers);
                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                Assert.Throws<AssertionException>(() => server.ShouldNotHaveHadHeaderFor(path, HttpMethods.Any, expectedHeader1, expectedValue1));

                //---------------Test Result -----------------------
            }
        }



        private void RequestSuppressed(HttpServer server, string path, string method = "GET", Dictionary<string, string> headers = null)
        {
            try
            {
                PerformRequest(server.GetFullUrlFor(path), method, headers);
            }
            catch
            {
                /* intentionally left blank */
            }
        }

        private void PerformRequest(string url, string method, Dictionary<string, string> headers )
        {
            var request = WebRequest.CreateHttp(url);
            request.Method = method;
            headers?.ForEach(h => request.Headers[h.Key] = h.Value);
            using (var stream = request.GetResponse())
            {
                /* intentially left blank */
            }
        }


        private HttpServer Create(bool enableRequestLogging = true)
        {
            var httpServer = new HttpServer();
            if (enableRequestLogging)
                httpServer.EnableRequestLogging();
            return httpServer;
        }
    }
}
