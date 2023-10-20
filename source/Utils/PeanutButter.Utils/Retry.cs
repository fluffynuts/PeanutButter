using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Retries operations up to a specific number of times
    /// </summary>
    public static class Retry
    {
        /// <summary>
        /// Set the max number of runs to allow
        /// </summary>
        /// <param name="maxAttempts"></param>
        /// <returns></returns>
        public static RetryContext Max(int maxAttempts)
        {
            return new RetryContext(maxAttempts);
        }

        /// <summary>
        /// Provides a wrapping context around retried code
        /// </summary>
        public class RetryContext
        {
            /// <summary>
            /// Maximum number of times to try before giving up
            /// </summary>
            public int MaxAttempts { get; }

            /// <summary>
            /// Creates a RetryContext for the specified maxRetries
            /// </summary>
            /// <param name="maxAttempts"></param>
            public RetryContext(
                int maxAttempts
            )
            {
                MaxAttempts = maxAttempts;
            }

            /// <summary>
            /// Run the code...
            /// </summary>
            /// <param name="toRun"></param>
            /// <param name="caller"></param>
            public void Times(
                Action toRun,
                [CallerMemberName] string caller = null
            )
            {
                Times(toRun, new DefaultBackoffStrategy(), caller);
            }

            /// <summary>
            /// Run the code...
            /// </summary>
            /// <param name="toRun"></param>
            /// <param name="backoffStrategy"></param>
            /// <param name="caller"></param>
            public void Times(
                Action toRun,
                IBackoffStrategy backoffStrategy,
                [CallerMemberName] string caller = null
            )
            {
                Exception lastException = null;

                for (var i = 0; i < MaxAttempts; i++)
                {
                    try
                    {
                        toRun();
                        return;
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        WaitIfNecessary(backoffStrategy, i);
                    }
                }

                throw new RetriesExceededException(
                    caller,
                    MaxAttempts,
                    lastException
                );
            }

            /// <summary>
            /// Run the code...
            /// </summary>
            /// <param name="toRun"></param>
            /// <param name="caller"></param>
            public async Task<T> Times<T>(
                Func<Task<T>> toRun,
                [CallerMemberName] string caller = null
            )
            {
                return await Times(toRun, new DefaultBackoffStrategy(), caller);
            }

            /// <summary>
            /// Run the code...
            /// </summary>
            /// <param name="toRun"></param>
            /// <param name="backoffStrategy"></param>
            /// <param name="caller"></param>
            public async Task<T> Times<T>(
                Func<Task<T>> toRun,
                IBackoffStrategy backoffStrategy,
                [CallerMemberName] string caller = null
            )
            {
                Exception lastException = null;

                for (var i = 0; i < MaxAttempts; i++)
                {
                    try
                    {
                        return await toRun();
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        WaitIfNecessary(backoffStrategy, i);
                    }
                }

                throw new RetriesExceededException(
                    caller,
                    MaxAttempts,
                    lastException
                );
            }

            /// <summary>
            /// Run the code...
            /// </summary>
            /// <param name="toRun"></param>
            /// <param name="caller"></param>
            public T Times<T>(
                Func<T> toRun,
                [CallerMemberName] string caller = null
            )
            {
                return Times(toRun, new DefaultBackoffStrategy(), caller);
            }

            /// <summary>
            /// Run the code...
            /// </summary>
            /// <param name="toRun"></param>
            /// <param name="backoffStrategy"></param>
            /// <param name="caller"></param>
            public T Times<T>(
                Func<T> toRun,
                IBackoffStrategy backoffStrategy,
                [CallerMemberName] string caller = null
            )
            {
                Exception lastException = null;

                for (var i = 0; i < MaxAttempts; i++)
                {
                    try
                    {
                        return toRun();
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        WaitIfNecessary(backoffStrategy, i);
                    }
                }

                throw new RetriesExceededException(
                    caller,
                    MaxAttempts,
                    lastException
                );
            }

            /// <summary>
            /// Run the code...
            /// </summary>
            /// <param name="toRun"></param>
            /// <param name="caller"></param>
            public async Task Times(
                Func<Task> toRun,
                [CallerMemberName] string caller = null
            )
            {
                await Times(toRun, new DefaultBackoffStrategy());
            }

            /// <summary>
            /// Run the code...
            /// </summary>
            /// <param name="toRun"></param>
            /// <param name="backoffStrategy"></param>
            /// <param name="caller"></param>
            public async Task Times(
                Func<Task> toRun,
                IBackoffStrategy backoffStrategy,
                [CallerMemberName] string caller = null
            )
            {
                Exception lastException = null;

                for (var i = 0; i < MaxAttempts; i++)
                {
                    try
                    {
                        await toRun();
                        return;
                    }
                    catch (Exception ex)
                    {
                        lastException = ex;
                        WaitIfNecessary(backoffStrategy, i);
                    }
                }

                throw new RetriesExceededException(
                    caller,
                    MaxAttempts,
                    lastException
                );
            }

            private void WaitIfNecessary(
                IBackoffStrategy backoffStrategy,
                int attempt
            )
            {
                if (attempt >= (MaxAttempts - 1))
                {
                    return;
                }
                backoffStrategy.Backoff(attempt);
            }
        }
    }

    /// <summary>
    /// Describes a service which backs off (ie, waits)
    /// according to the retry attempt number
    /// </summary>
    public interface IBackoffStrategy
    {
        /// <summary>
        /// Waits for an amount of time relevant to the attempt
        /// </summary>
        /// <param name="attempt"></param>
        void Backoff(int attempt);
    }

    /// <summary>
    /// Default backoff strategy for retries: linear, 100ms per attempt
    /// </summary>
    public class DefaultBackoffStrategy : IBackoffStrategy
    {
        /// <inheritdoc />
        public void Backoff(int attempt)
        {
            Thread.Sleep(100);
        }
    }

    /// <summary>
    /// Thrown when max retries are exceeded
    /// </summary>
    public class RetriesExceededException : Exception
    {
        /// <summary>
        /// The [CallerMemberName] gleaned at the time of running,
        /// if not explicitly set
        /// </summary>
        public string Caller { get; }

        /// <summary>
        /// The number of attempts that were made
        /// </summary>
        public int Attempts { get; }

        /// <summary>
        /// The last exception thrown
        /// </summary>
        public Exception LastException { get; }

        /// <summary>
        /// Creates the exception
        /// </summary>
        /// <param name="caller"></param>
        /// <param name="attempts"></param>
        /// <param name="lastException"></param>
        public RetriesExceededException(
            string caller,
            int attempts,
            Exception lastException
        ) : base(
            $"{caller} failed after {attempts} attempts: {lastException}",
            lastException
        )
        {
            Caller = caller;
            Attempts = attempts;
            LastException = lastException;
        }
    }
}