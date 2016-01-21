using System;

namespace PeanutButter.SimpleTcpServer
{
    public class PortUnavailableException: Exception
    {
        public PortUnavailableException(int port)
            : base("Can't listen on specified port '" + port + "': probably already in use?")
        {
        }
    }
}