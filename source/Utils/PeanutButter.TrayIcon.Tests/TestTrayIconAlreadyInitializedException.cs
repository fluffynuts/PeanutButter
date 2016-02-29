using System;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.TrayIcon.Tests
{
    [TestFixture]
    public class TestTrayIconAlreadyInitializedException
    {
        [Test]
        public void Type_ShouldInheritFrom_Exception()
        {
            //---------------Set up test pack-------------------
            var sut = typeof(TrayIconAlreadyInitializedException);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            sut.ShouldInheritFrom<Exception>();

            //---------------Test Result -----------------------
        }

        [Test]
        public void Construct_ShouldSetExpectedMessage()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var sut = new TrayIconAlreadyInitializedException();

            //---------------Test Result -----------------------
            Assert.AreEqual("This instance of the TrayIcon has already been initialized", sut.Message);
        }
    }

}