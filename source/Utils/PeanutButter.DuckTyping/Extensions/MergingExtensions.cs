#if NETSTANDARD
#else
using System.Configuration;
#endif
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using Imported.PeanutButter.Utils;
using Imported.PeanutButter.Utils.Dictionaries;
using PeanutButter.DuckTyping.Shimming;

namespace PeanutButter.DuckTyping.Extensions
{
    /// <summary>
    /// Provides extension method for merging multipl objects behind a
    /// required interface
    /// </summary>
    // TODO: publicise when ready
    internal static class MergingExtensions
    {
        /// <summary>
        /// Tests if a collection of objects can back
        /// the given interface T
        /// </summary>
        /// <param name="sources"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool CanMergeAs<T>(this IEnumerable<object> sources)
        {
            return InternalCanMergeAs<T>(sources, false, false, out var _);
        }

        /// <summary>
        /// Tests if a collection of objects can back
        /// the given interface T (fuzzy)
        /// </summary>
        /// <param name="sources"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool CanFuzzyMergeAs<T>(this IEnumerable<object> sources)
        {
            return InternalCanMergeAs<T>(sources, true, false, out var _);
        }

        public static T MergeAs<T>(this IEnumerable<object> sources) where T : class
        {
            return sources.MergeAs<T>(false);
        }

        public static T MergeAs<T>(
            this IEnumerable<object> sources,
            bool throwOnError
        ) where T : class
        {
            return InternalCanMergeAs<T>(sources, false, throwOnError, out var merged)
                ? merged.DuckAs<T>()
                : null;
        }

        public static T FuzzyMergeAs<T>(
            this IEnumerable<object> sources
        ) where T : class
        {
            return sources.FuzzyMergeAs<T>(false);
        }

        public static T FuzzyMergeAs<T>(
            this IEnumerable<object> sources,
            bool throwOnError
        ) where T : class
        {
            return InternalCanMergeAs<T>(sources, true, throwOnError, out var merged)
                ? merged.FuzzyDuckAs<T>()
                : null;
        }

        private static bool InternalCanMergeAs<T>(
            this IEnumerable<object> sources,
            bool allowFuzzy,
            bool throwOnError,
            out MergeDictionary<string, object> merged
        )
        {
            merged = MergeObjects(sources, allowFuzzy);
            return merged.InternalCanDuckAs<T>(allowFuzzy, throwOnError);
        }

        private static MergeDictionary<string, object> MergeObjects(
            IEnumerable<object> sources,
            bool isFuzzy
        )
        {
            var dictionaries = sources.Where(o => o != null)
                .Aggregate(
                    new List<IDictionary<string, object>>(),
                    (converted, toConvert) =>
                    {
                        var asDict = MergeConversionStrategies.Aggregate(
                            null as IDictionary<string, object>,
                            (acc, cur) => acc ?? cur(toConvert, isFuzzy));
                        if (asDict != null)
                            converted.Add(asDict);
                        return converted;
                    })
                .ToArray();
            var merged = new MergeDictionary<string, object>(dictionaries);
            return merged;
        }

        private static readonly Func<object, bool, IDictionary<string, object>>[] MergeConversionStrategies =
        {
            PassThrough,
            BoxifyDictionary,
            WrapNameValueCollection,
#if NETSTANDARD
#else
            WrapConnectionStringCollection,
#endif
            WrapObject
        };

#if NETSTANDARD
#else
        private static IDictionary<string, object> WrapConnectionStringCollection(
            object obj, bool isFuzzy
        )
        {
            var asCollection = obj as ConnectionStringSettingsCollection;
            if (asCollection == null)
                return null;
            return new DictionaryWrappingConnectionStringSettingCollection(
                asCollection,
                isFuzzy
            );
        }
#endif

        private static IDictionary<string, object> WrapObject(
            object obj,
            bool isFuzzy
        )
        {
            return new DictionaryWrappingObject(obj,
                isFuzzy
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal
            );
        }

        private static IDictionary<string, object> WrapNameValueCollection(
            object arg,
            bool isFuzzy
        )
        {
            var asNameValueCollection = arg as NameValueCollection;
            return asNameValueCollection == null
                ? null
                : new DictionaryWrappingNameValueCollection(asNameValueCollection, isFuzzy);
        }

        private static IDictionary<string, object> BoxifyDictionary(object input, bool isFuzzy)
        {
            if (!input.GetType().TryGetDictionaryKeyAndValueTypes(out var keyType, out var valueType))
                return null;
            if (keyType != typeof(string))
                return null;
            var method = BoxDictionaryMethod.MakeGenericMethod(valueType);
            return method.Invoke(null, new[] {input, isFuzzy}) as IDictionary<string, object>;
        }

        private const BindingFlags PRIVATE_STATIC = BindingFlags.NonPublic | BindingFlags.Static;

        private static readonly MethodInfo BoxDictionaryMethod =
            typeof(MergingExtensions).GetMethod(nameof(BoxDictionary), PRIVATE_STATIC);

        private static IDictionary<string, object> BoxDictionary<TValue>(
            IDictionary<string, TValue> src,
            bool isFuzzy
        )
        {
            return src.ToDictionary(kvp => kvp.Key,
                kvp => kvp.Value as object,
                isFuzzy
                    ? StringComparer.OrdinalIgnoreCase
                    : StringComparer.Ordinal
            );
        }

        private static IDictionary<string, object> PassThrough(object input, bool isFuzzy)
        {
            var asDict = input as IDictionary<string, object>;
            if (!isFuzzy)
                return asDict;
            return asDict == null
                ? null
                : new CaseWarpingDictionaryWrapper<object>(asDict, StringComparer.OrdinalIgnoreCase);
        }
    }
}