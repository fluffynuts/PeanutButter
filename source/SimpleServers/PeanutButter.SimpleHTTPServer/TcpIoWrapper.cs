using System;
using System.IO;
using System.Net.Sockets;

namespace PeanutButter.SimpleHTTPServer
{
    /// <summary>
    /// Wraps a TcpClient for easier IO
    /// </summary>
    public class TcpIoWrapper : IDisposable
    {
        /// <summary>
        /// Provides access to the raw stream
        /// </summary>
        public Stream RawStream => GetRawStream();

        /// <summary>
        /// Provides access to the stream writer
        /// </summary>
        public StreamWriter StreamWriter => GetStreamWriter();

        private StreamWriter _outputStreamWriter;
        private TcpClient _client;
        private BufferedStream _rawStream;
        private readonly object _lock = new object();

        /// <inheritdoc />
        public TcpIoWrapper(TcpClient client)
        {
            _client = client;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (this)
            {
                DisposeStreamWriter();
                DisposeRawStream();
                ShutdownClient();
            }
        }

        private void ShutdownClient()
        {
            _client?.Close();
            _client = null;
        }

        private void DisposeStreamWriter()
        {
            try
            {
                _outputStreamWriter?.Flush();
                _outputStreamWriter?.Dispose();
            }
            catch
            {
                /* intentionally left blank */
            }
            _outputStreamWriter = null;
        }

        private void DisposeRawStream()
        {
            try
            {
                _rawStream?.Flush();
                _rawStream?.Dispose();
            }
            catch
            {
                /* intentionally left blank */
            }
            _rawStream = null;
        }

        private StreamWriter GetStreamWriter()
        {
            lock (_lock)
            {
                if (_client == null)
                {
                    return null;
                }

                return _outputStreamWriter ??
                       (_outputStreamWriter = new StreamWriter(RawStream));
            }
        }

        private Stream GetRawStream()
        {
            lock (_lock)
            {
                if (_client == null)
                {
                    return null;
                }

                return _rawStream ??
                       (_rawStream = new BufferedStream(_client.GetStream()));
            }
        }
    }
}