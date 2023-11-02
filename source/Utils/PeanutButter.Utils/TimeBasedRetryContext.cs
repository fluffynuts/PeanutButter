using System;
using System.Diagnostics;

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
    class TimeBasedRetryContext
{
    private readonly TimeSpan _maxRunTime;
    private readonly Func<IBackoffStrategy> _defaultBackoffStrategyFactory;

    /// <summary>
    /// Constructs the context with the timeout and a mechanism
    /// for backing off on failure
    /// </summary>
    /// <param name="maxRunTime"></param>
    /// <param name="defaultBackoffStrategyFactory"></param>
    public TimeBasedRetryContext(
        TimeSpan maxRunTime,
        Func<IBackoffStrategy> defaultBackoffStrategyFactory
    )
    {
        _defaultBackoffStrategyFactory = defaultBackoffStrategyFactory
            ?? throw new ArgumentNullException(nameof(defaultBackoffStrategyFactory));
        _maxRunTime = maxRunTime;
    }

    /// <summary>
    /// Attempt to run the action until it either succeeds or times out
    /// </summary>
    /// <param name="action"></param>
    public void Do(
        Action action
    )
    {
        Do(action, _defaultBackoffStrategyFactory());
    }

    /// <summary>
    /// Attempt to run the action until it either succeeds or times out
    /// </summary>
    /// <param name="action"></param>
    /// <param name="backoffStrategy"></param>
    public void Do(
        Action action,
        IBackoffStrategy backoffStrategy
    )
    {
        Do(
            () =>
            {
                action();
                return true;
            },
            backoffStrategy
        );
    }

    /// <summary>
    /// Attempt to run the func until it either succeeds or times out,
    /// returning the first successful value
    /// </summary>
    /// <param name="func"></param>
    public T Do<T>(Func<T> func)
    {
        return Do(
            func,
            _defaultBackoffStrategyFactory()
        );
    }

    /// <summary>
    /// Attempt to run the func until it either succeeds or times out,
    /// returning the first successful value
    /// </summary>
    /// <param name="func"></param>
    /// <param name="backoffStrategy"></param>
    public T Do<T>(
        Func<T> func,
        IBackoffStrategy backoffStrategy
    )
    {
        Exception lastException = null;
        var haveRun = false;
        var attempt = 0;
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        do
        {
            if (haveRun)
            {
                backoffStrategy.Backoff(++attempt);
            }

            haveRun = true;

            try
            {
                return func();
            }
            catch (Exception ex)
            {
                lastException = ex;
            }
        } while (stopwatch.Elapsed < _maxRunTime);

        if (lastException is null)
        {
            throw new InvalidOperationException(
                $"maxRunTime was exceeded, but no exception was recorded"
            );
        }

        throw new TimeoutException(
            $"Exceeded retry timeout: {lastException}",
            lastException
        );
    }
}