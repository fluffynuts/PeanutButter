using System;

namespace PeanutButter.SimpleTcpServer
{
    /// <summary>
    /// Exception thrown when the requested port for the simple
    /// server is not available for binding.
    /// </summary>
    public class PortUnavailableException: Exception
    {
        /// <inheritdoc />
        public PortUnavailableException(int port)
            : base($"Can't listen on specified port '{port}': probably already in use?")
        {
        }
    }
}