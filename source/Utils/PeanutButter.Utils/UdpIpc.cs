using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PeanutButter.Utils.Experimental
{
    public class UdpIpc : IDisposable
    {
        public delegate void Receiver(byte[] message);

        public Receiver Receivers { get; set; }

        private int _startPort;
        private int _port;

        public UdpIpc(string identifier)
        {
            _startPort = GeneratePortFor(identifier);
            _port = _startPort;
            TryStart();
        }

        private void TryStart()
        {
            var testPort = _port;
            while (true)
            {
                try
                {
                    _client = new UdpClient(
                        new IPEndPoint(
                            IPAddress.Loopback,
                            testPort)
                    );
                    _asyncReceivedResult = _client.BeginReceive(Receive, new object());
                    _port = testPort;
                    return;
                }
                catch (SocketException)
                {
                    // try again
                    testPort++;
                }
            }
        }

        private static int GeneratePortFor(string identifier)
        {
            var possibleRange = 65535 - 1024;
            return 1024 + (Math.Abs(identifier.GetHashCode()) % possibleRange);
        }

        private UdpClient _client;

        IAsyncResult _asyncReceivedResult = null;

        private void Stop()
        {
            try
            {
                _client?.Close();
                _client = null;
                _asyncReceivedResult = null;
            }
            catch
            {
                /* don't care */
            }
        }

        private void Receive(IAsyncResult ar)
        {
            var ip = new IPEndPoint(IPAddress.Loopback, _port);
            var bytes = _client.EndReceive(ar, ref ip);
            Receivers(bytes);
            _asyncReceivedResult = _client.BeginReceive(Receive, new object());
        }

        public void Send(string message)
        {
            var client = new UdpClient();
            var ip = new IPEndPoint(IPAddress.Loopback, _port);
            var bytes = Encoding.ASCII.GetBytes(message);
            client.Send(bytes, bytes.Length, ip);
            client.Close();
        }

        public void Dispose()
        {
            Stop();
        }
    }

    public abstract class Message
    {
        public abstract string Type { get; set; }
        public abstract object PayloadObject { get; }

        public static Message<T> For<T>(T data)
        {
            return new Message<T>(data);
        }
    }

    public class Message<T>: Message
    {
        public override string Type { get; set; }

        public T Payload { get; set; }
        public override object PayloadObject => Payload;

        public Message()
        {
        }

        public Message(T payload)
        {
            Payload = payload;
            Type = typeof(T).Name;
        }
    }
}