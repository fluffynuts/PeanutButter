using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.SimpleHTTPServer.Tests
{
    [TestFixture]
    public class TestRequestLogItem
    {
        [Test]
        public void Construct_ShouldCopyParametersToProperties()
        {
            //---------------Set up test pack-------------------
            var path = GetRandomString();
            var code = GetRandom<HttpStatusCode>();
            var message = GetRandomString();
            var method = GetRandomString();
            var headers = new Dictionary<string, string>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = new RequestLogItem(path, code, method, message, headers);

            //---------------Test Result -----------------------
            Assert.AreEqual(path, sut.Path);
            Assert.AreEqual(code, sut.StatusCode);
            Assert.AreEqual(method, sut.Method);
            Assert.AreEqual(message, sut.Message);
        }

        [Test]
        public void Construct_GivenNullMessage_ShouldSetMessageFromStatusCodeString()
        {
            //---------------Set up test pack-------------------
            var path = GetRandomString();
            var code = GetRandom<HttpStatusCode>();
            var method = GetRandomString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = new RequestLogItem(path, code, method, null, null);

            //---------------Test Result -----------------------
            Assert.AreEqual(path, sut.Path);
            Assert.AreEqual(code, sut.StatusCode);
            Assert.AreEqual(method, sut.Method);
            Assert.AreEqual(code.ToString(), sut.Message);
            Assert.IsNotNull(sut.Headers);
        }

    }
}
