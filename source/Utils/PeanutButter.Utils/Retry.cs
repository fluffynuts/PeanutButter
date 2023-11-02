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
            return new RetryContext(
                maxAttempts,
                _defaultBackoffStrategyFactory
            );
        }

        /// <summary>
        /// installs a default backoff strategy to use other
        /// than the DefaultBackoffStrategy of PeanutButter.Utils
        /// so you don't have to specify the strategy every time
        /// </summary>
        /// <param name="backoffStrategy"></param>
        public static void InstallDefaultBackoffStrategy(
            IBackoffStrategy backoffStrategy
        )
        {
            InstallDefaultBackoffStrategy(
                () => backoffStrategy
            );
        }

        /// <summary>
        /// installs a factory for the default backoff strategy
        /// to use other than the DefaultBackoffStrategy of
        /// PeanutButter.Utils so you don't have to specify
        /// the strategy every time
        /// </summary>
        /// <param name="factory"></param>
        public static void InstallDefaultBackoffStrategy(
            Func<IBackoffStrategy> factory
        )
        {
            _defaultBackoffStrategyFactory = factory;
        }

        /// <summary>
        /// Corollary to InstallDefaultBackoffStrategy
        /// - will uninstall the default (if any) such that
        ///   DefaultBackoffStrategy from PeanutButter.Utils
        ///   is used instead
        /// </summary>
        public static void ForgetDefaultBackoffStrategy()
        {
            _defaultBackoffStrategyFactory = () => DefaultBackoffStrategyInstance;
        }

        private static Func<IBackoffStrategy> _defaultBackoffStrategyFactory
            = () => DefaultBackoffStrategyInstance;

        private static readonly IBackoffStrategy DefaultBackoffStrategyInstance
            = new DefaultBackoffStrategy();


        /// <summary>
        /// Provides a wrapping context around retried code
        /// </summary>
        public class RetryContext
        {
            /// <summary>
            /// Maximum number of times to try before giving up
            /// </summary>
            public int MaxAttempts { get; }

            // ReSharper disable once MemberHidesStaticFromOuterClass
            private readonly Func<IBackoffStrategy> _defaultBackoffStrategyFactory;

            /// <summary>
            /// Creates a RetryContext for the specified maxRetries
            /// </summary>
            /// <param name="maxAttempts"></param>
            /// <param name="defaultBackoffStrategy"></param>
            public RetryContext(
                int maxAttempts,
                Func<IBackoffStrategy> defaultBackoffStrategy
            )
            {
                _defaultBackoffStrategyFactory = defaultBackoffStrategy ??
                    throw new ArgumentNullException(nameof(defaultBackoffStrategy));
                if (maxAttempts < 1)
                {
                    throw new ArgumentException(
                        $"Cannot retry less than once"
                    );
                }

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
                Times(toRun, _defaultBackoffStrategyFactory(), caller);
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
                return await Times(toRun, _defaultBackoffStrategyFactory(), caller);
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
                if (backoffStrategy is null)
                {
                    throw new ArgumentNullException(
                        nameof(backoffStrategy)
                    );
                }

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
                return Times(toRun, _defaultBackoffStrategyFactory(), caller);
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
                await Times(
                    toRun,
                    _defaultBackoffStrategyFactory()
                );
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

                backoffStrategy.Backoff(attempt + 1);
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
        /// <summary>
        /// The number of milliseconds this will back off with
        /// after each failure
        /// </summary>
        public const int BACKOFF_MILLISECONDS = 100;

        /// <inheritdoc />
        public void Backoff(int attempt)
        {
            Thread.Sleep(BACKOFF_MILLISECONDS);
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