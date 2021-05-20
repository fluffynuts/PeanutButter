using System;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static NExpect.Expectations;
using NExpect;
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
                    (min, max, last) => last == 0
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
            using (var blocker = new MyTcpServer(2000))
            {
                blocker.Start();

                //---------------Assert Precondition----------------

                //---------------Execute Test ----------------------
                var another = new MyTcpServer();
                var result = another._FindOpenRandomPort();
                Assert.AreEqual(2001, result);

                //---------------Test Result -----------------------
            }
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

            public override void Dispose()
            {
                Disposed = true;
                base.Dispose();
            }

            public bool Disposed { get; private set; }
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