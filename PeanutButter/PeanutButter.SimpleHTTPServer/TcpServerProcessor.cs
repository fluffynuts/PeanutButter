using System.Net.Sockets;

namespace PeanutButter.SimpleHTTPServer
{
    public abstract class TcpServerProcessor
    {
        public TcpClient TcpClient { get; protected set; }

        protected TcpServerProcessor(TcpClient client)
        {
            this.TcpClient = client;
        }
    }
}