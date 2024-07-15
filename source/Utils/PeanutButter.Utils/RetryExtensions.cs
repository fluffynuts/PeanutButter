using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides common functionality to retry logic
    /// with configurable delay
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class RetryExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="retries"></param>
        /// <param name="retryDelays"></param>
        public static void RunWithRetries(
            this Action action,
            int retries,
            params TimeSpan[] retryDelays
        )
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var func = new Func<bool>(
                () =>
                {
                    action.Invoke();
                    return true;
                }
            );
            func.RunWithRetries(retries, retryDelays);
        }

        /// <summary>
        /// Runs the provided function with the requested
        /// number of retries and provided backoff delays
        /// </summary>
        /// <param name="func"></param>
        /// <param name="retries"></param>
        /// <param name="retryDelays"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static T RunWithRetries<T>(
            this Func<T> func,
            int retries,
            params TimeSpan[] retryDelays
        )
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            if (retryDelays.Length == 0)
            {
                retryDelays = new[]
                {
                    TimeSpan.FromSeconds(0)
                };
            }

            if (retries < 0)
            {
                throw new ArgumentException(
                    $"{nameof(retries)} must be at least 0 (provided value was {retries})",
                    nameof(retries)
                );
            }

            var lastDelay = retryDelays.Last();
            var delayQueue = new Queue<TimeSpan>(retryDelays);
            Exception lastException = null;
            do
            {
                try
                {
                    return func.Invoke();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (retries < 1)
                    {
                        throw;
                    }

                    Thread.Sleep(
                        delayQueue.DequeueOrDefault(fallback: lastDelay)
                    );
                }
            } while (--retries > 0);

            throw lastException ?? new InvalidOperationException(
                "Retries exceeded, no exception recorded"
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asyncAction"></param>
        /// <param name="retries"></param>
        /// <param name="retryDelays"></param>
        public static async Task RunWithRetries(
            this Func<Task> asyncAction,
            int retries,
            params TimeSpan[] retryDelays
        )
        {
            if (asyncAction is null)
            {
                throw new ArgumentNullException(nameof(asyncAction));
            }

            var func = new Func<Task<bool>>(
                async () =>
                {
                    await asyncAction.Invoke();
                    return true;
                }
            );
            await func.RunWithRetries(retries, retryDelays);
        }

        /// <summary>
        /// Runs the provided function with the requested
        /// number of retries and provided backoff delays
        /// </summary>
        /// <param name="func"></param>
        /// <param name="retries"></param>
        /// <param name="retryDelays"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<T> RunWithRetries<T>(
            this Func<Task<T>> func,
            int retries,
            params TimeSpan[] retryDelays
        )
        {
            if (func is null)
            {
                throw new ArgumentNullException(nameof(func));
            }

            if (retries < 0)
            {
                throw new ArgumentException(
                    $"{nameof(retries)} must be at least 0 (provided value was {retries})",
                    nameof(retries)
                );
            }

            if (retryDelays.Length == 0)
            {
                retryDelays = new[]
                {
                    TimeSpan.FromSeconds(0)
                };
            }

            var lastDelay = retryDelays.Last();
            Exception lastException = null;
            var delayQueue = new Queue<TimeSpan>(retryDelays);
            do
            {
                try
                {
                    return await func.Invoke();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Thread.Sleep(
                        delayQueue.DequeueOrDefault(fallback: lastDelay)
                    );
                }
            } while (--retries > 0);

            throw lastException ?? new InvalidOperationException(
                "Should never get here"
            );
        }
    }
}