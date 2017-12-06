using PeanutButter.TestUtils.Generic.NUnitAbstractions;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Generic
{
    /// <summary>
    /// Provides extensions for decimal values which make testing easier
    /// </summary>
    public static class DecimalExtensions
    {
        /// <summary>
        /// Tests that two decimal values match up to the second decimal place
        /// </summary>
        /// <param name="someValue">Value to compare against</param>
        /// <param name="otherValue">Value to compare with</param>
        public static void ShouldMatch(
            this decimal someValue,
            decimal otherValue
        )
        {
            ShouldMatch(someValue, otherValue, 2);
        }

        /// <summary>
        /// Tests if two decimal values match up to the required decimal places
        /// </summary>
        /// <param name="someValue">Value to compare against</param>
        /// <param name="otherValue">Value to compare with</param>
        /// <param name="toPlaces">Maximum decimal places to consider</param>
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