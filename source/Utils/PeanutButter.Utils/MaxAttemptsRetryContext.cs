using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable MemberCanBePrivate.Global

namespace
#if BUILD_PEANUTBUTTER_INTERNAL
    Imported.PeanutButter.Utils;
#else
    PeanutButter.Utils;
#endif
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
        Times(
            toRun,
            _defaultBackoffStrategyFactory(),
            caller
        );
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
        Times(
            _ => toRun(),
            backoffStrategy,
            AlwaysSuppress,
            caller
        );
    }

    /// <summary>
    /// Run the code...
    /// </summary>
    /// <param name="toRun"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="caller"></param>
    public void Times(
        Action toRun,
        Func<Exception, ExceptionHandlingStrategies> exceptionHandler,
        [CallerMemberName] string caller = null
    )
    {
        Times(
            _ => toRun(),
            _defaultBackoffStrategyFactory(),
            exceptionHandler,
            caller
        );
    }

    /// <summary>
    /// Run the code...
    /// </summary>
    /// <param name="toRun"></param>
    /// <param name="backoffStrategy"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="caller"></param>
    public void Times(
        Action toRun,
        IBackoffStrategy backoffStrategy,
        Func<Exception, ExceptionHandlingStrategies> exceptionHandler,
        [CallerMemberName] string caller = null
    )
    {
        Times(
            _ => toRun(),
            backoffStrategy,
            exceptionHandler,
            caller
        );
    }

    /// <summary>
    /// Run the code...
    /// </summary>
    /// <param name="toRun"></param>
    /// <param name="caller"></param>
    public void Times(
        Action<int> toRun,
        [CallerMemberName] string caller = null
    )
    {
        Times(
            toRun,
            _defaultBackoffStrategyFactory(),
            caller
        );
    }

    /// <summary>
    /// Run the code...
    /// </summary>
    /// <param name="toRun"></param>
    /// <param name="backoffStrategy"></param>
    /// <param name="caller"></param>
    public void Times(
        Action<int> toRun,
        IBackoffStrategy backoffStrategy,
        [CallerMemberName] string caller = null
    )
    {
        Times(
            toRun,
            backoffStrategy,
            AlwaysSuppress,
            caller
        );
    }

    /// <summary>
    /// Run the code...
    /// </summary>
    /// <param name="toRun"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="caller"></param>
    public void Times(
        Action<int> toRun,
        Func<Exception, ExceptionHandlingStrategies> exceptionHandler,
        [CallerMemberName] string caller = null
    )
    {
        Times(
            toRun,
            _defaultBackoffStrategyFactory(),
            exceptionHandler,
            caller
        );
    }

    /// <summary>
    /// Run the code...
    /// </summary>
    /// <param name="toRun"></param>
    /// <param name="backoffStrategy"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="caller"></param>
    public void Times(
        Action<int> toRun,
        IBackoffStrategy backoffStrategy,
        Func<Exception, ExceptionHandlingStrategies> exceptionHandler,
        [CallerMemberName] string caller = null
    )
    {
        Exception lastException = null;
        exceptionHandler ??= AlwaysSuppress;

        for (var i = 0; i < MaxAttempts; i++)
        {
            try
            {
                toRun(i);
                return;
            }
            catch (Exception ex)
            {
                if (exceptionHandler(ex) == ExceptionHandlingStrategies.Throw)
                {
                    throw;
                }

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
        Func<int, Task<T>> toRun,
        [CallerMemberName] string caller = null
    )
    {
        return await Times(
            toRun,
            _defaultBackoffStrategyFactory(),
            caller
        );
    }

    /// <summary>
    /// Run the code...
    /// </summary>
    /// <param name="toRun"></param>
    /// <param name="backoffStrategy"></param>
    /// <param name="caller"></param>
    public async Task<T> Times<T>(
        Func<int, Task<T>> toRun,
        IBackoffStrategy backoffStrategy,
        [CallerMemberName] string caller = null
    )
    {
        return await Times(
            toRun,
            backoffStrategy,
            AlwaysSuppress,
            caller
        );
    }

    /// <summary>
    /// Run the code...
    /// </summary>
    /// <param name="toRun"></param>
    /// <param name="backoffStrategy"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="caller"></param>
    public async Task<T> Times<T>(
        Func<int, Task<T>> toRun,
        IBackoffStrategy backoffStrategy,
        Func<Exception, ExceptionHandlingStrategies> exceptionHandler = null,
        [CallerMemberName] string caller = null
    )
    {
        if (backoffStrategy is null)
        {
            throw new ArgumentNullException(
                nameof(backoffStrategy)
            );
        }

        exceptionHandler ??= AlwaysSuppress;

        Exception lastException = null;

        for (var i = 0; i < MaxAttempts; i++)
        {
            try
            {
                return await toRun(i);
            }
            catch (Exception ex)
            {
                if (exceptionHandler(ex) == ExceptionHandlingStrategies.Throw)
                {
                    throw;
                }

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
        return Times(
            toRun,
            _defaultBackoffStrategyFactory(),
            caller
        );
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
        return Times(
            _ => toRun(),
            backoffStrategy,
            AlwaysSuppress,
            caller
        );
    }

    /// <summary>
    /// Run the code...
    /// </summary>
    /// <param name="toRun"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="caller"></param>
    public T Times<T>(
        Func<T> toRun,
        Func<Exception, ExceptionHandlingStrategies> exceptionHandler,
        [CallerMemberName] string caller = null
    )
    {
        return Times(
            _ => toRun(),
            _defaultBackoffStrategyFactory(),
            exceptionHandler,
            caller
        );
    }

    /// <summary>
    /// Run the code...
    /// </summary>
    /// <param name="toRun"></param>
    /// <param name="caller"></param>
    public T Times<T>(
        Func<int, T> toRun,
        [CallerMemberName] string caller = null
    )
    {
        return Times(
            toRun,
            _defaultBackoffStrategyFactory(),
            AlwaysSuppress,
            caller
        );
    }

    /// <summary>
    /// Run the code...
    /// </summary>
    /// <param name="toRun"></param>
    /// <param name="backoffStrategy"></param>
    /// <param name="caller"></param>
    public T Times<T>(
        Func<int, T> toRun,
        IBackoffStrategy backoffStrategy,
        [CallerMemberName] string caller = null
    )
    {
        return Times(
            toRun,
            backoffStrategy,
            AlwaysSuppress,
            caller
        );
    }

    /// <summary>
    /// Run the code...
    /// </summary>
    /// <param name="toRun"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="caller"></param>
    public T Times<T>(
        Func<int, T> toRun,
        Func<Exception, ExceptionHandlingStrategies> exceptionHandler = null,
        [CallerMemberName] string caller = null
    )
    {
        return Times(
            toRun,
            _defaultBackoffStrategyFactory(),
            exceptionHandler,
            caller
        );
    }

    /// <summary>
    /// Run the code...
    /// </summary>
    /// <param name="toRun"></param>
    /// <param name="backoffStrategy"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="caller"></param>
    public T Times<T>(
        Func<int, T> toRun,
        IBackoffStrategy backoffStrategy,
        Func<Exception, ExceptionHandlingStrategies> exceptionHandler = null,
        [CallerMemberName] string caller = null
    )
    {
        exceptionHandler ??= AlwaysSuppress;
        Exception lastException = null;

        for (var i = 0; i < MaxAttempts; i++)
        {
            try
            {
                return toRun(i);
            }
            catch (Exception ex)
            {
                if (exceptionHandler(ex) == ExceptionHandlingStrategies.Throw)
                {
                    throw;
                }

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
        Func<int, Task> toRun,
        [CallerMemberName] string caller = null
    )
    {
        await Times(
            toRun,
            AlwaysSuppress,
            caller
        );
    }

    /// <summary>
    /// Run the code...
    /// </summary>
    /// <param name="toRun"></param>
    /// <param name="backoffStrategy"></param>
    /// <param name="caller"></param>
    public async Task Times(
        Func<int, Task> toRun,
        IBackoffStrategy backoffStrategy,
        [CallerMemberName] string caller = null
    )
    {
        await Times(
            toRun,
            backoffStrategy,
            AlwaysSuppress,
            caller
        );
    }

    /// <summary>
    /// Run the code...
    /// </summary>
    /// <param name="toRun"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="caller"></param>
    public async Task Times(
        Func<int, Task> toRun,
        Func<Exception, ExceptionHandlingStrategies> exceptionHandler,
        [CallerMemberName] string caller = null
    )
    {
        await Times(
            toRun,
            _defaultBackoffStrategyFactory(),
            exceptionHandler,
            caller
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
            AlwaysSuppress,
            caller
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
        await Times(
            _ => toRun(),
            backoffStrategy,
            AlwaysSuppress,
            caller
        );
    }

    /// <summary>
    /// Run the code...
    /// </summary>
    /// <param name="toRun"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="caller"></param>
    public async Task Times(
        Func<Task> toRun,
        Func<Exception, ExceptionHandlingStrategies> exceptionHandler,
        [CallerMemberName] string caller = null
    )
    {
        await Times(
            _ => toRun(),
            _defaultBackoffStrategyFactory(),
            exceptionHandler,
            caller
        );
    }

    /// <summary>
    /// Run the code...
    /// </summary>
    /// <param name="toRun"></param>
    /// <param name="backoffStrategy"></param>
    /// <param name="exceptionHandler"></param>
    /// <param name="caller"></param>
    public async Task Times(
        Func<int, Task> toRun,
        IBackoffStrategy backoffStrategy,
        Func<Exception, ExceptionHandlingStrategies> exceptionHandler,
        [CallerMemberName] string caller = null
    )
    {
        Exception lastException = null;
        exceptionHandler ??= AlwaysSuppress;

        for (var i = 0; i < MaxAttempts; i++)
        {
            try
            {
                await toRun(i);
                return;
            }
            catch (Exception ex)
            {
                if (exceptionHandler(ex) == ExceptionHandlingStrategies.Throw)
                {
                    throw;
                }

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

    private ExceptionHandlingStrategies AlwaysSuppress(
        Exception ex
    )
    {
        return ExceptionHandlingStrategies.Suppress;
    }
}

/// <summary>
/// When configuring exception handling for Run.*,
/// your handler should return one of these values
/// in response to being called with a thrown exception
/// </summary>
public enum ExceptionHandlingStrategies
{
    /// <summary>
    /// Re-throw the exception
    /// </summary>
    Throw,

    /// <summary>
    /// Suppress the exception 
    /// </summary>
    Suppress
}