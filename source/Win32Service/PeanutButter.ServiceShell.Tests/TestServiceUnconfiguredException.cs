using System;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.ServiceShell.Tests
{
    [TestFixture]
    public class TestServiceUnconfiguredException
    {
        [Test]
        public void Type_ShouldInheritFrom_Exception()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (ServiceUnconfiguredException);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldInheritFrom<Exception>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_ShouldSetMessageContainingProvidedPropertyName()
        {
            //---------------Set up test pack-------------------
            var property = RandomValueGen.GetRandomString();
            var expected = "This service is not completely configured. Please set the " + property + " property value.";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = new ServiceUnconfiguredException(property);

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, sut.Message);
        }


    }
}
