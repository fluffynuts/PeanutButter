using System;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.SimpleTcpServer.Tests
{
    [TestFixture]
    public class TestPortUnavailableException
    {
        [Test]
        public void Type_ShouldInheritFrom_Exception()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (PortUnavailableException);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldInheritFrom<Exception>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_GivenPort_ShouldSetMessage()
        {
            //---------------Set up test pack-------------------
            var port = RandomValueGen.GetRandomInt();
            var expected = $"Can't listen on specified port '{port}': probably already in use?";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = new PortUnavailableException(port);
            var result = sut.Message;

            //---------------Test Result -----------------------
            Expect(result)
                .To.Equal(expected);
        }
    }
}
