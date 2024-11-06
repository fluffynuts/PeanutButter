using System.Collections.Generic;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.RandomGenerators;
#else
namespace PeanutButter.RandomGenerators;
#endif

/// <summary>
/// Provides extensions for generic IEnumerable collections
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Takes an input collection and returns a new collection which is the
    /// input collection in random order
    /// </summary>
    /// <param name="input">Collection to randomize</param>
    /// <typeparam name="T">Item type of the collection</typeparam>
    /// <returns>A new collection with the same items as the original, but in random order</returns>
    public static IEnumerable<T> Randomize<T>(this IEnumerable<T> input)
    {
        if (input == null)
        {
            return null;
        }

        var src = new List<T>(input);
        var result = new List<T>();
        while (src.Count > 0)
        {
            var pick = RandomValueGen.GetRandomInt(0, src.Count - 1);
            var pickItem = src[pick];
            src.RemoveAt(pick);
            result.Add(pickItem);
        }

        return result;
    }

    /// <summary>
    /// Takes an input collection and returns a new collection which is the
    /// input collection in random order
    /// </summary>
    /// <param name="input">Collection to randomize</param>
    /// <typeparam name="T">Item type of the collection</typeparam>
    /// <returns>A new collection with the same items as the original, but in random order</returns>
    public static IList<T> Randomize<T>(
        this IList<T> input
    )
    {
        if (input == null)
        {
            return null;
        }

        var src = new List<T>(input);
        var result = new List<T>();
        while (src.Count > 0)
        {
            var pick = RandomValueGen.GetRandomInt(0, src.Count - 1);
            var pickItem = src[pick];
            src.RemoveAt(pick);
            result.Add(pickItem);
        }

        return result;
    }
}