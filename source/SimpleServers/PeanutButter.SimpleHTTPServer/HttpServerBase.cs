/*
 * Many thanks to BobJanova for a seed project for this library (see the original here: http://www.codeproject.com/Articles/25050/Embedded-NET-HTTP-Server)
 * Original license is CPOL. My preferred licensing is BSD, which differs only from CPOL in that CPOL explicitly grants you freedom
 * from prosecution for patent infringement (not that this code is patented or that I even believe in the concept). So, CPOL it is.
 * You can find the CPOL here:
 * http://www.codeproject.com/info/cpol10.aspx 
 */

using System;
using System.IO;
using System.Net.Sockets;
using PeanutButter.SimpleTcpServer;
// ReSharper disable InconsistentNaming

namespace PeanutButter.SimpleHTTPServer
{
    /// <summary>
    /// Version of the HTTP protocol.
    /// </summary>
    public enum HttpVersion
    {
        /// <summary>
        /// HTTP/1.0
        /// </summary>
        Version10 = 0,

        /// <summary>
        /// HTTP/1.1
        /// </summary>
        Version11,
    } 
    
    /// <summary>
    /// Describes the base functionality in a simple http-server
    /// </summary>
    public interface IHttpServerBase: IDisposable
    {
        /// <summary>
        /// Log action used for requests
        /// </summary>
        Action<RequestLogItem> RequestLogAction { get; set; }

        /// <summary>
        /// Provides the base url from which the server serves
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// Flag: when true, this server has been disposed
        /// </summary>
        bool Disposed { get; }

        /// <summary>
        /// Maximum time, in milliseconds, to wait on the
        /// listener task when shutting down
        /// </summary>
        int MaxShutDownTime { get; set; }

        /// <summary>
        /// Whether or not to log random port discovery processes
        /// </summary>
        bool LogRandomPortDiscovery { get; set; }

        /// <summary>
        /// Action to employ when logging (defaults to logging to the console)
        /// </summary>
        Action<string> LogAction { get; set; }

        /// <summary>
        /// Port which this server has bound to
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Flag exposing listening state
        /// </summary>
        bool IsListening { get; }
        
        /// <summary>
        /// HTTP version reported by the server in responses
        /// Note that this does not change behavior of the server, only the exact format of the response
        /// </summary>
        HttpVersion Version { get; set; }

        /// <summary>
        /// Handles a request that does not contain a body (as of the HTTP spec).
        /// </summary>
        /// <param name="p">The HTTP processor.</param>
        /// <param name="method">The HTTP method.</param>
        void HandleRequestWithoutBody(HttpProcessor p, string method);

        /// <summary>
        /// Handles a general request with a request body.
        /// </summary>
        /// <param name="p">The HTTP processor.</param>
        /// <param name="inputData">The stream to read the request body from.</param>
        /// <param name="method">The HTTP method.</param>
        void HandleRequestWithBody(HttpProcessor p, MemoryStream inputData, string method);

        /// <summary>
        /// Resolves the full url to the provided path on the current server
        /// </summary>
        /// <param name="relativeUrl"></param>
        /// <returns></returns>
        string GetFullUrlFor(string relativeUrl);

        /// <summary>
        /// Start the server
        /// </summary>
        void Start();

        /// <summary>
        /// Stop the server
        /// </summary>
        void Stop();
    }

    /// <summary>
    /// Provides the abstract base HTTP Server
    /// </summary>
    public abstract class HttpServerBase : TcpServer, IHttpServerBase
    {
        /// <summary>
        /// Log action used for requests
        /// </summary>
        public Action<RequestLogItem> RequestLogAction { get; set; } = null;
        
        /// <inheritdoc />
        public HttpVersion Version { get; set; }

        /// <inheritdoc />
        protected HttpServerBase(int port) : base(port)
        {
        }

        /// <inheritdoc />
        protected HttpServerBase()
        {
        }

        /// <inheritdoc />
        protected override IProcessor CreateProcessorFor(TcpClient client)
        {
            return new HttpProcessor(client, this);
        }

        /// <inheritdoc />
        public abstract void HandleRequestWithoutBody(HttpProcessor p, string method);

        /// <inheritdoc />
        public abstract void HandleRequestWithBody(HttpProcessor p, MemoryStream inputData, string method);

        /// <inheritdoc />
        public string GetFullUrlFor(string relativeUrl)
        {
            var joinWith = relativeUrl.StartsWith("/")
                ? string.Empty
                : "/";
            return string.Join(joinWith, BaseUrl, relativeUrl);
        }

        /// <inheritdoc />
        public string BaseUrl => $"http://localhost:{Port}";
    }
}