using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace PeanutButter.SimpleHTTPServer
{
    /// <summary>
    /// Provides the ReadLine extension for a TcpClient
    /// </summary>
    public static class TcpClientExtensions
    {
        /// <summary>
        /// The default read timeout when invoking ReadLine
        /// without any timeout value
        /// </summary>
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public static int DefaultReadTimeoutMs = 3000;
        
        /// <summary>
        /// Reads one line of data from the TcpClient
        /// with the default read timeout as specified
        /// byt TcpClientExtensions.DefaultReadTimeoutMs
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static string ReadLine(
            this TcpClient client
        )
        {
            return client.ReadLine(DefaultReadTimeoutMs);
        }

        /// <summary>
        /// Reads one line of data from the TcpClient
        /// </summary>
        /// <param name="client"></param>
        /// <param name="readTimeout"></param>
        /// <returns></returns>
        public static string ReadLine(
            this TcpClient client,
            int readTimeout
        )
        {
            var stream = client.GetStream();
            stream.ReadTimeout = 3000;
            if (!stream.CanRead)
            {
                throw new InvalidOperationException(
                    "Cannot read from stream"
                );
            }

            var data = new List<char>();
            var readFails = 0;
            while (true)
            {
                var thisChar = stream.TryReadByte();
                if (thisChar < 0)
                {
                    if (thisChar == -2 || ++readFails > 10)
                    {
                        // it's possible the failure is transitive?
                        break;
                    }

                    Thread.Sleep(0);
                    continue;
                }

                if (thisChar == '\n') break;
                if (thisChar == '\r') continue;
                data.Add(Convert.ToChar(thisChar));
            } // while (stream.DataAvailable);

            var result = string.Join(string.Empty, data);
            return result;
        }

        /// <summary>
        /// Attempt to read a byte from the NetworkStream. Any encountered
        /// exception makes this return -2.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static int TryReadByte(
            this NetworkStream stream
        )
        {
            try
            {
                return stream.ReadByte();
            }
            catch
            {
                return -2;
            }
        }
    }
}