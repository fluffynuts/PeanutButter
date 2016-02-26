using System.Text;
using NUnit.Framework;
using PeanutButter.RandomGenerators;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestStringExtensions
    {
        [TestCase("Hello World", "^Hello", "Goodbye", "Goodbye World")]
        [TestCase("Hello World", "Wor.*", "Goodbye", "Hello Goodbye")]
        [TestCase("Hello World", "Hello$", "Goodbye", "Hello World")]
        public void RegexReplace_ShouldReplaceAccordingToRegexWithSuppliedValue(string input, string re, string replaceWith, string expected)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.RegexReplace(re, replaceWith);


            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [TestCase(null)]
        [TestCase("")]
        public void Or_ActingOnString_ShouldReturnOtherStringWhenPrimaryIs_(string value)
        {
            // think equivalent to Javascript:
            //  var left = null;
            //  var right = 'foo';
            //  var empty = '';
            //  var result = left || right; // result is 'foo'
            //  var other = empty || right; // result is 'foo'
            //---------------Set up test pack-------------------
            string src = null;
            var other = RandomValueGen.GetRandomString(1, 20);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = src.Or(other);

            //---------------Test Result -----------------------
            Assert.AreEqual(other, result);
        }

        [Test]
        public void Or_CanChainUntilFirstValidValue()
        {
            //---------------Set up test pack-------------------
            string start = null;
            var expected = RandomValueGen.GetRandomString(1, 10);
            var unexpected = RandomValueGen.GetRandomString(11, 20);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = start.Or("").Or(null).Or(expected).Or(unexpected);

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [TestCase("Yes", true)]
        [TestCase("Y", true)]
        [TestCase("yes", true)]
        [TestCase("y", true)]
        [TestCase("1", true)]
        [TestCase("True", true)]
        [TestCase("TRUE", true)]
        [TestCase("true", true)]
        public void AsBoolean_ShouldResolveToBooleanValue(string input, bool expected)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.AsBoolean();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AsBytes_WhenStringIsNull_ShouldReturnNull()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = ((string)null).AsBytes();

            //---------------Test Result -----------------------
            Assert.IsNull(result);
        }

        [Test]
        public void AsBytes_OperatingOnEmptyString_ShouldReturnEmptyByteArray()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = string.Empty.AsBytes();

            //---------------Test Result -----------------------
            Assert.IsNotNull(result);
            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public void AsBytes_OperatingOnNonEmptyString_ShouldReturnStringEncodedAsBytesFromUTF8()
        {
            //---------------Set up test pack-------------------
            var input = RandomValueGen.GetRandomString(50, 100);
            var expected = Encoding.UTF8.GetBytes(input);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.AsBytes();

            //---------------Test Result -----------------------
            CollectionAssert.AreEqual(expected, result);
        }

        [Test]
        public void IsInteger_WhenStringIsInteger_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var input = RandomValueGen.GetRandomInt().ToString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.IsInteger();

            //---------------Test Result -----------------------
            Assert.IsTrue(result);
        }

        [Test]
        public void IsInteger_WhenStringIsNotInteger_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var input = RandomValueGen.GetRandomAlphaString();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = input.IsInteger();

            //---------------Test Result -----------------------
            Assert.IsFalse(result);
        }




    }
}
