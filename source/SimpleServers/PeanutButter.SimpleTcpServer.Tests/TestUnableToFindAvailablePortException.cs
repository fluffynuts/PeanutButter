using System;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.SimpleTcpServer.Tests
{
    [TestFixture]
    public class TestUnableToFindAvailablePortException
    {
        [Test]
        public void Type_ShouldInheritFrom_Exception()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(UnableToFindAvailablePortException);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldInheritFrom<Exception>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_GivenPort_ShouldSetMessage()
        {
            //---------------Set up test pack-------------------
            var expected = "Can't find a port to listen on:";
            var innerMessage = GetRandomString(10);
            var inner = new InvalidOperationException(innerMessage);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = new UnableToFindAvailablePortException(inner);
            var result = sut.Message;

            //---------------Test Result -----------------------
            Expect(result)
                .To.Contain(expected);
            Expect(result)
                .To.Contain(innerMessage);
        }
    }
}