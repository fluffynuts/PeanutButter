namespace
#if BUILD_PEANUTBUTTER_INTERNAL
    Imported.PeanutButter.Utils;
#else
    PeanutButter.Utils;
#endif
/// <summary>
/// Describes a service which backs off (ie, waits)
/// according to the retry attempt number
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
public
#endif
    interface IBackoffStrategy
{
    /// <summary>
    /// Waits for an amount of time relevant to the attempt
    /// </summary>
    /// <param name="attempt"></param>
    void Backoff(int attempt);
}