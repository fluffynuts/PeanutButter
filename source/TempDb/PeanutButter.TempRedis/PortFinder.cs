using System;
using System.Net.Sockets;
using PeanutButter.SimpleTcpServer;

namespace PeanutButter.TempRedis
{
    internal class PortFinder : TcpServer
    {
        protected override void Init()
        {
        }

        protected override IProcessor CreateProcessorFor(
            TcpClient client)
        {
            // Shouldn't need a processor
            throw new NotImplementedException();
        }
    }
}