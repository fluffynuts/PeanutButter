using System;
using System.Globalization;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestDateTimeExtensions
    {
        [TestCase(0, 0, "00:00")]
        [TestCase(1, 1, "01:01")]
        [TestCase(12, 0, "12:00")]
        [TestCase(16, 30, "16:30")]
        public void AsHoursAndMinutes_ShouldReturnZeroPaddedHoursAndMinutesForDateTime(int hours, int minutes, string expected)
        {
            //---------------Set up test pack-------------------
            var randomYear = GetRandomInt(1900, 2000);
            var randomMonth = GetRandomInt(1, 12);
            var randomDay = GetRandomInt(1, 28);
            var date = new DateTime(randomYear,
                                        randomMonth,
                                        randomDay,
                                        hours, minutes, 0);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = date.AsHoursAndMinutes();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [TestCase(0, 0, 0, "00:00:00")]
        [TestCase(1, 1, 1, "01:01:01")]
        [TestCase(12, 0, 15, "12:00:15")]
        [TestCase(16, 30, 33, "16:30:33")]
        public void AsTimeString_ShouldReturnZeroPaddedHoursAndMinutesForDateTime(int hours, int minutes, int seconds, string expected)
        {
            //---------------Set up test pack-------------------
            var randomYear = GetRandomInt(1900, 2000);
            var randomMonth = GetRandomInt(1, 12);
            var randomDay = GetRandomInt(1, 28);
            var date = new DateTime(randomYear,
                                        randomMonth,
                                        randomDay,
                                        hours, minutes, seconds);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = date.AsTimeString();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void MillisecondsSinceStartOfDay_ShouldReturnNumberOfMillisecondsSinceStartOfDay()
        {
            //---------------Set up test pack-------------------
            var dateTime = GetRandomDate().TruncateMicroseconds();
            var expected = 1000 * (dateTime.Hour * 3600 + dateTime.Minute * 60 + dateTime.Second) + dateTime.Millisecond;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = dateTime.MillisecondsSinceStartOfDay();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void StartOfDay_ShouldReturnDateTimeAsOfStartOfDay()
        {
            //---------------Set up test pack-------------------
            var testDate = GetRandomDate();
            var expected = new DateTime(testDate.Year, testDate.Month, testDate.Day, 0, 0, 0);
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = testDate.StartOfDay();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void EndOfDay_ShouldReturnDateTimeAsAtOneMillisecondFromTheEndOfTheDay()
        {
            //---------------Set up test pack-------------------
            var testDate = GetRandomDate();
            var expected = new DateTime(testDate.Year, testDate.Month, testDate.Day, 23, 59, 59, 999);
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = testDate.EndOfDay();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void AsTimeOnly_ShouldReturnTheTimeComponentAttachedToDateTimeDotMinValue()
        {
            //---------------Set up test pack-------------------
            var testDate = GetRandomDate();
            var expected = new DateTime(DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, 
                                testDate.Hour, testDate.Minute, testDate.Second, testDate.Millisecond);
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = testDate.AsTimeOnly();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void WithTime_GivenTimeComponents_ShouldReturnNewDateTimeWithComponentsSet()
        {
            //---------------Set up test pack-------------------
            var testDate = GetRandomDate();
            var hour = GetRandomInt(0, 23);
            var minute = GetRandomInt(0, 59);
            var second = GetRandomInt(0, 59);
            var millisecond = GetRandomInt(0, 999);
            var expected = new DateTime(testDate.Year, testDate.Month, testDate.Day, hour, minute, second, millisecond);
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = testDate.WithTime(hour, minute, second, millisecond);

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }


        [Test]
        public void TruncateMicroseconds_ShouldReturnNewDateTime_TruncatedAfterMilliseconds()
        {
            //---------------Set up test pack-------------------
            var input = GetRandomDate();
            var expected = $"{input.Year}/{zeroPad(input.Month)}/{zeroPad(input.Day)} {zeroPad(input.Hour)}:{zeroPad(input.Minute)}:{zeroPad(input.Second)}.{zeroPad(input.Millisecond, 3)}000";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = GetTruncateTestStringFor(input.TruncateMicroseconds());

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TruncateMilliseconds_ShouldReturnNewDateTime_TruncatedAfterSeconds()
        {
            //---------------Set up test pack-------------------
            var input = GetRandomDate();
            var expected = $"{input.Year}/{zeroPad(input.Month)}/{zeroPad(input.Day)} {zeroPad(input.Hour)}:{zeroPad(input.Minute)}:{zeroPad(input.Second)}.000000";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = GetTruncateTestStringFor(input.TruncateMilliseconds());

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TruncateSeconds_ShouldReturnNewDateTime_TruncatedAfterMinutes()
        {
            //---------------Set up test pack-------------------
            var input = GetRandomDate();
            var expected = $"{input.Year}/{zeroPad(input.Month)}/{zeroPad(input.Day)} {zeroPad(input.Hour)}:{zeroPad(input.Minute)}:00.000000";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var dateResult = input.TruncateSeconds();
            Assert.AreEqual(input.Kind, dateResult.Kind);
            var result = GetTruncateTestStringFor(dateResult);

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TruncateMinutes_ShouldReturnNewDateTime_TruncatedAfterHours()
        {
            //---------------Set up test pack-------------------
            var input = GetRandomDate();
            var expected = $"{input.Year}/{zeroPad(input.Month)}/{zeroPad(input.Day)} {zeroPad(input.Hour)}:00:00.000000";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = GetTruncateTestStringFor(input.TruncateMinutes());

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TruncateHours_ShouldReturnNewDateTime_TruncatedAfterDays()
        {
            //---------------Set up test pack-------------------
            var input = GetRandomDate();
            var expected = $"{input.Year}/{zeroPad(input.Month)}/{zeroPad(input.Day)} 00:00:00.000000";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = GetTruncateTestStringFor(input.TruncateHours());

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TruncateDays_ShouldReturnNewDateTime_TruncatedAfterMonths_OnFirstOfMonth()
        {
            //---------------Set up test pack-------------------
            var input = GetRandomDate();
            var expected = $"{input.Year}/{zeroPad(input.Month)}/01 00:00:00.000000";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = GetTruncateTestStringFor(input.TruncateDays());

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TruncateMonths_ShouldReturnNewDateTime_TruncatedAfterYearss_OnFirstMonth()
        {
            //---------------Set up test pack-------------------
            var input = GetRandomDate();
            var expected = $"{input.Year}/01/01 00:00:00.000000";

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = GetTruncateTestStringFor(input.TruncateMonths());

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void Microseconds_ShouldReturnMicrosecondsOnDateTime()
        {
            //---------------Set up test pack-------------------
            var d = DateTime.Now;
            var decimalPart = d.ToString("ffffff");
            var lastThree = decimalPart.Substring(3);

            //---------------Assert Precondition----------------
            Assert.AreEqual(3, lastThree.Length);
            var expected = int.Parse(lastThree);

            //---------------Execute Test ----------------------
            var result = d.Microseconds();

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }


        private string zeroPad(int value, int places = 2)
        {
            var result = value.ToString();
            while (result.Length < places)
                result = "0" + result;
            return result;
        }
        private string GetTruncateTestStringFor(DateTime dateTime)
        {
            return dateTime.ToString("yyyy/MM/dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture);
        }
    }
}
