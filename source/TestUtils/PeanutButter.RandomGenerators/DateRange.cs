using System;

namespace PeanutButter.RandomGenerators
{
    public class DateRange
    {
        public DateTime From { get; }
        public DateTime To { get; }

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

        public bool InRange(DateTime value)
        {
            return value >= From && value <= To;
        }
    }
}