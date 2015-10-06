using System;
using System.Collections.Generic;
using System.Linq;

namespace PeanutButter.Utils
{
    public static class ExtensionsForIEnumerables
    {
        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> toRun)
        {
            foreach (var item in collection)
                toRun(item);
        }

        public static bool IsSameAs<T>(this IEnumerable<T> collection, IEnumerable<T> otherCollection)
        {
            if (collection == null && otherCollection == null) return true;
            if (collection == null || otherCollection == null) return false;
            var source = collection.ToArray();
            var target = otherCollection.ToArray();
            if (source.Count() != target.Count()) return false;
            return source.Aggregate(true, (state, item) => state && target.Contains(item));
        }

        public static string JoinWith<T>(this IEnumerable<T> collection, string joinWith)
        {
            var stringArray = collection as string[];
            if (stringArray == null)
            {
                if (typeof(T) == typeof(string))
                    stringArray = collection.ToArray() as string[];
                else
                    stringArray = collection.Select(i => i.ToString()).ToArray();
            }
            return string.Join(joinWith, stringArray);
        }

        public static bool IsEmpty<T>(this IEnumerable<T> collection)
        {
            if (collection == null) return true;
            return !collection.Any();
        }

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> collection)
        {
            return collection ?? new List<T>();
        }

        public static IEnumerable<T> GetShuffledCopyOf<T>(this IEnumerable<T> input)
        {
            var rnd = new Random(DateTime.Now.Millisecond);
            var copy = new List<T>(input);
            var result = new List<T>();
            while (copy.Count > 0)
            {
                var next = rnd.Next(copy.Count - 1);
                var nextValue = copy[next];
                copy.RemoveAt(next);
                result.Add(nextValue);
            }
            return result;
        }

    }
}
