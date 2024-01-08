using System;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;
using PeanutButter.WindowsServiceManagement.Exceptions;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static NExpect.Expectations;
using NExpect;

namespace PeanutButter.WindowsServiceManagement.Tests
{
    [TestFixture]
    public class TestServiceOperationException
    {
        [Test]
        public void Type_ShouldInheritFrom_Exception()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(ServiceOperationException);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldInheritFrom<Exception>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_ShouldSetExpectedMessage()
        {
            //---------------Set up test pack-------------------
            var serviceName = GetRandomString();
            var operation = GetRandomString();
            var info = GetRandomString();
            var expected = $"Unable to perform {operation} on service {serviceName}: {info}";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(serviceName, operation, info);

            //---------------Test Result -----------------------
            Expect(sut.Message)
                .To.Equal(expected);
        }

        [Test]
        public void Construct_GivenNullServiceName_ShouldSetExpectedMessage()
        {
            //---------------Set up test pack-------------------
            var operation = GetRandomString();
            var info = GetRandomString();
            var expected = $"Unable to perform {operation} on service (null): {info}";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(null, operation, info);

            //---------------Test Result -----------------------
            Expect(sut.Message)
                .To.Equal(expected);
        }
        [Test]
        public void Construct_GivenNullOperation_ShouldSetExpectedMessage()
        {
            //---------------Set up test pack-------------------
            var serviceName = GetRandomString();
            var info = GetRandomString();
            var expected = $"Unable to perform (null) on service {serviceName}: {info}";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(serviceName, null, info);

            //---------------Test Result -----------------------
            Expect(sut.Message)
                .To.Equal(expected);
        }

        [Test]
        public void Construct_GivenNullInfo_ShouldSetExpectedMessage()
        {
            //---------------Set up test pack-------------------
            var serviceName = GetRandomString();
            var operation = GetRandomString();
            var expected = $"Unable to perform {operation} on service {serviceName}: (null)";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = Create(serviceName, operation, null);

            //---------------Test Result -----------------------
            Expect(sut.Message)
                .To.Equal(expected);
        }

        private ServiceOperationException Create(string serviceName, string operation, string info)
        {
            return new ServiceOperationException(serviceName, operation, info);
        }
    }
}
