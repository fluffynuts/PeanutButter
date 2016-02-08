using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeanutButter.Utils;

namespace PeanutButter.RandomGenerators
{
    public class RandomValueGen
    {
        private static Dictionary<Type, Func<object>> _genericGenerators = new Dictionary<Type, Func<object>>()
        {
            { typeof(int), () => GetRandomInt() },
            { typeof(byte), () => Convert.ToByte(GetRandomInt(0, 255)) },
            { typeof(char), () => Convert.ToChar(GetRandomInt(0, 255)) },
            { typeof(long), () => GetRandomLong() },
            { typeof(float), () => Convert.ToSingle(GetRandomDecimal(decimal.MinValue, decimal.MaxValue)) },
            { typeof(double), () => Convert.ToDouble(GetRandomDecimal(decimal.MinValue, decimal.MaxValue)) },
            { typeof(decimal), () => GetRandomDecimal(decimal.MinValue, decimal.MaxValue) },
            { typeof(DateTime), () => GetRandomDate() },
            { typeof(string), () => GetRandomString() },
            { typeof(bool), () => GetRandomBoolean() }
        };
        public static object GetRandomValue<T>()
        {
            var type = typeof(T);
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = Nullable.GetUnderlyingType(type);
            Func<object> randomGenerator;
            if (_genericGenerators.TryGetValue(type, out randomGenerator))
                return (T)randomGenerator();
            throw new Exception("Can't get random value for type: '" + type.Name + "': either too complex or I missed a simple type?");
        }

        private static class DefaultRanges
        {
            public const int MINLENGTH_STRING = 8;
            public const int MAXLENGTH_STRING = -1;
            public const int MIN_NUMERIC_VALUE = 0;
            public const int MAX_NUMERIC_VALUE = 10;
            public const int MIN_ITEMS = 0;
            public const int MAX_ITEMS = 10;
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

        private static readonly string[] MimeTypes = { "text/plain", "text/html", "image/png", "application/pdf", "image/jpeg" };
        public static string GetRandomMIMEType()
        {
            var idx = GetRandomInt(0, MimeTypes.Length - 1);
            return MimeTypes[idx];
        }
        public static long GetRandomLong(long minValue = 0, long maxValue = 1000)
        {
            if (minValue > maxValue)
            {
                var swap = minValue;
                minValue = maxValue;
                maxValue = swap;
            }
            var dec = _rand.NextDouble();
            var range = maxValue - minValue + 1;
            return minValue + (long)(range * dec);
        }

