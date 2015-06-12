/*
 * Many thanks to BobJanova for a seed project for this library (see the original here: http://www.codeproject.com/Articles/25050/Embedded-NET-HTTP-Server)
 * Original license is CPOL. My preferred licensing is BSD, which differs only from CPOL in that CPOL explicitly grants you freedom
 * from prosecution for patent infringement (not that this code is patented or that I even believe in the concept). So, CPOL it is.
 * You can find the CPOL here:
 * http://www.codeproject.com/info/cpol10.aspx 
 */

using System.IO;
using System.Net.Sockets;
using PeanutButter.SimpleTcpServer;

namespace PeanutButter.SimpleHTTPServer
{
    public abstract class HttpServerBase : TcpServer
    {
        protected HttpServerBase(int port) : base(port)
        {
        }

        protected HttpServerBase()
        {
        }

        protected override IProcessor CreateProcessorFor(TcpClient client)
        {
            return new HttpProcessor(client, this);
        }

        public abstract void HandleGETRequest(HttpProcessor p);
        public abstract void HandlePOSTRequest(HttpProcessor p, Stream inputData);
    }
}