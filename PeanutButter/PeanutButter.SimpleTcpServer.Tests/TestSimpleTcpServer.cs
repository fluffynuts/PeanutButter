using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PeanutButter.SimpleTcpServer.Tests
{
    [TestFixture]
    public class TestSimpleTcpServer
    {
        public class MyTcpServer: TcpServer
        {
            public int LastRandomPort { get; private set;}

            public MyTcpServer(int port): base(port)
            {
            }

            public MyTcpServer()
            {
                LastRandomPort = 2000;
            }

            public int _FindOpenRandomPort()
            {
                return FindOpenRandomPort();
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
    }
}
