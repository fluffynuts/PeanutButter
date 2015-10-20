using System.Collections.Generic;
using PeanutButter.RandomGenerators;

namespace PeanutButter
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> input)
        {
            if (input == null)
                return null;
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
}
