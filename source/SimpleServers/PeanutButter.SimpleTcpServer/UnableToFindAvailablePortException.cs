using System;

namespace PeanutButter.SimpleTcpServer
{
    /// <summary>
    /// Thrown when no port could be found to bind to
    /// </summary>
    public class UnableToFindAvailablePortException: Exception
    {
        /// <inheritdoc />
        public UnableToFindAvailablePortException():
            base("Can't find a port to listen on ):")
        {
        }
    }
}