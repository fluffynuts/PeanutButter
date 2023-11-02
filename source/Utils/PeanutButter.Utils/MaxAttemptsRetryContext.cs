using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace
#if BUILD_PEANUTBUTTER_INTERNAL
    Imported.PeanutButter.Utils
#else
    PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides a wrapping context around retried code
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class MaxAttemptsRetryContext
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
        public MaxAttemptsRetryContext(
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