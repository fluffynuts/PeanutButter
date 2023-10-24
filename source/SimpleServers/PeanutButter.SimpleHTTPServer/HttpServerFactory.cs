using System;
using PeanutButter.Utils;

namespace PeanutButter.SimpleHTTPServer
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHttpServerFactory
    {
        /// <summary>
        /// Provides a new or previously-released http server and
        /// configures the log action
        /// </summary>
        /// <returns></returns>
        ILease<IHttpServer> Borrow(Action<string> logAction);
    }

    /// <summary>
    /// Provides a mechanism for re-usable http-servers, eg in testing
    /// since it takes a little time to spin up an http server
    /// </summary>
    public class HttpServerFactory : LeasingFactory<IHttpServer>, IHttpServerFactory
    {
        /// <summary>
        /// instantiates a new http-server factory
        /// </summary>
        public HttpServerFactory() : base(() => new HttpServer())
        {
        }

        /// <inheritdoc />
        public override ILease<IHttpServer> Borrow()
        {
            return Borrow(null);
        }

        /// <inheritdoc />
        public ILease<IHttpServer> Borrow(
            Action<string> logAction
        )
        {
            var result = base.Borrow();
            result.Item.LogAction = logAction;
            result.Item.Reset();
            return result;
        }
    }
    
}