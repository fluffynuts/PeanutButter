using System;

namespace
#if BUILD_PEANUTBUTTER_INTERNAL
    Imported.PeanutButter.Utils;
#else
    PeanutButter.Utils;
#endif
/// <summary>
/// Thrown when max retries are exceeded
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
public
#endif
    class RetriesExceededException : Exception
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
        $"{caller} failed after {attempts} attempts: {lastException} ({lastException.Message})",
        lastException
    )
    {
        Caller = caller;
        Attempts = attempts;
        LastException = lastException;
    }
}