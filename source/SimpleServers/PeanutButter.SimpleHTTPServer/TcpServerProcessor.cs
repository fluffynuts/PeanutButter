using System.Net.Sockets;

namespace PeanutButter.SimpleHTTPServer
{
    /// <summary>
    /// Abstract TCP processor
    /// </summary>
    public abstract class TcpServerProcessor
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once MemberCanBeProtected.Global
        /// <summary>
        /// Provides access to the TcpClient
        /// </summary>
        public TcpClient TcpClient { get; protected set; }

        /// <inheritdoc />
        protected TcpServerProcessor(TcpClient client)
        {
            TcpClient = client;
        }
    }
}