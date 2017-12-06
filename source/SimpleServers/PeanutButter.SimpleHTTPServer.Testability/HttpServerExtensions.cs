using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PeanutButter.Utils;
// ReSharper disable PossibleMultipleEnumeration

namespace PeanutButter.SimpleHTTPServer.Testability
{
    public static class HttpServerExtensions
    {
        static Dictionary<HttpServer, List<RequestLogItem>> _serverRequests = new Dictionary<HttpServer, List<RequestLogItem>>();
        public static void EnableRequestLogging(this HttpServer server)
        {
            var logs = new List<RequestLogItem>();
            _serverRequests[server] = logs;
            server.RequestLogAction = logs.Add;
        }

        public static void ShouldHaveReceivedRequestFor(this HttpServer server, string path, HttpMethods method = HttpMethods.Any)
        {
            Assert.IsTrue(server.GetRequestLogsMatching(path, method).Any(),
                            server.RequestErrorFor($"No request matching path {path} and method {method}."));
        }

        public static void ShouldNotHaveReceivedRequestFor(this HttpServer server, string path, HttpMethods method = HttpMethods.Any)
        {
            Assert.IsFalse(server.GetRequestLogsMatching(path, method).Any(),
                            server.RequestErrorFor($"Should have no requests matching path {path} and method {method}."));
        }

        public static void ShouldHaveHadHeaderFor(this HttpServer server, string path, HttpMethods method, string header, string expectedValue)
        {
            var request = GetSingleRequestFor(server, path, method);
            if (!request.Headers.ContainsKey(header))
                Assert.Fail($"Missing required header {header}");
            if (expectedValue != request.Headers[header])
                Assert.Fail($"Header {header} had unexpected value {request.Headers[header]}. Should have been {expectedValue}");
        }

        public static void ShouldNotHaveHadHeaderFor(this HttpServer server, string path, HttpMethods method, string header, string expectedValue)
        {
            var request = GetSingleRequestFor(server, path, method);
            if (request.Headers.ContainsKey(header) && (expectedValue == request.Headers[header]))
                Assert.Fail($"Header {header} had should not have had value {request.Headers[header]}.");
        }

        private static RequestLogItem GetSingleRequestFor(HttpServer server, string path, HttpMethods method)
        {
            var requests = server.GetRequestLogsMatching(path, method);
            if (requests.Count() > 1)
            {
                Assert.Fail(string.Join(Environment.NewLine,
                    "Expected to get one request matching",
                    method.ToString().ToUpper() + " " + path,
                    "But got none. ",
                    GetAllRequestsAsLinesFor(server)));
            }
            Assert.AreEqual(1, requests.Count(), $"Should have received only one request for {path}");
            var request = requests.FirstOrDefault();
            Assert.IsNotNull(request, $"Got no requests matching path {path}");
            return request;
        }

        private static string GetAllRequestsAsLinesFor(HttpServer server)
        {
            var allRequests = server.GetRequestLogsMatching(o => true);
            if (allRequests.IsEmpty()) return null;
            return string.Join(Environment.NewLine,
                "Got the following requests: ",
                string.Join(Environment.NewLine, allRequests.Select(AsReadableLine))
            );
        }

        private static object AsReadableLine(RequestLogItem arg)
        {
            return $"{arg.Method.ToUpper()} {arg.Path}";
        }

        public static IEnumerable<RequestLogItem> GetRequestLogsMatching(this HttpServer server, Func<RequestLogItem, bool> matcher)
        {
            List<RequestLogItem> logs;
            if (!_serverRequests.TryGetValue(server, out logs))
                throw new Exception("Logging hasn't been enabled for this server");
            return logs.Where(matcher);
        }

        public static IEnumerable<RequestLogItem> GetRequestLogsMatching(this HttpServer server, string path, HttpMethods method)
        {
            return server.GetRequestLogsMatching(l => l.Path == path && HttpMethodsExtensions.Matches(method, l.Method));
        }

        private static string RequestErrorFor(this HttpServer server, string mainError)
        {
            return $"{mainError}. Got requests: {PrettyPrintRequestsFor(server)}";
        }

        private static string PrettyPrintRequestsFor(HttpServer server)
        {
            var logs = _serverRequests[server];
            return logs.Select(PrettyPrintRequestLog).JoinWith(Environment.NewLine);
        }

        private static object PrettyPrintRequestLog(RequestLogItem arg)
        {
            return $"{arg.Method} {arg.Path}";
        }
    }
}
