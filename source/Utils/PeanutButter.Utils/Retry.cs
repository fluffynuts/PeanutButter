using System;

namespace
#if BUILD_PEANUTBUTTER_INTERNAL
    Imported.PeanutButter.Utils;
#else
    PeanutButter.Utils;
#endif
/// <summary>
/// Retries operations up to a specific number of times
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    static class Retry
{
    /// <summary>
    /// Set the max number of runs to allow
    /// </summary>
    /// <param name="maxAttempts"></param>
    /// <returns></returns>
    public static MaxAttemptsRetryContext Max(
        int maxAttempts
    )
    {
        return new MaxAttemptsRetryContext(
            maxAttempts,
            _defaultBackoffStrategyFactory
        );
    }

    /// <summary>
    /// Continues to retry the provided operation
    /// until the given timeout
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public static TimeBasedRetryContext Until(
        DateTime timeout
    )
    {
        var maxRunTime = timeout - DateTime.Now;
        return Until(maxRunTime);
    }

    /// <summary>
    /// Continues to retry the provided operation until
    /// the given timeout
    /// </summary>
    /// <param name="maxRunTime"></param>
    /// <returns></returns>
    public static TimeBasedRetryContext Until(
        TimeSpan maxRunTime
    )
    {
        return new TimeBasedRetryContext(
            maxRunTime,
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
}