using System;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Win32ServiceControl.Exceptions;

namespace PeanutButter.WindowsServiceManagement.Tests.Exceptions
{
    [TestFixture]
    public class TestServiceNotInstalledException
    {
        [Test]
        public void Type_ShouldInheritFrom_Exception()
        {
            //---------------Set up test pack-------------------
            var sut = typeof (ServiceNotInstalledException);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldInheritFrom<Exception>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_ShouldSetMessageFromProvidedServiceName()
        {
            //---------------Set up test pack-------------------
            var serviceName = RandomValueGen.GetRandomString();
            var expected = serviceName + " is not installed";
            var sut = new ServiceNotInstalledException(serviceName);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = sut.Message;

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }


    }
}
