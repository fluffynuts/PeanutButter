using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace PeanutButter.TestUtils.Generic.Tests
{
    [TestFixture]
    public class TestDecimalExtensions
    {
        [Test]
        public void ShouldMatch_ShouldThrowWhenNumbersDontMatchAtGivenPosition()
        {
            //---------------Set up test pack-------------------
            var num1 = 1.23456M;
            var num2 = 1.23999M;

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            Assert.DoesNotThrow(() => num1.ShouldMatch(num2, 0));
            Assert.DoesNotThrow(() => num1.ShouldMatch(num2, 1));
            Assert.DoesNotThrow(() => num1.ShouldMatch(num2, 2));
            Assert.Throws<AssertionException>(() => num1.ShouldMatch(num2, 3));
            Assert.Throws<AssertionException>(() => num1.ShouldMatch(num2, 4));
            Assert.Throws<AssertionException>(() => num1.ShouldMatch(num2, 5));

            //---------------Test Result -----------------------
        }
    }
}
