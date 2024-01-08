using System;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using PeanutButter.Utils;

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.SimpleTcpServer.Tests
{
    [TestFixture]
    public class TestSimpleTcpServer
    {
        public class MyTcpServer : TcpServer
        {
            public int LastRandomPort { get; private set; }

            public MyTcpServer(int port) : base(port)
            {
            }

            public MyTcpServer()
            {
                LastRandomPort = 2000;
            }

            public int _FindOpenRandomPort()
            {
                return PortFinder.FindOpenPort(
                    IPAddress.Loopback,
                    1024,
                    32768,
                    (_, _, last) => last == 0
                        ? LastRandomPort
                        : NextRandomPort()
                );
            }

            protected override int NextRandomPort()
            {
                return LastRandomPort++;
            }

            protected override void Init()
            {
            }

            protected override IProcessor CreateProcessorFor(TcpClient client)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void FindOpenRandomPort_WhenCannotBindOnCurrentRandomPort_ShouldChooseAnotherPortAndBind()
        {
            //---------------Set up test pack-------------------
            using var blocker = new MyTcpServer(2000);
            blocker.Start();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var another = new MyTcpServer(2000);
            var result = another._FindOpenRandomPort();
            Expect(result)
                .To.Equal(2001);

            //---------------Test Result -----------------------
        }

        public class MyServer : TcpServer
        {
            protected override void Init()
            {
            }

            protected override IProcessor CreateProcessorFor(TcpClient client)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void DisposeShouldBeOverrideable()
        {
            // Arrange
            var sut = new MyServer() as TcpServer;
            // Pre-assert
            // Act
            sut.Dispose();
            // Assert
            var upcast = sut as MyServer;
            Expect(upcast.Disposed).To.Be.True();
        }
    }
}