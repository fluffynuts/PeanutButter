using System;

namespace PeanutButter.Utils
{
    public static class DateTimeExtensions
    {
        public static string AsHoursAndMinutes(this DateTime value)
        {
            return value.ToString("HH:mm");
        }

        public static string AsTimeString(this DateTime value)
        {
            return value.ToString("HH:mm:ss");
        }

        public static long MillisecondsSinceStartOfDay(this DateTime value)
        {
            var timeSpan = new TimeSpan(0, value.Hour, value.Minute, value.Second, value.Millisecond);
            return (long)timeSpan.TotalMilliseconds;
        }

        public static DateTime StartOfDay(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, 0, 0, 0);
        }

        public static DateTime EndOfDay(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, value.Day, 23, 59, 59, 999);
        }

        public static DateTime AsTimeOnly(this DateTime value)
        {
            return new DateTime(DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day,
                                value.Hour, value.Minute, value.Second, value.Millisecond);
        }

        public static DateTime WithTime(this DateTime value, int hour, int minute, int second, int millisecond = 0)
        {
            return new DateTime(value.Year, value.Month, value.Day, hour, minute, second, millisecond);
        }

        public static DateTime TruncateMicroseconds(this DateTime value)
        {
            return DateTimeFor(value.Kind, value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, value.Millisecond);
        }

        public static DateTime TruncateMilliseconds(this DateTime value)
        {
            return DateTimeFor(value.Kind, value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second);
        }

        public static DateTime TruncateSeconds(this DateTime value)
        {
            return DateTimeFor(value.Kind, value.Year, value.Month, value.Day, value.Hour, value.Minute);
        }

        public static DateTime TruncateMinutes(this DateTime value)
        {
            return DateTimeFor(value.Kind, value.Year, value.Month, value.Day, value.Hour);
        }
        public static DateTime TruncateHours(this DateTime value)
        {
            return DateTimeFor(value.Kind, value.Year, value.Month, value.Day);
        }
        public static DateTime TruncateDays(this DateTime value)
        {
            return DateTimeFor(value.Kind, value.Year, value.Month);
        }

        public static DateTime TruncateMonths(this DateTime value)
        {
            return DateTimeFor(value.Kind, value.Year);
        }

        private static DateTime DateTimeFor(DateTimeKind kind,
                                            int years, 
                                            int months = 1, 
                                            int days = 1, 
                                            int hours = 0, 
                                            int minutes = 0, 
                                            int seconds = 0, 
                                            int milliseconds = 0)
        {
            return new DateTime(years, months, days, hours, minutes, seconds, milliseconds, kind);
        }

        public static int Microseconds(this DateTime value)
        {
            return int.Parse(value.ToString("ffffff").Substring(3));
        }
    }
}
