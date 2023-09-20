using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;
using PeanutButter.RandomGenerators;

namespace PeanutButter.Utils.NetCore.Tests
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
        public void Construct_GivenStringValueWIthPeriodFormattedDecimal_ToDecimalReturnsDecimalValue(string input,
            decimal expected)
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

        [Test]
        public void PerformanceVsDecimalParse()
        {
            // Arrange
            var expected = 1.2345M;
            var input = expected.ToString(CultureInfo.CurrentCulture); // get the local representation
            var stopwatch = new Stopwatch();
            var runs = 100000;
            var warmup = new DecimalDecorator(input).ToDecimal();
            // Act
            var parseTime = Time(runs, stopwatch, () => decimal.Parse(input));
            var sutTime = Time(runs, stopwatch, () => DecimalDecorator.Parse(input));
            // Assert
            Console.WriteLine($"Run times:\ninternal: {parseTime}ms\ndecorator: {sutTime}ms");
        }

        private static long Time(
            int runs,
            Stopwatch stopwatch,
            Action toRun
        )
        {
            stopwatch.Reset();
            stopwatch.Start();
            for (var i = 0; i < runs; i++)
            {
                toRun();
            }
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
    }
}