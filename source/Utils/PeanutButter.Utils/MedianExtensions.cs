using System;
using System.Collections.Generic;
using System.Linq;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides .Median() extension methods for numeric collections
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class MedianExtensions
    {
        /// <summary>
        /// Produces the median value from a non-empty collection or
        /// throws for an empty collection
        /// The median is defined as the middle value in a sorted collection
        /// or the average of the two middle values in a sorted collection
        /// with an even number of items in it
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static decimal Median(this IEnumerable<byte> collection)
        {
            return DetermineMedianFor(
                collection,
                i => i,
                (prev, middle) => (prev + middle) / 2
            );
        }

        /// <summary>
        /// Produces the median value from a non-empty collection or
        /// throws for an empty collection
        /// The median is defined as the middle value in a sorted collection
        /// or the average of the two middle values in a sorted collection
        /// with an even number of items in it
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static decimal Median(this IEnumerable<int> collection)
        {
            return DetermineMedianFor(
                collection,
                i => i,
                (prev, middle) => (prev + middle) / 2
            );
        }

        /// <summary>
        /// Produces the median value from a non-empty collection or
        /// throws for an empty collection
        /// The median is defined as the middle value in a sorted collection
        /// or the average of the two middle values in a sorted collection
        /// with an even number of items in it
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static decimal Median(this IEnumerable<uint> collection)
        {
            return DetermineMedianFor(
                collection,
                i => i,
                (prev, middle) => (prev + middle) / 2
            );
        }

        /// <summary>
        /// Produces the median value from a non-empty collection or
        /// throws for an empty collection
        /// The median is defined as the middle value in a sorted collection
        /// or the average of the two middle values in a sorted collection
        /// with an even number of items in it
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static decimal Median(this IEnumerable<long> collection)
        {
            return DetermineMedianFor(
                collection,
                i => i,
                (prev, middle) => (prev + middle) / 2
            );
        }

        /// <summary>
        /// Produces the median value from a non-empty collection or
        /// throws for an empty collection
        /// The median is defined as the middle value in a sorted collection
        /// or the average of the two middle values in a sorted collection
        /// with an even number of items in it
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static decimal Median(this IEnumerable<ulong> collection)
        {
            return DetermineMedianFor(
                collection,
                i => i,
                (prev, middle) => (prev + middle) / 2
            );
        }

        /// <summary>
        /// Produces the median value from a non-empty collection or
        /// throws for an empty collection
        /// The median is defined as the middle value in a sorted collection
        /// or the average of the two middle values in a sorted collection
        /// with an even number of items in it
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static decimal Median(this IEnumerable<float> collection)
        {
            return DetermineMedianFor(
                collection,
                i => (decimal) i,
                (prev, middle) => (prev + middle) / 2
            );
        }

        /// <summary>
        /// Produces the median value from a non-empty collection or
        /// throws for an empty collection
        /// The median is defined as the middle value in a sorted collection
        /// or the average of the two middle values in a sorted collection
        /// with an even number of items in it
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static decimal Median(this IEnumerable<double> collection)
        {
            return DetermineMedianFor(
                collection,
                i => (decimal) i,
                (prev, middle) => (prev + middle) / 2
            );
        }

        /// <summary>
        /// Produces the median value from a non-empty collection or
        /// throws for an empty collection
        /// The median is defined as the middle value in a sorted collection
        /// or the average of the two middle values in a sorted collection
        /// with an even number of items in it
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static decimal Median(this IEnumerable<decimal> collection)
        {
            return DetermineMedianFor(
                collection,
                i => i,
                (prev, middle) => (prev + middle) / 2
            );
        }

        private static decimal DetermineMedianFor<T>(
            IEnumerable<T> collection,
            Func<T, decimal> caster,
            Func<decimal, decimal, decimal> whenCollectionHasEvenItems
        )
        {
            var sorted = collection.OrderBy(o => o).ToArray();
            if (sorted.Length == 0)
            {
                throw new ArgumentException(
                    "unable to calculate Median of empty collection",
                    nameof(collection)
                );
            }

            var middle = sorted.Length / 2;
            if (sorted.Length % 2 == 1)
            {
                return caster(sorted[middle]);
            }

            var prev = sorted[middle - 1];
            return whenCollectionHasEvenItems(
                caster(prev),
                caster(sorted[middle])
            );
        }
    }
}