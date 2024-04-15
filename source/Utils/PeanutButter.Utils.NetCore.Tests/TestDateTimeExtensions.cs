using System.Globalization;
using PeanutButter.RandomGenerators;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestDateTimeExtensions
{
    [TestFixture]
    public class AsHoursAndMinutes
    {
        [TestCase(0, 0, "00:00")]
        [TestCase(1, 1, "01:01")]
        [TestCase(12, 0, "12:00")]
        [TestCase(16, 30, "16:30")]
        public void ShouldReturnZeroPaddedHoursAndMinutesForDateTime(
            int hours,
            int minutes,
            string expected
        )
        {
            //---------------Set up test pack-------------------
            var randomYear = GetRandomInt(1900, 2000);
            var randomMonth = GetRandomInt(1, 12);
            var randomDay = GetRandomInt(1, 28);
            var date = new DateTime(
                randomYear,
                randomMonth,
                randomDay,
                hours,
                minutes,
                0
            );
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = date.AsHoursAndMinutes();

            //---------------Test Result -----------------------
            Expect(result).To.Equal(expected);
        }
    }

    [TestFixture]
    public class AsTimeString
    {
        [TestCase(0, 0, 0, "00:00:00")]
        [TestCase(1, 1, 1, "01:01:01")]
        [TestCase(12, 0, 15, "12:00:15")]
        [TestCase(16, 30, 33, "16:30:33")]
        public void ShouldReturnZeroPaddedHoursAndMinutesForDateTime(
            int hours,
            int minutes,
            int seconds,
            string expected
        )
        {
            //---------------Set up test pack-------------------
            var randomYear = GetRandomInt(1900, 2000);
            var randomMonth = GetRandomInt(1, 12);
            var randomDay = GetRandomInt(1, 28);
            var date = new DateTime(
                randomYear,
                randomMonth,
                randomDay,
                hours,
                minutes,
                seconds
            );
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = date.AsTimeString();

            //---------------Test Result -----------------------
            Expect(result).To.Equal(expected);
        }
    }

    [TestFixture]
    public class AsMillisecondsSinceStartOfDay
    {
        [Test]
        public void ShouldReturnNumberOfMillisecondsSinceStartOfDay()
        {
            //---------------Set up test pack-------------------
            var dateTime = GetRandomDate().TruncateMicroseconds();
            var expected = 1000 * (dateTime.Hour * 3600 + dateTime.Minute * 60 + dateTime.Second) +
                dateTime.Millisecond;
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = dateTime.MillisecondsSinceStartOfDay();

            //---------------Test Result -----------------------
            Expect(result).To.Equal(expected);
        }
    }

    [TestFixture]
    public class StartOfDay
    {
        [Test]
        public void ShouldReturnDateTimeAsOfStartOfDay()
        {
            //---------------Set up test pack-------------------
            var kind = GetRandomEnum<DateTimeKind>();
            var testDate = GetRandomDate(kind);
            var expected = new DateTime(
                testDate.Year,
                testDate.Month,
                testDate.Day,
                0,
                0,
                0,
                kind
            );

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = testDate.StartOfDay();

            //---------------Test Result -----------------------
            Expect(result).To.Equal(expected);
        }
    }

    [TestFixture]
    public class EndOfDay
    {
        [Test]
        public void ShouldReturnDateTimeAsAtOneMillisecondFromTheEndOfTheDay()
        {
            //---------------Set up test pack-------------------
            var kind = GetRandomEnum<DateTimeKind>();
            var testDate = GetRandomDate(kind);
            var expected = new DateTime(
                testDate.Year,
                testDate.Month,
                testDate.Day,
                23,
                59,
                59,
                999,
                kind
            );

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = testDate.EndOfDay();

            //---------------Test Result -----------------------
            Expect(result).To.Equal(expected);
        }
    }

    [TestFixture]
    public class AsTimeOnly
    {
        [Test]
        public void ShouldReturnTheTimeComponentAttachedToDateTimeDotMinValue()
        {
            //---------------Set up test pack-------------------
            var kind = GetRandomEnum<DateTimeKind>();
            var testDate = GetRandomDate(kind);
            var expected = new DateTime(
                DateTime.MinValue.Year,
                DateTime.MinValue.Month,
                DateTime.MinValue.Day,
                testDate.Hour,
                testDate.Minute,
                testDate.Second,
                testDate.Millisecond,
                kind
            );

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = testDate.AsTimeOnly();

            //---------------Test Result -----------------------
            Expect(result).To.Equal(expected);
        }
    }

    [TestFixture]
    public class WithTime
    {
        [Test]
        public void GivenTimeComponents_ShouldReturnNewDateTimeWithComponentsSet()
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
                DateTimeKind.Local
            );

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = testDate.WithTime(hour, minute, second, millisecond);

            //---------------Test Result -----------------------
            Expect(result).To.Equal(expected);
        }

        [Test]
        public void ShouldPreserveOriginalKind()
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
        public void GivenValidTimeSpan_ShouldSetTime()
        {
            // Arrange
            var kind = GetRandomEnum<DateTimeKind>();
            var date = GetRandomDate(kind);
            var timeSpan = GetRandomTimeOfDay();
            // Act
            var result = date.WithTime(timeSpan);
            // Assert
            Expect(result.TimeOfDay).To.Equal(timeSpan);
            Expect(result.Kind).To.Equal(kind);
        }

        [Test]
        public void GivenTimeSpanLessThanZero_ShouldClampToZero()
        {
            // Arrange
            var kind = GetRandomEnum<DateTimeKind>();
            var date = GetRandomDate(kind);
            var timeSpan = TimeSpan.Zero - TimeSpan.FromMinutes(GetRandomInt(1, 100));
            // Act
            var result = date.WithTime(timeSpan);
            // Assert
            Expect(result.TimeOfDay).To.Equal(TimeSpan.Zero);
        }

        [Test]
        public void GivenTimeSpanGreaterThan24Hours_ShouldClampTo235959()
        {
            // Arrange
            var date = GetRandomDate();
            var expected = TimeSpan.FromHours(24) - TimeSpan.FromMilliseconds(1);
            var timeSpan = expected + TimeSpan.FromSeconds(GetRandomInt(1, 10000));
            // Act
            var result = date.WithTime(timeSpan);
            // Assert
            Expect(result.TimeOfDay).To.Equal(expected);
        }
    }

    [TestFixture]
    public class Truncation
    {
        [TestFixture]
        public class OperatingOnDateTime
        {
            [TestFixture]
            public class TruncateMicroseconds
            {
                [Test]
                public void ShouldTruncateMicroseconds()
                {
                    //---------------Set up test pack-------------------
                    var kind = GetRandomEnum<DateTimeKind>();
                    var input = GetRandomDate(kind);
                    var expected =
                        $"{input.Year}/{ZeroPad(input.Month)}/{ZeroPad(input.Day)} {ZeroPad(input.Hour)}:{ZeroPad(input.Minute)}:{ZeroPad(input.Second)}.{ZeroPad(input.Millisecond, 3)}000";

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var truncated = input.TruncateMicroseconds();
                    var result = GetTruncateTestStringFor(truncated);

                    //---------------Test Result -----------------------
                    Expect(result).To.Equal(expected);
                    Expect(truncated.Kind).To.Equal(kind);
                }
            }

            [TestFixture]
            public class TruncateMilliseconds
            {
                [Test]
                public void ShouldReturnNewDateTimeWithTruncatedMilliseconds()
                {
                    //---------------Set up test pack-------------------
                    var kind = GetRandomEnum<DateTimeKind>();
                    var input = GetRandomDate(kind);
                    var expected =
                        $"{input.Year}/{ZeroPad(input.Month)}/{ZeroPad(input.Day)} {ZeroPad(input.Hour)}:{ZeroPad(input.Minute)}:{ZeroPad(input.Second)}.000000";

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var truncated = input.TruncateMilliseconds();
                    var result = GetTruncateTestStringFor(truncated);

                    //---------------Test Result -----------------------
                    Expect(truncated)
                        .Not.To.Be(input);
                    Expect(result)
                        .To.Equal(expected);
                    Expect(truncated.Kind)
                        .To.Equal(kind);
                }
            }

            [TestFixture]
            public class TruncateSeconds
            {
                [Test]
                public void ShouldReturnNewDateTimeWithSecondsTruncated()
                {
                    //---------------Set up test pack-------------------
                    var input = GetRandomDate();
                    var expected =
                        $"{input.Year}/{ZeroPad(input.Month)}/{ZeroPad(input.Day)} {ZeroPad(input.Hour)}:{ZeroPad(input.Minute)}:00.000000";

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var dateResult = input.TruncateSeconds();
                    var result = GetTruncateTestStringFor(dateResult);

                    //---------------Test Result -----------------------
                    Expect(dateResult)
                        .Not.To.Be(input);
                    Expect(result)
                        .To.Equal(expected);
                    Expect(dateResult.Kind)
                        .To.Equal(input.Kind);
                }
            }

            [TestFixture]
            public class TruncateMinutes
            {
                [Test]
                public void ShouldReturnNewDateTimeWithMinutesTruncated()
                {
                    //---------------Set up test pack-------------------
                    var input = GetRandomDate();
                    var expected =
                        $"{input.Year}/{ZeroPad(input.Month)}/{ZeroPad(input.Day)} {ZeroPad(input.Hour)}:00:00.000000";

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var result = GetTruncateTestStringFor(input.TruncateMinutes());

                    //---------------Test Result -----------------------
                    Expect(result).To.Equal(expected);
                }
            }

            [TestFixture]
            public class TruncateHours
            {
                [Test]
                public void ShouldReturnNewDateTimeWithHoursTruncated()
                {
                    //---------------Set up test pack-------------------
                    var input = GetRandomDate();
                    var expected = $"{input.Year}/{ZeroPad(input.Month)}/{ZeroPad(input.Day)} 00:00:00.000000";

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var dateResult = input.TruncateHours();
                    var result = GetTruncateTestStringFor(dateResult);

                    //---------------Test Result -----------------------
                    Expect(dateResult)
                        .Not.To.Be(input);
                    Expect(result)
                        .To.Equal(expected);
                }
            }

            [TestFixture]
            public class TruncateDays
            {
                [Test]
                public void ShouldReturnNewDateTime_TruncatedAfterMonths_OnFirstOfMonth()
                {
                    //---------------Set up test pack-------------------
                    var input = GetRandomDate();
                    var expected = $"{input.Year}/{ZeroPad(input.Month)}/01 00:00:00.000000";

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var dateResult = input.TruncateDays();
                    var result = GetTruncateTestStringFor(dateResult);

                    //---------------Test Result -----------------------
                    Expect(dateResult)
                        .Not.To.Be(input);
                    Expect(result)
                        .To.Equal(expected);
                }
            }

            [TestFixture]
            public class TruncateMonths
            {
                [Test]
                public void ShouldReturnNewDateTime_TruncatedAfterYears_OnFirstMonth()
                {
                    //---------------Set up test pack-------------------
                    var input = GetRandomDate();
                    var expected = $"{input.Year}/01/01 00:00:00.000000";

                    //---------------Assert Precondition----------------

                    //---------------Execute Test ----------------------
                    var dateResult = input.TruncateMonths();
                    var result = GetTruncateTestStringFor(dateResult);

                    //---------------Test Result -----------------------
                    Expect(dateResult)
                        .Not.To.Be(input);
                    Expect(result)
                        .To.Equal(expected);
                }
            }
        }
    }

    [TestFixture]
    public class Microseconds
    {
        [Test]
        public void ShouldReturnMicrosecondsOnDateTime()
        {
            //---------------Set up test pack-------------------
            var d = DateTime.Now;
            var decimalPart = d.ToString("ffffff");
            var lastThree = decimalPart.Substring(3);

            //---------------Assert Precondition----------------
            Expect(lastThree.Length)
                .To.Equal(3);
            var expected = int.Parse(lastThree);

            //---------------Execute Test ----------------------
            var result = d.Microseconds();

            //---------------Test Result -----------------------
            Expect(result)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    public class IsWithinRange
    {
        [TestFixture]
        public class WhenBothDatesAreBeforeSubject
        {
            [Test]
            public void ShouldReturnFalse()
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
        }

        [TestFixture]
        public class WhenBothDatesAreAfterSubject
        {
            [Test]
            public void ShouldReturnFalse()
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
        }

        [TestFixture]
        public class WhenSubjectIsInRange
        {
            [Test]
            public void ShouldReturnTrue()
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

            [TestFixture]
            public class AndRangeIsInverted
            {
                [Test]
                public void ShouldReturnTrue()
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
            }
        }
    }

    [TestFixture]
    public class ToKind
    {
        [Test]
        public void ShouldConvertToNewDateTimeWithSameValuesAndProvidedKind()
        {
            // Arrange
            var source = GetRandomDate();
            // Pre-assert
            Expect(source.Kind).To.Equal(DateTimeKind.Local);
            // Act
            var result = source.WithKind(DateTimeKind.Utc);
            // Assert
            Expect(result.Kind).To.Equal(DateTimeKind.Utc);
            Expect(result.Year).To.Equal(source.Year);
            Expect(result.Month).To.Equal(source.Month);
            Expect(result.Day).To.Equal(source.Day);
            Expect(result.Hour).To.Equal(source.Hour);
            Expect(result.Minute).To.Equal(source.Minute);
            Expect(result.Second).To.Equal(source.Second);
            Expect(result.Millisecond).To.Equal(source.Millisecond);
            if (TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes == 0)
            {
                // can't do the reverse test below if we're on utc,
                // ie, enjoying some bangers and mash with a warm beer
                return;
            }

            var utc = source.ToUniversalTime();
            Expect(result)
                .Not.To.Equal(utc, () => ".WithKind should not perform an actual TZ conversion");
        }
    }

    [TestFixture]
    public class IsBetween
    {
        [TestFixture]
        public class WhenSubjectIsBetweenDates
        {
            [Test]
            public void ShouldReturnTrue()
            {
                // Arrange
                var subject = GetRandomDate();
                var before = GetRandomDate(subject.AddDays(-GetRandomInt(1)), subject.AddDays(-1));
                var after = GetRandomDate(subject.AddDays(1), subject.AddDays(GetRandomInt(1)));
                // Act
                var result = subject.IsBetween(before, after);
                // Assert
                Expect(result)
                    .To.Be.True();
            }

            [Test]
            public void ShouldReturnTrueWhenSwapped()
            {
                // Arrange
                var subject = GetRandomDate();
                var before = GetRandomDate(subject.AddDays(-GetRandomInt(1)), subject.AddDays(-1));
                var after = GetRandomDate(subject.AddDays(1), subject.AddDays(GetRandomInt(1)));
                // Act
                var result = subject.IsBetween(after, before);
                // Assert
                Expect(result)
                    .To.Be.True();
            }
        }

        [TestFixture]
        public class WhenSubjectBeforeStart
        {
            [Test]
            public void ShouldReturnFalse()
            {
                // Arrange
                var subject = GetRandomDate();
                var before = GetRandomDate(subject.AddDays(GetRandomInt(1)), subject.AddDays(100));
                var after = GetRandomDate(before.AddDays(1), before.AddDays(GetRandomInt(100)));
                Expect(subject)
                    .To.Be.Less.Than(before)
                    .And
                    .To.Be.Less.Than(after);
                // Act
                var result = subject.IsBetween(before, after);
                // Assert
                Expect(result)
                    .To.Be.False(() => $"{
                        subject
                    } should be between {before} and {after}:\n{before} < {subject}: {before < subject}\n{after} > {subject}: {after > subject}");
            }

            [Test]
            public void ShouldReturnFalseWhenSwapped()
            {
                // Arrange
                var subject = GetRandomDate();
                var before = GetRandomDate(subject.AddDays(GetRandomInt(1)), subject.AddDays(100));
                var after = GetRandomDate(before.AddDays(1), before.AddDays(GetRandomInt(100)));
                Expect(subject)
                    .To.Be.Less.Than(before)
                    .And
                    .To.Be.Less.Than(after);
                // Act
                var result = subject.IsBetween(after, before);
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }

        [TestFixture]
        public class WhenSubjectAfterEnd
        {
            [Test]
            public void ShouldReturnFalse()
            {
                // Arrange
                var subject = GetRandomDate();
                var after = GetRandomDate(subject.AddDays(-GetRandomInt(100)), subject.AddDays(-1));
                var before = GetRandomDate(after.AddDays(-GetRandomInt(100)), after.AddDays(-1));
                // Act
                var result = subject.IsBetween(before, after);
                // Assert
                Expect(result)
                    .To.Be.False();
            }

            [Test]
            public void ShouldReturnFalseWhenSwapped()
            {
                // Arrange
                var subject = GetRandomDate();
                var after = GetRandomDate(subject.AddDays(-GetRandomInt(100)), subject.AddDays(-1));
                var before = GetRandomDate(after.AddDays(-GetRandomInt(100)), after.AddDays(-1));
                // Act
                var result = subject.IsBetween(after, before);
                // Assert
                Expect(result)
                    .To.Be.False();
            }
        }
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
                .WithProp(o => o.DateProperty = o.DateProperty.WithKind(DateTimeKind.Utc));
        }
    }


    private static string ZeroPad(int value, int places = 2)
    {
        var result = value.ToString();
        while (result.Length < places)
            result = "0" + result;
        return result;
    }

    private static string GetTruncateTestStringFor(DateTime dateTime)
    {
        return dateTime.ToString("yyyy/MM/dd HH:mm:ss.ffffff", CultureInfo.InvariantCulture);
    }
}