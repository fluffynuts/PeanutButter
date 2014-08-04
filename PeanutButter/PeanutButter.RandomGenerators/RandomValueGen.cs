using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PeanutButter.Utils;

namespace PeanutButter.RandomGenerators
{
    public class RandomValueGen
    {
        public static object GetRandomValue<T>()
        {
            var tType = typeof(T);
            if (tType.IsGenericType && tType.GetGenericTypeDefinition() == typeof(Nullable<>))
                tType = Nullable.GetUnderlyingType(tType);
            if (tType == typeof(int))
                return GetRandomInt();
            if (tType == typeof(byte))
                return (byte)GetRandomInt();
            if (tType == typeof(char))
                return (char)GetRandomInt();
            if (tType == typeof(long))
                return (long)GetRandomInt();
            if (tType == typeof(float))
                return (float)GetRandomDecimal();
            if (tType == typeof(double))
                return (double)GetRandomDecimal();
            if (tType == typeof(decimal))
                return GetRandomDecimal();
            if (tType == typeof(DateTime))
                return GetRandomDate();
            if (tType == typeof(string))
                return GetRandomString();
            if (tType == typeof(bool))
                return GetRandomBoolean();
            throw new Exception("Can't get random value for type: '" + tType.Name + "': either too complex or I missed a simple type?");
        }
        private class DefaultRanges
        {
            public const int MINLENGTH_STRING = 8;
            public const int MAXLENGTH_STRING = -1;
            public const int MIN_NUMERIC_VALUE = 0;
            public const int MAX_NUMERIC_VALUE = 10;
        }
        private static readonly Random _rand = new Random();
        private static string _defaultRandomStringChars = "abcdefghijklmnopqrstuvwxyz1234567890";

        public static int GetRandomInt(int minValue = DefaultRanges.MIN_NUMERIC_VALUE, int maxValue = DefaultRanges.MAX_NUMERIC_VALUE)
        {
            return (int)GetRandomLong(minValue, maxValue);
        }
        public static bool GetRandomBoolean()
        {
            return GetRandomInt(1, 100)<50;
        }

        static readonly string[] _mimeTypes = new[] { "text/plain", "text/html", "image/png", "application/pdf", "image/jpeg" };
        public static string GetRandomMIMEType()
        {
            var idx = GetRandomInt(0, _mimeTypes.Length - 1);
            return _mimeTypes[idx];
        }
        public static Int64 GetRandomLong(Int64 minValue = 0, Int64 maxValue = 1000)
        {
            if (minValue > maxValue)
            {
                var swap = minValue;
                minValue = maxValue;
                maxValue = swap;
            }
            var dec = _rand.NextDouble();
            var range = maxValue - minValue + 1;
            return minValue + (Int64)(range * dec);
        }

        public static string GetRandomString(int minLength = 1, int maxLength = 32, string charSet = null)
        {
            var actualLength = GetRandomInt(minLength, maxLength);
            var chars = new List<char>();
            if (charSet == null) charSet = _defaultRandomStringChars;
            var charSetLength = charSet.Length;
            for (var i = 0; i < actualLength; i++)
            {
                var pos = (int)GetRandomInt(0, charSetLength - 1);
                chars.Add(charSet[pos]);
            }
            return String.Join("", chars);
        }


        public static DateTime GetRandomDate(DateTime? minDate = null, DateTime? maxDate = null, bool dateOnly = false, 
                                                DateTime? minTime = null, DateTime? maxTime = null)
        {
            var minTicks = (minDate.HasValue ? minDate.Value : new DateTime(1990, 1, 1)).Ticks;
            var maxTicks = (maxDate.HasValue ? maxDate.Value : new DateTime(2020, 12, 31)).Ticks;
            var actualTicks = GetRandomLong(minTicks, maxTicks);
            var rawDateTime = new DateTime(actualTicks);
            var sanitised = new DateTime(rawDateTime.Year, 
                                            rawDateTime.Month, 
                                            rawDateTime.Day, 
                                            rawDateTime.Hour, 
                                            rawDateTime.Minute, 
                                            rawDateTime.Second);
            return RangeCheckTimeOnRandomDate(minTime, maxTime, dateOnly ? sanitised.StartOfDay() : sanitised);
        }

