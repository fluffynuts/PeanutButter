using System;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides some useful extension methods for DateTime values
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class DateTimeExtensions
    {
        /// <summary>
        /// Calculates whether or not a provided DateTime is within the provided range
        /// </summary>
        /// <param name="value">DateTime subject to test</param>
        /// <param name="start">Minimum value of DateTime range</param>
        /// <param name="end">MaximumValue of DateTime range</param>
        /// <returns>True if the value is within the provided range or false if it is not</returns>
        public static bool IsWithinRange(
            this DateTime value,
            DateTime start,
            DateTime end
        )
        {
            var test = value.ToUniversalTime();
            var testStart = start.ToUniversalTime();
            var testEnd = end.ToUniversalTime();
            if (testStart > testEnd)
            {
                // ReSharper disable once SwapViaDeconstruction
                var swap = testEnd;
                testEnd = testStart;
                testStart = swap;
            }

            return test >= testStart.ToUniversalTime() &&
                test <= testEnd.ToUniversalTime();
        }

        /// <summary>
        /// Returns a string representation of the form HH:mm for a DateTime
        /// </summary>
        /// <param name="value">Datetime subject</param>
        /// <returns>String representation of the time part of the subject, in the format HH:mm</returns>
        public static string AsHoursAndMinutes(this DateTime value)
        {
            return value.ToString("HH:mm");
        }

        /// <summary>
        /// Returns a string representation of the form HH:mm:ss for a DateTime
        /// </summary>
        /// <param name="value">Datetime subject</param>
        /// <returns>String representation of the time part of the subject, in the format HH:mm:ss</returns>
        public static string AsTimeString(this DateTime value)
        {
            return value.ToString("HH:mm:ss");
        }

        /// <summary>
        /// Calculates the number of milliseconds since the start of the day for a given DateTime value
        /// </summary>
        /// <param name="value">DateTime subject to calculate for</param>
        /// <returns>Number of milliseconds since the start of the day for the subject DateTime</returns>
        public static long MillisecondsSinceStartOfDay(this DateTime value)
        {
            var timeSpan = new TimeSpan(0, value.Hour, value.Minute, value.Second, value.Millisecond);
            return (long) timeSpan.TotalMilliseconds;
        }

        /// <summary>
        /// Calculates the DateTime of the start of the day for a provided DateTime value
        /// </summary>
        /// <param name="value">Subject DateTime to calculate the start of day for</param>
        /// <returns>A new DateTime value which represents the start of the day for the subject DateTime</returns>
        public static DateTime StartOfDay(this DateTime value)
        {
            return new DateTime(
                value.Year,
                value.Month,
                value.Day,
                0,
                0,
                0,
                value.Kind);
        }

        /// <summary>
        /// Calculates the DateTime of the end of the day for a provided DateTime value
        /// </summary>
        /// <param name="value">Subject DateTime to calculate the end of day for</param>
        /// <returns>A new DateTime value which represents the end of the day for the subject DateTime</returns>
        public static DateTime EndOfDay(this DateTime value)
        {
            return new DateTime(
                value.Year,
                value.Month,
                value.Day,
                23,
                59,
                59,
                999,
                value.Kind);
        }

        /// <summary>
        /// Gets a DateTime value which has the same time components as the provided subject
        /// with minimum values (typically 0 or 1) for the date components
        /// </summary>
        /// <param name="value">Subject DateTime to operate on</param>
        /// <returns>new DateTime with date components set from DateTime.MinValue and time components set from the subject</returns>
        public static DateTime AsTimeOnly(this DateTime value)
        {
            return new DateTime(
                DateTime.MinValue.Year,
                DateTime.MinValue.Month,
                DateTime.MinValue.Day,
                value.Hour,
                value.Minute,
                value.Second,
                value.Millisecond,
                value.Kind);
        }

        /// <summary>
        /// Gets a new DateTime with the time component mutated
        /// </summary>
        /// <param name="value">Subject DateTime to start with</param>
        /// <param name="hour">Hours to set on result</param>
        /// <param name="minute">Minutes to set on result</param>
        /// <param name="second">Seconds to set on result</param>
        /// <param name="millisecond">Milliseconds to set on result</param>
        /// <returns>
        /// A new DateTime with the date component copied from the subject
        /// and the time component set from provided arguments
        /// </returns>
        public static DateTime WithTime(
            this DateTime value,
            int hour,
            int minute,
            int second,
            int millisecond = 0)
        {
            return new DateTime(
                value.Year,
                value.Month,
                value.Day,
                hour,
                minute,
                second,
                millisecond,
                value.Kind);
        }

        /// <summary>
        /// Produces a new DateTime with the time mutated to the time specified
        /// by the provided TimeSpan, clamped to within 24 hours
        /// </summary>
        /// <param name="value">Subject DateTime to start with</param>
        /// <param name="time">Required time</param>
        /// <returns>new DateTime with provided time</returns>
        public static DateTime WithTime(
            this DateTime value,
            TimeSpan time)
        {
            if (time < TimeSpan.Zero)
            {
                time = TimeSpan.Zero;
            }
            else if (time >= TwentyFourHours)
            {
                time = TwentyFourHours - OneMillisecond;
            }

            return value.WithTime(
                time.Hours,
                time.Minutes,
                time.Seconds,
                time.Milliseconds);
        }

        private static readonly TimeSpan TwentyFourHours = TimeSpan.FromHours(24);
        private static readonly TimeSpan OneMillisecond = TimeSpan.FromMilliseconds(1);

        /// <summary>
        /// Provides a new DateTime with all components from the subject except
        /// Microseconds, which are truncated.
        /// </summary>
        /// <param name="value">Subject DateTime to start with</param>
        /// <returns>
        /// New DateTime with all components except milliseconds set from the subject.
        /// Microseconds are set to zero.
        /// </returns>
        public static DateTime TruncateMicroseconds(this DateTime value)
        {
            return DateTimeFor(
                value.Kind,
                value.Year,
                value.Month,
                value.Day,
                value.Hour,
                value.Minute,
                value.Second,
                value.Millisecond);
        }

        /// <summary>
        /// Provides a new DateTime with all components from the subject except
        /// Milliseconds, which are truncated.
        /// </summary>
        /// <param name="value">Subject DateTime to start with</param>
        /// <returns>
        /// New DateTime with all components except milliseconds set from the subject.
        /// Milliseconds are set to zero.
        /// </returns>
        public static DateTime TruncateMilliseconds(this DateTime value)
        {
            return DateTimeFor(
                value.Kind,
                value.Year,
                value.Month,
                value.Day,
                value.Hour,
                value.Minute,
                value.Second);
        }

        /// <summary>
        /// Provides a new DateTime object with the DateTimeKind set as required
        /// </summary>
        /// <param name="value"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static DateTime ToKind(this DateTime value, DateTimeKind kind)
        {
            return DateTimeFor(
                kind,
                value.Year,
                value.Month,
                value.Day,
                value.Hour,
                value.Minute,
                value.Second,
                value.Millisecond);
        }

        /// <summary>
        /// Provides a new DateTime with all components from the subject except
        /// Seconds, which are truncated.
        /// </summary>
        /// <param name="value">Subject DateTime to start with</param>
        /// <returns>
        /// New DateTime with all components except milliSeconds set from the subject.
        /// Seconds are set to zero.
        /// </returns>
        public static DateTime TruncateSeconds(this DateTime value)
        {
            return DateTimeFor(value.Kind, value.Year, value.Month, value.Day, value.Hour, value.Minute);
        }

        /// <summary>
        /// Provides a new DateTime with all components from the subject except
        /// Minutes, which are truncated.
        /// </summary>
        /// <param name="value">Subject DateTime to start with</param>
        /// <returns>
        /// New DateTime with all components except milliMinutes set from the subject.
        /// Minutes are set to zero.
        /// </returns>
        public static DateTime TruncateMinutes(this DateTime value)
        {
            return DateTimeFor(value.Kind, value.Year, value.Month, value.Day, value.Hour);
        }

        /// <summary>
        /// Provides a new DateTime with all components from the subject except
        /// Hours, which are truncated.
        /// </summary>
        /// <param name="value">Subject DateTime to start with</param>
        /// <returns>
        /// New DateTime with all components except milliHours set from the subject.
        /// Hours are set to zero.
        /// </returns>
        public static DateTime TruncateHours(this DateTime value)
        {
            return DateTimeFor(value.Kind, value.Year, value.Month, value.Day);
        }

        /// <summary>
        /// Provides a new DateTime with all components from the subject except
        /// Days, which are truncated.
        /// </summary>
        /// <param name="value">Subject DateTime to start with</param>
        /// <returns>
        /// New DateTime with all components except milliDays set from the subject.
        /// Days are set to zero.
        /// </returns>
        public static DateTime TruncateDays(this DateTime value)
        {
            return DateTimeFor(value.Kind, value.Year, value.Month);
        }

        /// <summary>
        /// Provides a new DateTime with all components from the subject except
        /// Months, which are truncated.
        /// </summary>
        /// <param name="value">Subject DateTime to start with</param>
        /// <returns>
        /// New DateTime with all components except milliMonths set from the subject.
        /// Months are set to zero.
        /// </returns>
        public static DateTime TruncateMonths(this DateTime value)
        {
            return DateTimeFor(value.Kind, value.Year);
        }

        /// <summary>
        /// Gets the microseconds component for a DateTime value
        /// </summary>
        /// <param name="value">Subject DateTime to operate on</param>
        /// <returns>Microseconds component of the subject DateTime value</returns>
        public static int Microseconds(this DateTime value)
        {
            return int.Parse(value.ToString("ffffff").Substring(3));
        }

        private static DateTime DateTimeFor(
            DateTimeKind kind,
            int years,
            int months = 1,
            int days = 1,
            int hours = 0,
            int minutes = 0,
            int seconds = 0,
            int milliseconds = 0)
        {
            return new DateTime(
                years,
                months,
                days,
                hours,
                minutes,
                seconds,
                milliseconds,
                kind);
        }

        /// <summary>
        /// Returns a new TimeSpan equal to the input with truncated milliseconds
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static TimeSpan TruncateMilliseconds(
            this TimeSpan timeSpan
        )
        {
            return new TimeSpan(
                timeSpan.Days,
                timeSpan.Hours,
                timeSpan.Minutes,
                timeSpan.Seconds,
                0
            );
        }

        /// <summary>
        /// Returns a new TimeSpan equal to the input with truncated milliseconds
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static TimeSpan TruncateSeconds(
            this TimeSpan timeSpan
        )
        {
            return new TimeSpan(
                timeSpan.Days,
                timeSpan.Hours,
                timeSpan.Minutes,
                0,
                0
            );
        }

        /// <summary>
        /// Returns a new TimeSpan equal to the input with truncated milliseconds
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static TimeSpan TruncateMinutes(
            this TimeSpan timeSpan
        )
        {
            return new TimeSpan(
                timeSpan.Days,
                timeSpan.Hours,
                0,
                0,
                0
            );
        }

        /// <summary>
        /// Returns a new TimeSpan equal to the input with truncated milliseconds
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static TimeSpan TruncateHours(
            this TimeSpan timeSpan
        )
        {
            return new TimeSpan(
                timeSpan.Days,
                0,
                0,
                0,
                0
            );
        }
    }
}