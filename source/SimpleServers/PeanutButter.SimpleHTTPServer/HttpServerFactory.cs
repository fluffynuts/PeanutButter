using System;
using System.Collections.Generic;
using System.Linq;
using PeanutButter.Utils;

namespace PeanutButter.SimpleHTTPServer
{
    /// <summary>
    /// Provides a mechanism for re-usable http-servers, eg in testing
    /// since it takes a little time to spin up an http server
    /// </summary>
    public class HttpServerFactory : IDisposable
    {
        private readonly List<HttpServer> _servers = new();

        /// <summary>
        /// Provides a new or previously-released http server
        /// </summary>
        /// <returns></returns>
        public Lease<HttpServer> BorrowServer()
        {
            return BorrowServer(null);
        }

        /// <summary>
        /// Provides a new or previously-released http server
        /// </summary>
        /// <returns></returns>
        public Lease<HttpServer> BorrowServer(
            Action<string> logAction
        )
        {
            lock (_servers)
            {
                if (_servers.TryShift(out var server))
                {
                    server.LogAction = logAction;
                    server.Reset();
                }
                else
                {
                    server = new HttpServer(logAction);
                }

                return new Lease<HttpServer>(
                    server,
                    () =>
                    {
                        lock (_servers)
                        {
                            _servers.Add(server);
                        }
                    }
                );
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            var servers = _servers.ToArray();
            _servers.Clear();
            if (servers.Length == 0)
            {
                return;
            }

            Run.InParallel(
                100,
                servers.Select(
                    s => new Action(
                        s.Dispose
                    )
                )
            );
        }
    }
}