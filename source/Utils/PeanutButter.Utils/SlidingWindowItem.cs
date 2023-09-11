using System;

namespace
#if BUILD_PEANUTBUTTER_INTERNAL
    Imported.PeanutButter.Utils;
#else
    PeanutButter.Utils;
#endif
/// <summary>
/// Describes a sliding window item
/// </summary>
/// <typeparam name="T"></typeparam>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    interface ISlidingWindowItem<out T>
{
    /// <summary>
    /// The stored value
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// When this value was stored. Inserted values will
    /// have their Created time interpolated from surrounding items
    /// </summary>
    public DateTime Created { get; }
}

internal class SlidingWindowItem<T> : ISlidingWindowItem<T>
{
    public T Value { get; internal set; }
    public DateTime Created { get; internal set; }

    internal SlidingWindowItem(T value)
    {
        Created = DateTime.Now;
        Value = value;
    }
}