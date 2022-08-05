using System;
using System.Collections.Generic;
using System.Linq;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.Extensions
#else
namespace PeanutButter.DuckTyping.Extensions
#endif
{
    internal static class DuckableTypesCache
    {
        private class DuckCacheItem
        {
            public Type FromType { get; }
            public Type ToType { get; }
            public bool Duckable { get; }

            public DuckCacheItem(Type fromType, Type toType, bool duckable)
            {
                ToType = toType;
                FromType = fromType;
                Duckable = duckable;
            }
        }

        private static readonly List<DuckCacheItem> Duckables = new List<DuckCacheItem>();
        private static readonly List<DuckCacheItem> FuzzyDuckables = new List<DuckCacheItem>();
        private static readonly object Lock = new object();

        internal static void CacheDuckable(
            Type from,
            Type to,
            bool result,
            bool fuzzy)
        {
            lock (Lock)
            {
                var cacheItem = new DuckCacheItem(from, to, result);
                Duckables.Add(cacheItem);
                if (fuzzy)
                    FuzzyDuckables.Add(cacheItem);
            }
        }

        internal static bool CanDuckAs<T>(Type toDuckFrom, bool fuzzy)
        {
            lock (Lock)
            {
                return HaveMatch(fuzzy
                    ? FuzzyDuckables
                    : Duckables, toDuckFrom, typeof(T));
            }
        }

        internal static bool CanDuckAs(Type toDuckFrom, Type toDuckTo, bool fuzzy)
        {
            lock (Lock)
            {
                return HaveMatch(fuzzy
                    ? FuzzyDuckables
                    : Duckables, toDuckFrom, toDuckTo);
            }
        }

        private static bool HaveMatch(
            IEnumerable<DuckCacheItem> cache,
            Type toDuckFrom,
            Type toDuckTo
        )
        {
            lock (Lock)
            {
                var match = cache.FirstOrDefault(
                    o => o.FromType == toDuckFrom &&
                        o.ToType == toDuckTo);
                return match?.Duckable ?? false;
            }
        }
    }
}