        private static DateTime RangeCheckTimeOnRandomDate(DateTime? minTime, DateTime? maxTime, DateTime value)
        {
            if (!minTime.HasValue && !maxTime.HasValue)
                return value;
            var rangeStart = minTime.HasValue ? minTime.Value.AsTimeOnly() : value.StartOfDay().AsTimeOnly();
            var rangeEnd = maxTime.HasValue ? maxTime.Value.AsTimeOnly() : value.EndOfDay().AsTimeOnly();
            if (rangeStart > rangeEnd)
            {
                var swap = rangeEnd;
                rangeEnd = rangeStart;
                rangeStart = swap;
            }
            var minTimeTestValue = new DateTime(value.Year, value.Month, value.Day, rangeStart.Hour, rangeStart.Minute, rangeStart.Second, rangeStart.Millisecond);
            var maxTimeTestValue = new DateTime(value.Year, value.Month, value.Day, rangeEnd.Hour, rangeEnd.Minute, rangeEnd.Second, rangeEnd.Millisecond);
            var minDelta = minTimeTestValue - value;
            var maxDelta = value - maxTimeTestValue;
            if (minDelta.TotalMilliseconds > 0)
            {
                var maxAdd = rangeEnd - minTimeTestValue.AsTimeOnly();
                var millisecondsToAdd = GetRandomLong((long) minDelta.TotalMilliseconds, (long) maxAdd.TotalMilliseconds);
                value = value.AddMilliseconds(millisecondsToAdd);
            }
            else if (maxDelta.TotalMilliseconds > 0)
            {
                var maxSubtract = maxTimeTestValue.AsTimeOnly() - rangeStart;
                var millisecondsToSubtract = GetRandomLong((long) maxDelta.TotalMilliseconds, (long) maxSubtract.TotalMilliseconds);
                value = value.AddMilliseconds(-1*millisecondsToSubtract);
            }
            return value;
        }

        public static double GetRandomDouble(double min = 0, double max = 10)
        {
            return (_rand.NextDouble() * (max - min)) + min;
        }

        public static decimal GetRandomDecimal(decimal min = 0, decimal max = 10)
        {
            return (decimal)(GetRandomDouble((double)min, (double)max));
        }

        public static byte[] GetRandomBytes(int minLength = 0, int maxLength = 1024)
        {
            var str = GetRandomString(minLength, maxLength);
            return Encoding.UTF8.GetBytes(str);
        }

        public static string GetRandomEmail()
        {
            return String.Join("", new[] { GetRandomString(), "@", GetRandomString(), ".com" });
        }

        public static string GetRandomFileName()
        {
            return String.Join(".", new[] { GetRandomString(10, 20), GetRandomString(3, 3) });
        }

        public static string GetRandomWords(int min = 10, int max = 50)
        {
            var actual = GetRandomInt(min, max);
            var words = new List<string>();
            for (var i = 0; i < actual; i++)
            {
                words.Add(GetRandomAlphaNumericString(1, 10));
            }
            return String.Join(" ", words);
        }

        public static string GetRandomHttpUrl()
        {
            return String.Join("/", new[] { "http:", "", GetRandomAlphaNumericString() + ".com", GetRandomAlphaNumericString() });
        }

        public static string GetRandomAlphaNumericString(int minLength = DefaultRanges.MINLENGTH_STRING, int maxLength = DefaultRanges.MAXLENGTH_STRING)
        {
            return GetRandomString(minLength, maxLength, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");
        }

        public static string GetRandomAlphaString(int minLength = DefaultRanges.MINLENGTH_STRING, int maxLength = DefaultRanges.MAXLENGTH_STRING)
        {
            return GetRandomString(minLength, maxLength, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
        }

        public static T GetRandomEnum<T>() where T: struct, IConvertible
        {
            var type = typeof(T);
            if (!type.IsEnum)
                throw new ArgumentException("GetRandomEnum cannot be called on something other than an enum ('" + type.Name + "')");
            var possible = Enum.GetValues(type).Cast<T>();
            return GetRandomFrom(possible);
        }

        public static T GetRandomFrom<T>(IEnumerable<T> items)
        {
            var itemArray = items as T[] ?? items.ToArray();
            var upper = itemArray.Count() - 1;
            return itemArray.Skip(RandomValueGen.GetRandomInt(0, upper)).First();
        }

        public static DateTime GetRandomTimeOn(DateTime theDate)
        {
            var min = new DateTime(theDate.Year, theDate.Month, theDate.Day, 0, 0, 0);
            var max = new DateTime(theDate.Year, theDate.Month, theDate.Day, 0, 0, 0);
            max = max.AddDays(1).AddMilliseconds(-1);
            return GetRandomDate(min, max);
        }
    }
}
