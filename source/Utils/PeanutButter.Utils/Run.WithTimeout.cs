using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeanutButter.Utils;

/// <summary>
/// Provides a convenience wrapper to
/// time-out a long-running operation
/// which has taken too long
/// </summary>
public static partial class Run
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
        var waitCancellationTokenSource = new CancellationTokenSource();
        var waitToken = waitCancellationTokenSource.Token;

        var logicCancellationTokenSource = new CancellationTokenSource();
        var logicToken = logicCancellationTokenSource.Token;

        var t1 = Task.Run(
            async () =>
            {
                await Task.Delay(timeout, waitToken);
                if (waitCancellationTokenSource.IsCancellationRequested)
                {
                    return default(T);
                }

                logicCancellationTokenSource.Cancel();
                return default(T);
            },
            waitToken
        );
        var t2 = Task.Run(
            async () =>
            {
                var result = await logic(logicToken);
                if (logicCancellationTokenSource.IsCancellationRequested)
                {
                    return default(T);
                }

                waitCancellationTokenSource.Cancel();
                return result;
            },
            logicToken
        );

        var result = await Task.WhenAny(t1, t2);
        return logicToken.IsCancellationRequested
            ? throw new TimeoutException($"Unable to complete requested logic within {timeout}")
            : await result;
    }
}