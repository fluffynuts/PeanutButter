using System.Collections.Concurrent;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides some common functionality for concurrent collections
    /// - some of this functionality is only required for net462
    ///   as dotnet already contains them
    /// </summary>
    public static class ConcurrentCollectionExtensions
    {
        /// <summary>
        /// Clears the queue
        /// </summary>
        /// <param name="queue"></param>
        /// <typeparam name="T"></typeparam>
        public static void Clear<T>(
            this ConcurrentQueue<T> queue
        )
        {
            while (queue.Count > 0)
            {
                queue.TryDequeue(out _);
            }
        }
        /// <summary>
        /// Clears the bag
        /// </summary>
        /// <param name="bag"></param>
        /// <typeparam name="T"></typeparam>
        public static void Clear<T>(
            this ConcurrentBag<T> bag
        )
        {
            while (bag.Count > 0)
            {
                bag.TryTake(out _);
            }
        }
    }
}