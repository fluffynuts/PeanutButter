using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PeanutButter.RandomGenerators;
using PeanutButter.Utils;

namespace PenautButter.Utils.Tests
{
    [TestFixture]
    public class TestDecimalDecorator
    {
        [TestCase(1.23, "1.23")]
        [TestCase(4.23, "4.23")]
        [TestCase(1.56, "1.56")]
        [TestCase(12.4, "12.4")]
        public void Construct_GivenDecimalValue_ToStringReturnsPeriodFormattedString(decimal input, string expected)
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var decorated = new DecimalDecorator(input);
            var result = decorated.ToString();
            //---------------Test Result -----------------------
            Assert.AreEqual(result, expected);
        }

        [Test]
        public void Construct_GivenDecimalValue_ToDecimalAlwaysReturnsThatValue()
        {
            //---------------Set up test pack-------------------
            var input = RandomValueGen.GetRandomDecimal();

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var decorated = new DecimalDecorator(input);
            var result = decorated.ToDecimal();
            //---------------Test Result -----------------------
            Assert.AreEqual(input, result);
        }

        [TestCase("1.23", 1.23)]
        [TestCase("4.23", 4.23)]
        [TestCase("1.56", 1.56)]
        [TestCase("12.4", 12.4)]
        [TestCase(" 12.4", 12.4)]
        [TestCase(" 12.56 ", 12.56)]
        [TestCase("123 123.123", 123123.123)]
        public void Construct_GivenStringValueWIthPeriodFormattedDecimal_ToDecimalReturnsDecimalValue(string input, decimal expected)
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var decorated = new DecimalDecorator(input);
            var result = decorated.ToDecimal();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Construct_GivenStringDecimalWithPeriodFormatting_ToStringReturnsThatValue()
        {
            //---------------Set up test pack-------------------
            var nfi = new NumberFormatInfo()
            {
                CurrencyDecimalSeparator = ".",
                NumberDecimalSeparator = ".",
                PercentDecimalSeparator = ".",
                CurrencyGroupSeparator = "",
                NumberGroupSeparator = "",
                PercentGroupSeparator = ""
            };
            var input = RandomValueGen.GetRandomDecimal().ToString(nfi);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var decorated = new DecimalDecorator(input);
            var result = decorated.ToString();

            //---------------Test Result -----------------------
            Assert.AreEqual(result, input);
        }

        [Test]
        public void Construct_GivenNullString_ToDecimalReturns0()
        {
            //---------------Set up test pack-------------------
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            DecimalDecorator dd = null;
            Assert.DoesNotThrow(() => dd = new DecimalDecorator(null));

            //---------------Test Result -----------------------
            Assert.AreEqual(0m, dd.ToDecimal());
        }

        [Test]
        public void Construct_GivenDecimalWithFormatting_ToStringHonorsFormatting()
        {
            //---------------Set up test pack-------------------
            var input = 1.234m;
            var format = "0.0";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var decorator = new DecimalDecorator(input, format);
            var result = decorator.ToString();

            //---------------Test Result -----------------------
            Assert.AreEqual("1.2", result);
        }
    }
}
