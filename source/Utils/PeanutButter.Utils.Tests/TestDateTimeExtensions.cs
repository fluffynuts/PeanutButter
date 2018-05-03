using System;
using System.Globalization;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using PeanutButter.RandomGenerators;
using static NExpect.Expectations;

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
            Expect(result).To.Equal(expected);
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
            Expect(result).To.Equal(expected);
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
            Expect(result).To.Equal(expected);
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
            Expect(result).To.Equal(expected);
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
            Expect(result).To.Equal(expected);
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
            Expect(result).To.Equal(expected);
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
            var expected = new DateTime(
                testDate.Year, 
                testDate.Month, 
                testDate.Day, 
                hour, 
                minute, 
                second, 
                millisecond,
                DateTimeKind.Local);

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = testDate.WithTime(hour, minute, second, millisecond);

            //---------------Test Result -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void WithTime_ShouldPreserveOriginalKind()
        {
            // Arrange
            var testDate = GetRandomDate(DateTimeKind.Utc);
            var hour = GetRandomInt(0, 23);
            var minute = GetRandomInt(0, 59);
            var second = GetRandomInt(0, 59);
            var millisecond = GetRandomInt(0, 999);
            // Pre-Assert
            // Act
            var result = testDate.WithTime(hour, minute, second, millisecond);
            // Assert
            Expect(result.Kind).To.Equal(testDate.Kind);
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
            Expect(result).To.Equal(expected);
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
            Expect(result).To.Equal(expected);
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
            Expect(result).To.Equal(expected);
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
            Expect(result).To.Equal(expected);
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
            Expect(result).To.Equal(expected);
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
            Expect(result).To.Equal(expected);
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
            Expect(result).To.Equal(expected);
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
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void IsWithinRange_GivenBothDatesAreBeforeSubject_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------
            var subject = GetRandomDate();
            var start = subject.AddDays(-100);
            var end = subject.AddDays(-99);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = subject.IsWithinRange(start, end);

            //--------------- Assert -----------------------
            Expect(result).To.Be.False();
        }

        [Test]
        public void IsWithinRange_GivenBothDatesAreAfterSubject_ShouldReturnFalse()
        {
            //--------------- Arrange -------------------
            var subject = GetRandomDate();
            var start = subject.AddDays(100);
            var end = subject.AddDays(99);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = subject.IsWithinRange(start, end);

            //--------------- Assert -----------------------
            Expect(result).To.Be.False();
        }

        [Test]
        public void IsWithinRange_GivenSubjectIsInRange_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var subject = GetRandomDate();
            var start = subject.AddDays(-1);
            var end = subject.AddDays(2);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = subject.IsWithinRange(start, end);

            //--------------- Assert -----------------------
            Expect(result).To.Be.True();
        }

        [Test]
        public void IsWithinRange_GivenSubjectIsInRange_WhenRangeIsInverted_ShouldReturnTrue()
        {
            //--------------- Arrange -------------------
            var subject = GetRandomDate();
            var start = subject.AddDays(2);
            var end = subject.AddDays(-1);

            //--------------- Assume ----------------

            //--------------- Act ----------------------
            var result = subject.IsWithinRange(start, end);

            //--------------- Assert -----------------------
            Expect(result).To.Be.True();
        }

        [Test]
        public void ToKind_ShouldConvertToNewDateTimeWithSameValuesAndProvidedKind()
        {
            // Arrange
            var source = GetRandomDate();
            // Pre-assert
            Expect(source.Kind).To.Equal(DateTimeKind.Local);
            // Act
            var result = source.ToKind(DateTimeKind.Utc);
            // Assert
            Expect(result.Kind).To.Equal(DateTimeKind.Utc);
            Expect(result.Year).To.Equal(source.Year);
            Expect(result.Month).To.Equal(source.Month);
            Expect(result.Day).To.Equal(source.Day);
            Expect(result.Hour).To.Equal(source.Hour);
            Expect(result.Minute).To.Equal(source.Minute);
            Expect(result.Second).To.Equal(source.Second);
            Expect(result.Millisecond).To.Equal(source.Millisecond);
        }

        public class ThingWithDate
        {
            public DateTime DateProperty { get; set; }
        }

        public class Builder : GenericBuilder<Builder, ThingWithDate>
        {
            public override Builder WithRandomProps()
            {
                return base.WithRandomProps()
                    .WithProp(o => o.DateProperty = o.DateProperty.ToKind(DateTimeKind.Utc));
            }
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
