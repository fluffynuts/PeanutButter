using System;
using System.Threading;

namespace
#if BUILD_PEANUTBUTTER_INTERNAL
    Imported.PeanutButter.Utils;
#else
    PeanutButter.Utils;
#endif
/// <summary>
/// Default backoff strategy for retries: linear, 100ms per attempt
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
public
#endif
    class DefaultBackoffStrategy : IBackoffStrategy
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
/// Provides a constant-time backoff (like the default)
/// but where the caller can set the interval
/// </summary>
    #if BUILD_PEANUTBUTTER_INTERNAL
internal
    #else
public
#endif
    class ConstantTimeBackoffStrategy : IBackoffStrategy
{
    private readonly TimeSpan _interval;

    /// <summary>
    /// The interval between retries
    /// </summary>
    /// <param name="interval"></param>
    public ConstantTimeBackoffStrategy(
        TimeSpan interval
    )
    {
        _interval = interval;
    }

    /// <inheritdoc />
    public void Backoff(int attempt)
    {
        Thread.Sleep(_interval);
    }
}