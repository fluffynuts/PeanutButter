using System;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using PeanutButter.WindowsServiceManagement.Exceptions;

namespace PeanutButter.WindowsServiceManagement.Tests.Exceptions
{
    [TestFixture]
    public class TestWindowsServiceUtilException
    {
        [Test]
        public void Type_ShouldInheritFrom_Exception()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (WindowsServiceUtilException);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldInheritFrom<Exception>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_ShouldSetMessageFromProvidedServiceName()
        {
            //---------------Set up test pack-------------------
            var expected = RandomValueGen.GetRandomString();
            var sut = new WindowsServiceUtilException(expected);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.Message;

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }
    }
}
