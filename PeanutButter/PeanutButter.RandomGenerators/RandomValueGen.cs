using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeanutButter.RandomGenerators
{
    public class RandomValueGen
    {
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
            var range = maxValue - minValue;
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

        public static DateTime GetRandomDate(DateTime? minDate = null, DateTime? maxDate = null)
        {
            if (!minDate.HasValue) minDate = new DateTime(1990, 1, 1);
            if (!maxDate.HasValue) maxDate = new DateTime(2020, 12, 31);
            var minTicks = minDate.Value.Ticks;
            var maxTicks = maxDate.Value.Ticks;
            var actualTicks = GetRandomLong(minTicks, maxTicks);
            return new DateTime(actualTicks);
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

    }
}
