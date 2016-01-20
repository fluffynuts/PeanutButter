using System;
using System.IO;
using System.Net.Sockets;

namespace PeanutButter.SimpleHTTPServer
{
    public class TcpIoWrapper : IDisposable
    {
        public Stream RawStream { get { return GetRawStream(); } }
        public StreamWriter StreamWriter { get { return GetStreamWriter(); } }

        private StreamWriter _outputStreamWriter;
        private TcpClient _client;
        private BufferedStream _rawStream;

        public TcpIoWrapper(TcpClient client)
        {
            _client = client;
        }

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
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
        }

        private void DisposeStreamWriter()
        {
            if (_outputStreamWriter != null)
            {
                _outputStreamWriter.Flush();
                _outputStreamWriter.Dispose();
                _outputStreamWriter = null;
            }
        }

        private void DisposeRawStream()
        {
            if (_rawStream != null)
            {
                try
                {
                    _rawStream.Flush();
                    _rawStream.Dispose();
                }
                catch { }
                _rawStream = null;
            }
        }

        private StreamWriter GetStreamWriter()
        {
            lock (this)
            {
                if (_client == null)
                    return null;
                if (_outputStreamWriter == null)
                    _outputStreamWriter = new StreamWriter(RawStream);
                return _outputStreamWriter;
            }
        }

        private Stream GetRawStream()
        {
            lock (this)
            {
                if (_client == null) return null;
                if (_rawStream == null)
                    _rawStream = new BufferedStream(_client.GetStream());
                return _rawStream;
            }
        }
    }
}