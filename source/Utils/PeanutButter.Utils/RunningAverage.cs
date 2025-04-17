using System.Linq;

namespace PeanutButter.Utils;

/// <summary>
/// Provides a convenience class to keep
/// a running average of values without
/// storing the values in memory
/// </summary>
public class RunningAverage
{
    /// <summary>
    /// Provides the current average
    /// </summary>
    public decimal Average { get; private set; }

    /// <summary>
    /// Provides a count of items that have been added
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Add a value to the pool
    /// - recalculates the average, discards the value
    /// </summary>
    /// <param name="value"></param>
    public void Add(params decimal[] value)
    {
        var total = (Count * Average) + value.Sum();
        Count+= value.Length;
        Average = total/Count;
    }
}