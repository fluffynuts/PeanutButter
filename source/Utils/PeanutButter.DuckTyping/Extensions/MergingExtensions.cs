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
            var dictionaries = sources.Where(o => o != null)
                .Aggregate(
                    new List<IDictionary<string, object>>(),
                    (converted, toConvert) =>
                    {
                        var asDict = _mergeConversionStrategies.Aggregate(
                            null as IDictionary<string, object>,
                            (acc, cur) => acc ?? cur(toConvert, true));
                        if (asDict != null)
                            converted.Add(asDict);
                        return converted;
                    })
                .ToArray();
            var merged = new MergeDictionary<string, object>(dictionaries);

            return merged.InternalCanDuckAs<T>(false, false);
        }

        private static readonly Func<object, bool, IDictionary<string, object>>[] _mergeConversionStrategies =
        {
            PassThrough,
            BoxifyDictionary,
            WrapNameValueCollection,
            WrapObject
        };

        private static IDictionary<string, object> WrapObject(object arg1, bool arg2)
        {
            return new DictionaryWrappingObject(arg1);
        }

        private static IDictionary<string, object> WrapNameValueCollection(object arg, bool caseSensitive)
        {
            var asNameValueCollection = arg as NameValueCollection;
            return asNameValueCollection == null
                ? null
                : new DictionaryWrappingNameValueCollection(asNameValueCollection, caseSensitive);
        }

        private static IDictionary<string, object> BoxifyDictionary(object input, bool caseSensitive)
        {
            if (!input.GetType().TryGetDictionaryKeyAndValueTypes(out var keyType, out var valueType))
                return null;
            if (keyType != typeof(string))
                return null;
            var method = _boxDictionaryMethod.MakeGenericMethod(valueType);
            return method.Invoke(null, new[] {input, caseSensitive}) as IDictionary<string, object>;
        }

        private static readonly BindingFlags _privateStatic = BindingFlags.NonPublic | BindingFlags.Static;

        private static readonly MethodInfo _boxDictionaryMethod =
            typeof(MergingExtensions).GetMethod(nameof(BoxDictionary), _privateStatic);

        private static IDictionary<string, object> BoxDictionary<TValue>(
            IDictionary<string, TValue> src,
            bool caseSensitive
        )
        {
            // TODO: two-way passthrough with dictionary wrapper instead of conversion
            return src.ToDictionary(kvp => kvp.Key,
                kvp => kvp.Value as object,
                caseSensitive
                    ? StringComparer.InvariantCulture
                    : StringComparer.InvariantCultureIgnoreCase
            );
        }

        private static IDictionary<string, object> PassThrough(object input, bool caseSensitive)
        {
            // TODO: handle case-insensitivity
            return input as IDictionary<string, object>;
        }
    }
}