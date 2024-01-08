using System;
using PeanutButter.TestUtils.Generic.NUnitAbstractions;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Generic
{
    /// <summary>
    /// Provides assertions against DateTime values
    /// </summary>
    public static class DateTimeAssert
    {
        /// <summary>
        /// Tests if a DateTime value is within a provided range
        /// </summary>
        /// <param name="test">Value to test</param>
        /// <param name="min">Minimum value required</param>
        /// <param name="max">Maxmimum value required</param>
        public static void IsInRange(DateTime test, DateTime min, DateTime max)
        {
            DoAssertion(test, min, max, IsInDateRangeFailMessageFor(test, min, max));
        }

        private static string IsInDateRangeFailMessageFor(DateTime test, DateTime min, DateTime max)
        {
            return $"{test} does not fall in the range {min} - {max}";
        }

        /// <summary>
        /// Tests if a DateTime value is within the provided range by parts
        /// </summary>
        /// <param name="test">Value to test</param>
        /// <param name="minYear">Minimum year to test against</param>
        /// <param name="minMonth">Minimum mont to test against</param>
        /// <param name="minDay">Minimum day to test against</param>
        /// <param name="maxYear">Maximum year to test against</param>
        /// <param name="maxMonth">Maximum month to test against</param>
        /// <param name="maxDay">Maximum day to test against</param>
        public static void IsInRange(
            DateTime test,
            int minYear,
            int minMonth,
            int minDay,
            int maxYear,
            int maxMonth,
            int maxDay
        )
        {
            var minDate = new DateTime(minYear, minMonth, minDay, 0, 0, 0);
            var maxDate = new DateTime(maxYear, maxMonth, maxDay, 0, 0, 0);
            DoAssertion(test,
                        minDate,
                        maxDate,
                        IsInDateRangeFailMessageFor(test, minDate, maxDate));
        }

        /// <summary>
        /// Tests if a given DateTime value has the TIME component within the provided range
        /// </summary>
        /// <param name="test">DateTime value to test</param>
        /// <param name="min">DateTime with minimum time to test with. Date part is ignored.</param>
        /// <param name="max">DateTIme with maximum time to test with. Date part is ignored.</param>
        public static void IsInTimeRange(DateTime test, DateTime min, DateTime max)
        {
            var testMin = new DateTime(test.Year, test.Month, test.Day, min.Hour, min.Minute, min.Second);
            var testMax = new DateTime(test.Year, test.Month, test.Day, max.Hour, max.Minute, max.Second);
            DoAssertion(
                test, testMin, testMax,
                $"Time of {test} does not fall within expected range: {min.AsTimeString()} - {max.AsTimeString()}");
        }

        private static void DoAssertion(DateTime test, DateTime min, DateTime max, string failMessage)
        {
            if (test > max || test < min)
            {
                Assert.Fail(failMessage);
            }
        }
    }
}
