using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        /// <param name="callerFilePath">do not set - automatically set by the compiler</param>
        /// <param name="callerLineNumber">do not set - automatically set by the compiler</param>
        /// <param name="callerMemberName">do not set - automatically set by the compiler</param>
        public static void RunWithRetries(
            this Action action,
            int retries,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = -1,
            [CallerMemberName] string callerMemberName = null
        )
        {
            action.RunWithRetries(
                retries,
                [],
                // ReSharper disable ExplicitCallerInfoArgument
                callerFilePath,
                callerLineNumber,
                callerMemberName
                // ReSharper restore ExplicitCallerInfoArgument
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="retries"></param>
        /// <param name="retryDelays"></param>
        /// <param name="callerFilePath">do not set - automatically set by the compiler</param>
        /// <param name="callerLineNumber">do not set - automatically set by the compiler</param>
        /// <param name="callerMemberName">do not set - automatically set by the compiler</param>
        public static void RunWithRetries(
            this Action action,
            int retries,
            TimeSpan[] retryDelays,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = -1,
            [CallerMemberName] string callerMemberName = null
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
            func.RunWithRetries(
                retries,
                retryDelays,
                // ReSharper disable ExplicitCallerInfoArgument
                callerFilePath,
                callerLineNumber,
                callerMemberName
                // ReSharper restore ExplicitCallerInfoArgument
            );
        }

        /// <summary>
        /// Runs the provided function with the requested
        /// number of retries and provided backoff delays
        /// </summary>
        /// <param name="func"></param>
        /// <param name="retries"></param>
        /// <param name="callerFilePath">do not set - automatically set by the compiler</param>
        /// <param name="callerLineNumber">do not set - automatically set by the compiler</param>
        /// <param name="callerMemberName">do not set - automatically set by the compiler</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static T RunWithRetries<T>(
            this Func<T> func,
            int retries,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = -1,
            [CallerMemberName] string callerMemberName = null
        )
        {
            return RunWithRetries(
                func,
                retries,
                [],
                // ReSharper disable ExplicitCallerInfoArgument
                callerFilePath,
                callerLineNumber,
                callerMemberName
                // ReSharper restore ExplicitCallerInfoArgument
            );
        }

        /// <summary>
        /// Runs the provided function with the requested
        /// number of retries and provided backoff delays
        /// </summary>
        /// <param name="func"></param>
        /// <param name="retries"></param>
        /// <param name="retryDelays"></param>
        /// <param name="callerFilePath">do not set - automatically set by the compiler</param>
        /// <param name="callerLineNumber">do not set - automatically set by the compiler</param>
        /// <param name="callerMemberName">do not set - automatically set by the compiler</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static T RunWithRetries<T>(
            this Func<T> func,
            int retries,
            TimeSpan[] retryDelays,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = -1,
            [CallerMemberName] string callerMemberName = null
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
            Exception lastException;
            var remaining = retries;
            var attempt = 0;
            do
            {
                try
                {
                    attempt++;
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
            } while (--remaining > 0);

            // ReSharper disable once ConstantNullCoalescingCondition
            throw lastException ?? new InvalidOperationException(
                $"Should never get here (attempts: {attempt}/{retries}; {callerMemberName} at {callerFilePath}:{callerLineNumber})"
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asyncFunc"></param>
        /// <param name="retries"></param>
        /// <param name="retryDelays"></param>
        /// <param name="callerFilePath"></param>
        /// <param name="callerLineNumber"></param>
        /// <param name="callerMemberName"></param>
        public static async Task RunWithRetries(
            this Func<Task> asyncFunc,
            int retries,
            TimeSpan[] retryDelays,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = -1,
            [CallerMemberName] string callerMemberName = null
        )
        {
            if (asyncFunc is null)
            {
                throw new ArgumentNullException(nameof(asyncFunc));
            }

            var func = new Func<Task<bool>>(
                async () =>
                {
                    await asyncFunc.Invoke();
                    return true;
                }
            );
            await func.RunWithRetries(
                retries,
                retryDelays,
                // ReSharper disable ExplicitCallerInfoArgument
                callerFilePath,
                callerLineNumber,
                // ReSharper restore ExplicitCallerInfoArgument
                callerMemberName
            );
        }

        /// <summary>
        /// Runs the provided function with the requested
        /// number of retries and provided backoff delays
        /// </summary>
        /// <param name="func"></param>
        /// <param name="retries"></param>
        /// <param name="callerFilePath">do not set - automatically set by the compiler</param>
        /// <param name="callerLineNumber">do not set - automatically set by the compiler</param>
        /// <param name="callerMemberName">do not set - automatically set by the compiler</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<T> RunWithRetries<T>(
            this Func<Task<T>> func,
            int retries,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = -1,
            [CallerMemberName] string callerMemberName = null
        )
        {
            return await RunWithRetries(
                func,
                retries,
                [],
                // ReSharper disable ExplicitCallerInfoArgument
                callerFilePath,
                callerLineNumber,
                // ReSharper restore ExplicitCallerInfoArgument
                callerMemberName
            );
        }

        /// <summary>
        /// Runs the provided function with the requested
        /// number of retries and provided backoff delays
        /// </summary>
        /// <param name="func"></param>
        /// <param name="retries"></param>
        /// <param name="retryDelays"></param>
        /// <param name="callerFilePath">do not set - automatically set by the compiler</param>
        /// <param name="callerLineNumber">do not set - automatically set by the compiler</param>
        /// <param name="callerMemberName">do not set - automatically set by the compiler</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<T> RunWithRetries<T>(
            this Func<Task<T>> func,
            int retries,
            TimeSpan[] retryDelays,
            [CallerFilePath] string callerFilePath = null,
            [CallerLineNumber] int callerLineNumber = -1,
            [CallerMemberName] string callerMemberName = null
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
            Exception lastException;
            var delayQueue = new Queue<TimeSpan>(retryDelays);
            var attempt = 0;
            var remaining = retries;
            do
            {
                try
                {
                    attempt++;
                    return await func.Invoke();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Thread.Sleep(
                        delayQueue.DequeueOrDefault(fallback: lastDelay)
                    );
                }
            } while (--remaining > 0);

            // ReSharper disable once ConstantNullCoalescingCondition
            throw lastException ?? new InvalidOperationException(
                $"Should never get here (attempts: {attempt}/{retries}; {callerMemberName} at {callerFilePath}:{callerLineNumber})"
            );
        }
    }
}