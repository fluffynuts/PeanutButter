using NUnit.Framework;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Generic
{
    public static class DecimalExtensions
    {
        public static void ShouldMatch(this decimal someValue, decimal otherValue)
        {
            ShouldMatch(someValue, otherValue, 2);
        }

        public static void ShouldMatch(this decimal someValue, decimal otherValue, int toPlaces)
        {
            if (toPlaces < 1)
            {
                Assert.AreEqual((long) someValue, (long) otherValue);
                return;
            }
            var format = "{0:0." + new string('0', toPlaces) + "}";
            var someValueAsString = GetTruncatedStringValueFor(someValue, format, toPlaces);
            var otherValueAsString = GetTruncatedStringValueFor(otherValue, format, toPlaces);
            Assert.AreEqual(someValueAsString, otherValueAsString);
        }

        private static string GetTruncatedStringValueFor(decimal value, string format, int places)
        {
            var truncated = value.TruncateTo(places);
            return string.Format(format, truncated);
        }
    }
}