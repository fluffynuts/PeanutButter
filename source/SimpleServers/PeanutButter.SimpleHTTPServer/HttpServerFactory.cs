using System;
using PeanutButter.Utils;

namespace PeanutButter.SimpleHTTPServer
{
    public interface IHttpServerFactory
    {
        /// <summary>
        /// Provides a new or previously-released http server
        /// </summary>
        /// <returns></returns>
        Lease<HttpServer> BorrowServer();
        /// <summary>
        /// Provides a new or previously-released http server and
        /// configures the log action
        /// </summary>
        /// <returns></returns>
        Lease<HttpServer> BorrowServer(Action<string> logAction);
    }

    /// <summary>
    /// Provides a mechanism for re-usable http-servers, eg in testing
    /// since it takes a little time to spin up an http server
    /// </summary>
    public class HttpServerFactory : LeasingFactory<HttpServer>, IHttpServerFactory
    {
        /// <summary>
        /// instantiates a new http-server factory
        /// </summary>
        public HttpServerFactory() : base(() => new HttpServer())
        {
        }

        public Lease<HttpServer> BorrowServer()
        {
            return BorrowServer(null);
        }

        public Lease<HttpServer> BorrowServer(
            Action<string> logAction
        )
        {
            var result = Borrow();
            result.Item.LogAction = logAction;
            result.Item.Reset();
            return result;
        }
    }
    
}