using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.SimpleTcpServer;

namespace PeanutButter.SimpleSMTPServer.Tests
{
    [TestFixture]
    public class TestSMTPServer
    {
        [Test]
        public void Construct_GivenNoParameters_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            SMTPServer server = null;
            Assert.DoesNotThrow(() => server = new SMTPServer());

            //---------------Test Result -----------------------
            Assert.IsInstanceOf<TcpServer>(server);
            Assert.AreNotEqual(0, server.Port);
        }

        [Test]
        public void Construct_GivenPort_ShouldNotThrow()
        {
            //---------------Set up test pack-------------------
            var port = RandomValueGen.GetRandomInt(1000, 10000);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            SMTPServer server = null;
            Assert.DoesNotThrow(() => server = new SMTPServer(port));

            //---------------Test Result -----------------------
            Assert.IsInstanceOf<TcpServer>(server);
            Assert.AreEqual(port, server.Port);
        }

        [Test]
        [Ignore("WIP")]
        public void Start_IncomingClient_ShouldGetGreeting()
        {
            //---------------Set up test pack-------------------
            using (var server = Create())
            {
                //---------------Assert Precondition----------------
                server.Start();

                //---------------Execute Test ----------------------

                //---------------Test Result -----------------------
            }
        }

        private SMTPServer Create()
        {
            return new SMTPServer();
        }
    }

}
