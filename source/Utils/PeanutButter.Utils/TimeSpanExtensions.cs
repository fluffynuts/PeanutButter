using System;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils;
#else
namespace PeanutButter.Utils;
#endif
/// <summary>
/// Adds methods similar to those found on
/// DateTime to TimeSpan
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    static class TimeSpanExtensions
{
    /// <summary>
    /// Add this many days to the TimeSpan,
    /// returning the new value
    /// </summary>
    /// <param name="ts"></param>
    /// <param name="howMany"></param>
    /// <returns></returns>
    public static TimeSpan AddDays(
        this TimeSpan ts,
        double howMany
    )
    {
        return ts.Add(
            TimeSpan.FromDays(howMany)
        );
    }

    /// <summary>
    /// Add this many days to the TimeSpan,
    /// returning the new value
    /// </summary>
    /// <param name="ts"></param>
    /// <param name="howMany"></param>
    /// <returns></returns>
    public static TimeSpan AddHours(
        this TimeSpan ts,
        double howMany
    )
    {
        return ts.Add(
            TimeSpan.FromHours(howMany)
        );
    }

    /// <summary>
    /// Add this many days to the TimeSpan,
    /// returning the new value
    /// </summary>
    /// <param name="ts"></param>
    /// <param name="howMany"></param>
    /// <returns></returns>
    public static TimeSpan AddMinutes(
        this TimeSpan ts,
        double howMany
    )
    {
        return ts.Add(
            TimeSpan.FromMinutes(howMany)
        );
    }

    /// <summary>
    /// Add this many days to the TimeSpan,
    /// returning the new value
    /// </summary>
    /// <param name="ts"></param>
    /// <param name="howMany"></param>
    /// <returns></returns>
    public static TimeSpan AddSeconds(
        this TimeSpan ts,
        double howMany
    )
    {
        return ts.Add(
            TimeSpan.FromSeconds(howMany)
        );
    }

    /// <summary>
    /// Add this many days to the TimeSpan,
    /// returning the new value
    /// </summary>
    /// <param name="ts"></param>
    /// <param name="howMany"></param>
    /// <returns></returns>
    public static TimeSpan AddMilliseconds(
        this TimeSpan ts,
        double howMany
    )
    {
        return ts.Add(
            TimeSpan.FromMilliseconds(howMany)
        );
    }

    // TODO
    // public static TimeSpan TruncateMicroseconds(
    //     this TimeSpan timeSpan
    // )
    // {
    // }

    /// <summary>
    /// Returns a new TimeSpan equal to the input with truncated milliseconds
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public static TimeSpan TruncateMilliseconds(
        this TimeSpan timeSpan
    )
    {
        return new TimeSpan(
            timeSpan.Days,
            timeSpan.Hours,
            timeSpan.Minutes,
            timeSpan.Seconds,
            0
        );
    }

    /// <summary>
    /// Returns a new TimeSpan equal to the input with truncated milliseconds
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public static TimeSpan TruncateSeconds(
        this TimeSpan timeSpan
    )
    {
        return new TimeSpan(
            timeSpan.Days,
            timeSpan.Hours,
            timeSpan.Minutes,
            0,
            0
        );
    }

    /// <summary>
    /// Returns a new TimeSpan equal to the input with truncated milliseconds
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public static TimeSpan TruncateMinutes(
        this TimeSpan timeSpan
    )
    {
        return new TimeSpan(
            timeSpan.Days,
            timeSpan.Hours,
            0,
            0,
            0
        );
    }

    /// <summary>
    /// Returns a new TimeSpan equal to the input with truncated milliseconds
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    public static TimeSpan TruncateHours(
        this TimeSpan timeSpan
    )
    {
        return new TimeSpan(
            timeSpan.Days,
            0,
            0,
            0,
            0
        );
    }
}