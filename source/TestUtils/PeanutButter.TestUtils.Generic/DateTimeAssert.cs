using System;
using NUnit.Framework;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Generic
{
    public class DateTimeAssert
    {
        public static void IsInRange(DateTime test, DateTime min, DateTime max)
        {
            DoAssertion(test, min, max, IsInDateRangeFailMessageFor(test, min, max));
        }

        private static string IsInDateRangeFailMessageFor(DateTime test, DateTime min, DateTime max)
        {
            return test.ToString() + " does not fall in the range " + min.ToString() + " - " + max.ToString();
        }

        public static void IsInRange(DateTime test, int minYear, int minMonth, int minDay, int maxYear, int maxMonth, int maxDay)
        {
            var minDate = new DateTime(minYear, minMonth, minDay, 0, 0, 0);
            var maxDate = new DateTime(maxYear, maxMonth, maxDay, 0, 0, 0);
            DoAssertion(test, 
                        minDate, 
                        maxDate,
                        IsInDateRangeFailMessageFor(test, minDate, maxDate));
        }

        public static void IsInTimeRange(DateTime test, DateTime min, DateTime max)
        {
            var testMin = new DateTime(test.Year, test.Month, test.Day, min.Hour, min.Minute, min.Second);
            var testMax = new DateTime(test.Year, test.Month, test.Day, max.Hour, max.Minute, max.Second);
            DoAssertion(test, testMin, testMax,
                "Time of " + test.ToString() + " does not fall within expected range: " + min.AsTimeString() + " - " + max.AsTimeString());
        }

        private static void DoAssertion(DateTime test, DateTime min, DateTime max, string failMessage)
        {
            if (test > max || test < min)
                Assert.Fail(failMessage);
        }
    }
}
