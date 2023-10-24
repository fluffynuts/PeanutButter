using System;
using System.Collections.Generic;
using System.Linq;
using PeanutButter.Utils;

// ReSharper disable PossibleMultipleEnumeration

namespace PeanutButter.SimpleHTTPServer.Testability
{
    public static class HttpServerExtensions
    {
        static readonly Dictionary<IHttpServer, List<RequestLogItem>> ServerRequests = new();

        public static void EnableRequestLogging(this IHttpServer server)
        {
            var logs = new List<RequestLogItem>();
            ServerRequests[server] = logs;
            server.RequestLogAction = logs.Add;
        }

        public static void ShouldHaveReceivedRequestFor(
            this IHttpServer server,
            string path,
            HttpMethods method = HttpMethods.Any
        )
        {
            if (server.GetRequestLogsMatching(path, method).Any())
            {
                return;
            }

            throw new HttpServerAssertionException(
                server.RequestErrorFor($"No request matching path {path} and method {method}.")
            );
        }

        public static void ShouldNotHaveReceivedRequestFor(
            this IHttpServer server,
            string path,
            HttpMethods method = HttpMethods.Any
        )
        {
            if (server.GetRequestLogsMatching(path, method).Any())
            {
                throw new HttpServerAssertionException(
                    server.RequestErrorFor($"Should have no requests matching path {path} and method {method}.")
                );
            }
        }

        public static void ShouldHaveHadHeaderFor(
            this IHttpServer server,
            string path,
            HttpMethods method,
            string header,
            string expectedValue
        )
        {
            var request = GetSingleRequestFor(server, path, method);
            if (!request.Headers.ContainsKey(header))
            {
                throw new HttpServerAssertionException(
                    $"Missing required header {header}"
                );
            }

            if (expectedValue != request.Headers[header])
            {
                throw new HttpServerAssertionException(
                    $"Header {header} had unexpected value {request.Headers[header]}. Should have been {expectedValue}"
                );
            }
        }

        public static void ShouldNotHaveHadHeaderFor(
            this IHttpServer server,
            string path,
            HttpMethods method,
            string header,
            string expectedValue
        )
        {
            var request = GetSingleRequestFor(server, path, method);
            if (request.Headers.ContainsKey(header) && (expectedValue == request.Headers[header]))
            {
                throw new HttpServerAssertionException(
                    $"Header {header} had should not have had value {request.Headers[header]}."
                );
            }
        }


        public static void Reset(this IHttpServer server)
        {
            server.Reset();
            if (ServerRequests.TryGetValue(server, out var logs))
            {
                logs.Clear();
            }
        }

        private static RequestLogItem GetSingleRequestFor(
            IHttpServer server,
            string path,
            HttpMethods method
        )
        {
            var requests = server.GetRequestLogsMatching(path, method);
            if (requests.Count() > 1)
            {
                throw new HttpServerAssertionException(
                    string.Join(
                        Environment.NewLine,
                        "Expected to get one request matching",
                        $"{method.ToString().ToUpper()} {path}",
                        "But got none. ",
                        GetAllRequestsAsLinesFor(server)
                    )
                );
            }

            if (requests.Count() != 1)
            {
                throw new HttpServerAssertionException(
                    $"Should have received only one request for {path} (received: {requests.Count()}"
                );
            }

            return requests.FirstOrDefault();
        }

        private static string GetAllRequestsAsLinesFor(
            IHttpServer server
        )
        {
            var allRequests = server.GetRequestLogsMatching(_ => true);
            if (allRequests.IsEmpty())
            {
                return null;
            }

            return string.Join(
                Environment.NewLine,
                "Got the following requests: ",
                string.Join(Environment.NewLine, allRequests.Select(AsReadableLine))
            );
        }

        private static object AsReadableLine(RequestLogItem arg)
        {
            return $"{arg.Method.ToUpper()} {arg.Path}";
        }

        public static IEnumerable<RequestLogItem> GetRequestLogsMatching(
            this IHttpServer server,
            Func<RequestLogItem, bool> matcher
        )
        {
            List<RequestLogItem> logs;
            if (!ServerRequests.TryGetValue(server, out logs))
            {
                throw new Exception("Logging hasn't been enabled for this server");
            }

            return logs.Where(matcher);
        }

        public static IEnumerable<RequestLogItem> GetRequestLogsMatching(
            this IHttpServer server,
            string path,
            HttpMethods method
        )
        {
            return server.GetRequestLogsMatching(
                l =>
                    l.Path == path && method.Matches(l.Method)
            );
        }

        private static string RequestErrorFor(
            this IHttpServer server,
            string mainError
        )
        {
            return $"{mainError}. Got requests: {PrettyPrintRequestsFor(server)}";
        }

        private static string PrettyPrintRequestsFor(
            IHttpServer server
        )
        {
            var logs = ServerRequests[server];
            return logs.Select(PrettyPrintRequestLog).JoinWith(Environment.NewLine);
        }

        private static object PrettyPrintRequestLog(RequestLogItem arg)
        {
            return $"{arg.Method} {arg.Path}";
        }
    }
}