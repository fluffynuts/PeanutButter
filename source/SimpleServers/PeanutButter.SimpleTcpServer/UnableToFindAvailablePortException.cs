using System;

namespace PeanutButter.SimpleTcpServer
{
    public class UnableToFindAvailablePortException: Exception
    {
        public UnableToFindAvailablePortException(): base("Can't find a port to listen on ):")
        {
        }
    }
}