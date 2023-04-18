using System;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides convenience functions to predict an estimated time
    /// remaining based on a remaining item count
    /// </summary>
    public static class SlidingWindowEstimatedTimeRemainingExtensions
    {
        /// <summary>
        /// Estimate the time remaining based on the rate in the window
        /// being operated on and the number of items left to process
        /// </summary>
        /// <param name="window"></param>
        /// <param name="remainingItems"></param>
        /// <returns></returns>
        public static TimeSpan EstimatedTimeRemaining(
            this ISlidingWindow<int> window,
            int remainingItems
        )
        {
            var ratePerSecond = window.CurrentRate();
            var remainingSeconds = remainingItems / ratePerSecond;
            return TimeSpan.FromSeconds((double) remainingSeconds);
        }

        /// <summary>
        /// Estimate the time remaining based on the rate in the window
        /// being operated on and the number of items left to process
        /// </summary>
        /// <param name="window"></param>
        /// <param name="remainingItems"></param>
        /// <returns></returns>
        public static TimeSpan EstimatedTimeRemaining(
            this ISlidingWindow<long> window,
            long remainingItems
        )
        {
            var ratePerSecond = window.CurrentRate();
            var remainingSeconds = remainingItems / ratePerSecond;
            return TimeSpan.FromSeconds((double) remainingSeconds);
        }

        /// <summary>
        /// Estimate the time remaining based on the rate in the window
        /// being operated on and the number of items left to process
        /// </summary>
        /// <param name="window"></param>
        /// <param name="remainingItems"></param>
        /// <returns></returns>
        public static TimeSpan EstimatedTimeRemaining(
            this ISlidingWindow<decimal> window,
            decimal remainingItems
        )
        {
            var ratePerSecond = window.CurrentRate();
            var remainingSeconds = remainingItems / ratePerSecond;
            return TimeSpan.FromSeconds((double) remainingSeconds);
        }

        /// <summary>
        /// Estimate the time remaining based on the rate in the window
        /// being operated on and the number of items left to process
        /// </summary>
        /// <param name="window"></param>
        /// <param name="remainingItems"></param>
        /// <returns></returns>
        public static TimeSpan EstimatedTimeRemaining(
            this ISlidingWindow<double> window,
            double remainingItems
        )
        {
            var ratePerSecond = window.CurrentRate();
            var remainingSeconds = (decimal) remainingItems / ratePerSecond;
            return TimeSpan.FromSeconds((double) remainingSeconds);
        }
    }
}