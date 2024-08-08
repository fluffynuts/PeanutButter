using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable MemberCanBePrivate.Global

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils;
#else
namespace PeanutButter.Utils;
#endif

/// <summary>
/// Provides a convenience wrapper to
/// time-out a long-running operation
/// which has taken too long
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    static partial class Run
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeoutMs"></param>
    /// <param name="logic"></param>
    public static void WithTimeout(
        int timeoutMs,
        Action logic
    )
    {
        WithTimeout(
            TimeSpan.FromMilliseconds(timeoutMs),
            logic
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeout"></param>
    /// <param name="logic"></param>
    public static void WithTimeout(
        TimeSpan timeout,
        Action logic
    )
    {
        Async.RunSync(
            async () => await WithTimeoutAsync(
                timeout,
                _ =>
                {
                    logic();
                    return Task.FromResult(0);
                }
            )
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeoutMs"></param>
    /// <param name="logic"></param>
    public static async Task WithTimeoutAsync(
        int timeoutMs,
        Func<Task> logic
    )
    {
        await WithTimeoutAsync(
            TimeSpan.FromMilliseconds(timeoutMs),
            logic
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeout"></param>
    /// <param name="logic"></param>
    public static async Task WithTimeoutAsync(
        TimeSpan timeout,
        Func<Task> logic
    )
    {
        await WithTimeoutAsync(
            timeout,
            _ => logic()
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeoutMs"></param>
    /// <param name="logic"></param>
    /// <returns></returns>
    public static async Task WithTimeoutAsync(
        int timeoutMs,
        Func<CancellationToken, Task> logic
    )
    {
        await WithTimeoutAsync(
            TimeSpan.FromMilliseconds(timeoutMs),
            logic
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeout"></param>
    /// <param name="logic"></param>
    /// <returns></returns>
    public static async Task WithTimeoutAsync(
        TimeSpan timeout,
        Func<CancellationToken, Task> logic
    )
    {
        await WithTimeoutAsync(
            timeout,
            async token =>
            {
                await logic(token);
                return 0;
            }
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeoutMs"></param>
    /// <param name="logic"></param>
    public static async Task<T> WithTimeoutAsync<T>(
        int timeoutMs,
        Func<CancellationToken, Task<T>> logic
    )
    {
        return await WithTimeoutAsync(
            TimeSpan.FromMilliseconds(timeoutMs),
            async token => await logic(token)
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeout"></param>
    /// <param name="logic"></param>
    public static async Task<T> WithTimeoutAsync<T>(
        TimeSpan timeout,
        Func<CancellationToken, Task<T>> logic
    )
    {
        return await WithTimeoutAsync(
            timeout,
            logic,
            ErrorStrategies.Throw
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeout"></param>
    /// <param name="logic"></param>
    /// <param name="errorStrategy"></param>
    public static async Task<T> WithTimeoutAsync<T>(
        TimeSpan timeout,
        Func<CancellationToken, Task<T>> logic,
        ErrorStrategies errorStrategy
    )
    {
        var waitCancellationTokenSource = new CancellationTokenSource();
        var waitToken = waitCancellationTokenSource.Token;

        var logicCancellationTokenSource = new CancellationTokenSource();
        var logicToken = logicCancellationTokenSource.Token;

        var barrier = new Barrier(2);

        var t1 = Task.Run(
            async () =>
            {
                try
                {
                    barrier.SignalAndWait(waitToken);
                    await Task.Delay(timeout, waitToken);
                    if (waitCancellationTokenSource.IsCancellationRequested)
                    {
                        return default;
                    }

                    logicCancellationTokenSource.Cancel();
                    return default;
                }
                catch
                {
                    return default(T);
                }
            },
            waitToken
        );
        var t2 = Task.Run(
            async () =>
            {
                try
                {
                    barrier.SignalAndWait(logicToken);
                    var result = await logic(logicToken);
                    if (logicCancellationTokenSource.IsCancellationRequested)
                    {
                        return default;
                    }

                    waitCancellationTokenSource.Cancel();
                    return result;
                }
                catch
                {
                    if (errorStrategy == ErrorStrategies.Throw)
                    {
                        throw;
                    }

                    return default;
                }
            },
            logicToken
        );

        var result = await Task.WhenAny(t1, t2);
        return logicToken.IsCancellationRequested
            ? throw new TimeoutException($"Unable to complete requested logic within {timeout}")
            : await result;
    }
}