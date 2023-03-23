using System.Collections.Generic;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides some useful extensions for Queues
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class QueueExtensions
    {
        /// <summary>
        /// Attempt to Dequeue on the queue (NOT thread-safe). If
        /// successful, return true and set the output result.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryDequeue<T>(
            this Queue<T> queue,
            out T result
        )
        {
            if (queue.Count > 0)
            {
                result = queue.Dequeue();
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Attempt to dequeue or return the default value of T
        /// </summary>
        /// <param name="queue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T DequeueOrDefault<T>(
            this Queue<T> queue
        )
        {
            return queue.DequeueOrDefault(default);
        }

        /// <summary>
        /// Attempt to Dequeue on the queue (NOT thread-safe).
        /// If unsuccessful, return the provided fallback value.
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="fallback"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T DequeueOrDefault<T>(
            this Queue<T> queue,
            T fallback
        )
        {
            return queue.TryDequeue(out var queueResult)
                ? queueResult
                : fallback;
        }
    }
}