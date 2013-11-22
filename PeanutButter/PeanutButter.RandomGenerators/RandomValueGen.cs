using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeanutButter.RandomGenerators
{
    public class RandomValueGen
    {
        private static readonly Random _rand = new Random();
        private static string _defaultRandomStringChars = "abcdefghijklmnopqrstuvwxyz1234567890";

        public static int GetRandomInt(int minValue = 0, int maxValue = 1000)
        {
            return (int)GetRandomLong(minValue, maxValue);
        }
        public static bool GetRandomBool()
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

        internal static bool GetRandomBoolean()
        {
            return RandomValueGen.GetRandomInt(1, 100) < 50;
        }

        public static string GetRandomEmail()
        {
            return String.Join("", new[] { GetRandomString(), "@", GetRandomString(), ".com" });
        }
    }
}
