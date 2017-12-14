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
    /// Provides the abstract base HTTP Server
    /// </summary>
    public abstract class HttpServerBase : TcpServer
    {
        /// <summary>
        /// Log action used for requests
        /// </summary>
        public Action<RequestLogItem> RequestLogAction { get; set; } = null;

        /// <inheritdoc />
        protected HttpServerBase(int port) : base(port)
        {
        }

        /// <inheritdoc />
        protected HttpServerBase()
        {
        }

        /// <summary>
        /// Factory function to create the processor for a given TcpClient
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected override IProcessor CreateProcessorFor(TcpClient client)
        {
            return new HttpProcessor(client, this);
        }

        /// <summary>
        /// Handles a GET request
        /// </summary>
        /// <param name="p"></param>
        public abstract void HandleGETRequest(HttpProcessor p);

        /// <summary>
        /// Handles a POST request
        /// </summary>
        /// <param name="p"></param>
        /// <param name="inputData"></param>
        public abstract void HandlePOSTRequest(HttpProcessor p, Stream inputData);
    }
}