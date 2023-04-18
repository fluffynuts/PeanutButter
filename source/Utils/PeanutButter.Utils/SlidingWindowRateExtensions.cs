using System;
using System.Linq;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides convenience extensions for sliding windows
    /// </summary>
    public static class SlidingWindowRateExtensions
    {
        // This would be a lot simpler if INumeric<T> was in netstandard...
        /// <summary>
        /// The rate of items per minute, where each integer value
        /// N represents N items, relative to the current time.
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static decimal CurrentRate(this ISlidingWindow<int> window)
        {
            return window.CurrentRate(TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// The rate of items per provided period size, where each
        /// integer value N represents N items, relative to the current time
        /// </summary>
        /// <param name="window"></param>
        /// <param name="periodSize"></param>
        /// <returns></returns>
        public static decimal CurrentRate(
            this ISlidingWindow<int> window,
            TimeSpan periodSize
        )
        {
            window.Trim();
            if (window.Count < 2)
            {
                return 0;
            }

            var sum = window.Sum();
            var totalTime = window.MaxLifeSpan;
            var periods = CalculatePeriodCount(totalTime, periodSize);
            return sum / periods;
        }

        /// <summary>
        /// The rate of items per minute, where each long value
        /// N represents N items, relative to the current time.
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static decimal CurrentRate(this ISlidingWindow<long> window)
        {
            return window.CurrentRate(TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// The rate of items per provided period size, where each
        /// long value N represents N items, relative to the current time
        /// </summary>
        /// <param name="window"></param>
        /// <param name="periodSize"></param>
        /// <returns></returns>
        public static decimal CurrentRate(
            this ISlidingWindow<long> window,
            TimeSpan periodSize
        )
        {
            window.Trim();
            if (window.Count < 2)
            {
                return 0;
            }

            var sum = window.Sum();
            var totalTime = window.MaxLifeSpan;
            var periods = CalculatePeriodCount(totalTime, periodSize);
            return sum / periods;
        }

        /// <summary>
        /// The rate of items per minute, where each decimal value
        /// N represents N items, relative to the current time.
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static decimal CurrentRate(this ISlidingWindow<decimal> window)
        {
            return window.CurrentRate(TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// The rate of items per provided period size, where each
        /// decimal value N represents N items, relative to the current time
        /// </summary>
        /// <param name="window"></param>
        /// <param name="periodSize"></param>
        /// <returns></returns>
        public static decimal CurrentRate(
            this ISlidingWindow<decimal> window,
            TimeSpan periodSize
        )
        {
            window.Trim();
            if (window.Count < 2)
            {
                return 0;
            }

            var sum = window.Sum();
            var totalTime = window.MaxLifeSpan;
            var periods = CalculatePeriodCount(totalTime, periodSize);
            return sum / periods;
        }

        /// <summary>
        /// The rate of items per minute, where each double value
        /// N represents N items, relative to the current time.
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public static decimal CurrentRate(this ISlidingWindow<double> window)
        {
            return window.CurrentRate(TimeSpan.FromSeconds(1));
        }

        /// <summary>
        /// The rate of items per provided period size, where each
        /// double value N represents N items, relative to the current time
        /// </summary>
        /// <param name="window"></param>
        /// <param name="periodSize"></param>
        /// <returns></returns>
        public static decimal CurrentRate(
            this ISlidingWindow<double> window,
            TimeSpan periodSize
        )
        {
            window.Trim();
            if (window.Count < 2)
            {
                return 0;
            }

            var sum = (decimal) window.Sum();
            var totalTime = window.MaxLifeSpan;
            var periods = CalculatePeriodCount(totalTime, periodSize);
            return sum / periods;
        }

        private static decimal CalculatePeriodCount(TimeSpan totalTime, TimeSpan periodSize)
        {
            return (decimal) totalTime.TotalMilliseconds /
                (decimal) periodSize.TotalMilliseconds;
        }
    }
}