using System;
using NUnit.Framework;
using PeanutButter.RandomGenerators;

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
            var randomYear = RandomValueGen.GetRandomInt(1900, 2000);
            var randomMonth = RandomValueGen.GetRandomInt(1, 12);
            var randomDay = RandomValueGen.GetRandomInt(1, 28);
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
            var randomYear = RandomValueGen.GetRandomInt(1900, 2000);
            var randomMonth = RandomValueGen.GetRandomInt(1, 12);
            var randomDay = RandomValueGen.GetRandomInt(1, 28);
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
            var dateTime = RandomValueGen.GetRandomDate();
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
            var testDate = RandomValueGen.GetRandomDate();
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
            var testDate = RandomValueGen.GetRandomDate();
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
            var testDate = RandomValueGen.GetRandomDate();
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
            var testDate = RandomValueGen.GetRandomDate();
            var hour = RandomValueGen.GetRandomInt(0, 23);
            var minute = RandomValueGen.GetRandomInt(0, 59);
            var second = RandomValueGen.GetRandomInt(0, 59);
            var millisecond = RandomValueGen.GetRandomInt(0, 999);
            var expected = new DateTime(testDate.Year, testDate.Month, testDate.Day, hour, minute, second, millisecond);
            
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = testDate.WithTime(hour, minute, second, millisecond);

            //---------------Test Result -----------------------
            Assert.AreEqual(expected, result);
        }

    }
}
