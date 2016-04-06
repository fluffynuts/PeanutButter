using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
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

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = new RequestLogItem(path, code, message);

            //---------------Test Result -----------------------
            Assert.AreEqual(path, sut.Path);
            Assert.AreEqual(code, sut.StatusCode);
            Assert.AreEqual(message, sut.Message);
        }

        [Test]
        public void Construct_GivenNullMessage_ShouldSetMessageFromStatusCodeString()
        {
            //---------------Set up test pack-------------------
            var path = GetRandomString();
            var code = GetRandom<HttpStatusCode>();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = new RequestLogItem(path, code, null);

            //---------------Test Result -----------------------
            Assert.AreEqual(path, sut.Path);
            Assert.AreEqual(code, sut.StatusCode);
            Assert.AreEqual(code.ToString(), sut.Message);
        }

    }
}
