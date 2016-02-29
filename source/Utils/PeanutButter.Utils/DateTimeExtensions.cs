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
            var timeSpan = new TimeSpan(value.Hour, value.Minute, value.Second);
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
    }
}
