using System;

namespace PeanutButter.RandomGenerators
{
    /// <summary>
    /// Holds a date range
    /// </summary>
    public class DateRange
    {
        /// <summary>
        /// Start of the date range
        /// </summary>

        public DateTime From { get; }
        /// <summary>
        /// End of the date range
        /// </summary>
        public DateTime To { get; }

        /// <summary>
        /// Constructs a new DateRange object, ensuring that {from} is less than {to}
        /// </summary>
        /// <param name="from">Date to start the range</param>
        /// <param name="to">Date for the end of the range</param>
        public DateRange(DateTime from, DateTime to)
        {
            From = from;
            To = to;
            if (From > To)
            {
                var swap = From;
                From = To;
                To = swap;
            }
        }

        /// <summary>
        /// Tests whether a provided DateTime value is within the stored range
        /// </summary>
        /// <param name="value">Value to test</param>
        /// <returns>True if the value falls within the date range</returns>
        public bool InRange(DateTime value)
        {
            return value >= From && value <= To;
        }
    }
}