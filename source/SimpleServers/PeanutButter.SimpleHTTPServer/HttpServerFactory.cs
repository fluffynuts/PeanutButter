using PeanutButter.Utils;

namespace PeanutButter.SimpleHTTPServer
{
    /// <summary>
    /// Describes a factory for your http server usage:
    /// - Take() an IPoolItem&lt;IHttpServer&gt;
    /// - work with the server
    /// - return it to the pool by disposing of the pool item (use 'using' for safety)
    /// </summary>
    public interface IHttpServerFactory : IPool<IHttpServer>
    {
    }

    /// <inheritdoc cref="PeanutButter.SimpleHTTPServer.IHttpServerFactory" />
    public class HttpServerFactory : Pool<IHttpServer>, IHttpServerFactory
    {
        /// <summary>
        /// Constructs a new HttpServerFactory with no limit
        /// on the total number of servers that can be in play
        /// </summary>
        public HttpServerFactory() : this(int.MaxValue)
        {
        }

        /// <summary>
        /// Constructs a new HttpServerFactory with the provided
        /// limit on servers which can be in play
        /// </summary>
        /// <param name="maxItems"></param>
        public HttpServerFactory(int maxItems)
            : base(() => new HttpServer(), s => s.Reset(), maxItems)
        {
        }
    }
}