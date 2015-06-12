using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using PeanutButter.SimpleTcpServer;

namespace PeanutButter.SimpleSMTPServer
{
    public class SMTPServer: TcpServer
    {
        public SMTPServer(int port) : base(port)
        {
        }
        public SMTPServer()
        {
        }

        protected override void Init()
        {
        }

        protected override IProcessor CreateProcessorFor(TcpClient client)
        {
            return new SMTPProcessor(client, this);
        }
    }

    public class SMTPProcessor : IProcessor
    {
        private SMTPServer _server;
        private TcpClient _client;

        public SMTPProcessor(TcpClient client, SMTPServer server)
        {
            this._client = client;
            this._server = server;
        }

        public void ProcessRequest()
        {
            throw new NotImplementedException();
        }
    }
}