        public static string GetRandomString(int minLength = 1, int maxLength = 32, string charSet = null)
        {
            var actualLength = GetRandomInt(minLength, maxLength);
            var chars = new List<char>();
            if (charSet == null) charSet = _defaultRandomStringChars;
            var charSetLength = charSet.Length;
            for (var i = 0; i < actualLength; i++)
            {
                var pos = GetRandomInt(0, charSetLength - 1);
                chars.Add(charSet[pos]);
            }
            return string.Join(string.Empty, chars.Select(c => c.ToString()).ToArray());
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

        public static double GetRandomDouble(double min = 0, double max = DefaultRanges.MAX_NUMERIC_VALUE)
        {
            return (_rand.NextDouble() * (max - min)) + min;
        }

        public static decimal GetRandomDecimal(decimal min = 0, decimal max = DefaultRanges.MAX_NUMERIC_VALUE)
        {
            return (decimal)GetRandomDouble((double)min, (double)max);
        }

        public static byte[] GetRandomBytes(int minLength = 0, int maxLength = 1024)
        {
            var str = GetRandomString(minLength, maxLength);
            return Encoding.UTF8.GetBytes(str);
        }

        public static string GetRandomEmail()
        {
            return string.Join(string.Empty, GetRandomString(), "@", GetRandomString(), ".com");
        }

        public static string GetRandomFileName()
        {
            return string.Join(".", GetRandomString(10, 20), GetRandomString(3, 3));
        }

        public static string GetRandomWords(int min = 10, int max = 50)
        {
            var actual = GetRandomInt(min, max);
            var words = new List<string>();
            for (var i = 0; i < actual; i++)
            {
                words.Add(GetRandomAlphaNumericString(1, 10));
            }
            return string.Join(" ", words.ToArray());
        }

        public static string GetRandomHttpUrl()
        {
            return string.Join("/", "http:", string.Empty, GetRandomAlphaNumericString(3,12) + ".com", GetRandomAlphaNumericString(0,20));
        }

        public static string GetRandomAlphaNumericString(int minLength = DefaultRanges.MINLENGTH_STRING, int maxLength = DefaultRanges.MAXLENGTH_STRING)
        {
            return GetRandomString(minLength, maxLength, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890");
        }

        public static string GetRandomAlphaString(int minLength = DefaultRanges.MINLENGTH_STRING, int maxLength = DefaultRanges.MAXLENGTH_STRING)
        {
            return GetRandomString(minLength, maxLength, "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
        }

        public static string GetRandomNumericString(int minLength = DefaultRanges.MINLENGTH_STRING, int maxLength = DefaultRanges.MAXLENGTH_STRING)
        {
            return GetRandomString(minLength, maxLength, "1234567890");
        }

        public static T GetRandomEnum<T>() where T: struct, IConvertible
        {
            var type = typeof(T);
            if (!type.IsEnum)
                throw new ArgumentException("GetRandomEnum cannot be called on something other than an enum ('" + type.Name + "')");
            var possible = Enum.GetValues(type).Cast<T>();
            return GetRandomFrom(possible);
        }

        // not refactoring the above to use this as I don't want to pay the boxing/unboxing penalty
        public static object GetRandomEnum(Type enumType)
        {
            if (!enumType.IsEnum)
                throw new ArgumentException("GetRandomEnum cannot be called on something other than an enum ('" + enumType.Name + "')");
            var possible = Enum.GetValues(enumType).Cast<object>();
            return GetRandomFrom(possible);
        }

        public static T GetRandomFrom<T>(IEnumerable<T> items)
        {
            var itemArray = items as T[] ?? items.ToArray();
            var upper = itemArray.Count() - 1;
            return itemArray.Skip(GetRandomInt(0, upper)).First();
        }

        public static T GetRandomFrom<T>(IEnumerable<T> items, params T[] butNot)
        {
            if (items.Count() < butNot.Length - 1)
                throw new ArgumentException("Items collection does not contain enough items to apply the exclusion list, assuming the exclusions are actually in the source list");
            T result;
            do
            {
                result = GetRandomFrom(items);
            } while (butNot.Contains(result));
            return result;
        }

        public static IEnumerable<T> GetRandomSelectionFrom<T>(IEnumerable<T> items, 
            int minValues = DefaultRanges.MIN_ITEMS, int maxValues = DefaultRanges.MAX_ITEMS)
        {
            if (items.Count() == 0)
                return new T[] {};
            if (minValues >= items.Count())
                return items.Randomize();
            if (maxValues > items.Count())
                maxValues = items.Count();
            var howMany = GetRandomInt(minValues, maxValues);
            var result = new List<T>();
            while (result.Count < howMany)
            {
                var toAdd = GetRandomFrom(items);
                if (!result.Contains(toAdd))
                    result.Add(toAdd);
            }
            return result;
        }

        public static DateTime GetRandomTimeOn(DateTime theDate)
        {
            var min = new DateTime(theDate.Year, theDate.Month, theDate.Day, 0, 0, 0);
            var max = new DateTime(theDate.Year, theDate.Month, theDate.Day, 0, 0, 0);
            max = max.AddDays(1).AddMilliseconds(-1);
            return GetRandomDate(min, max);
        }

        public static IEnumerable<T> GetRandomCollection<T>(Func<T> generator, int minValues = 0, int maxValues = DefaultRanges.MAX_ITEMS)
        {
            var howMany = GetRandomInt(minValues, maxValues);
            var result = new List<T>();
            for (var i = 0; i < howMany; i++)
            {
                result.Add(generator());
            }
            return result;
        }

        public const int MAX_DIFFERENT_RANDOM_VALUE_ATTEMPTS = 1000;
        public static T GetAnother<T>(T differentFromThisValue, Func<T> byUsingThisGenerator, Func<T,T,bool> areEqual = null)
        {
            areEqual = areEqual ?? DefaultEqualityTest;
            Func<T, bool> isANewValue = o => !areEqual(differentFromThisValue, o);
            return GetANewRandomValueUsing(differentFromThisValue, byUsingThisGenerator, isANewValue);
        }


        public static T GetAnother<T>(IEnumerable<T> notAnyOfThese, Func<T> byUsingThisGenerator, Func<T,T,bool> areEqual = null)
        {
            areEqual = areEqual ?? DefaultEqualityTest;
            Func<T, bool> isANewValue = o => notAnyOfThese.All(i => !areEqual(o, i));
            return GetANewRandomValueUsing(notAnyOfThese, byUsingThisGenerator, isANewValue);
        }

        private static bool DefaultEqualityTest<T>(T left, T right)
        {
            return left.Equals(right) && right.Equals(left);
        }

        private static T1 GetANewRandomValueUsing<T1, T2>(T2 differentFromThisValue, Func<T1> byUsingThisGenerator, Func<T1, bool> isANewValue)
        {
            var attempts = 0;
            do
            {
                var result = byUsingThisGenerator();
                if (isANewValue(result))
                    return result;
                if (++attempts >= MAX_DIFFERENT_RANDOM_VALUE_ATTEMPTS)
                    throw new CannotGetAnotherDifferentRandomValueException<T2>(differentFromThisValue);
            } while (true);
        }
    }
}
