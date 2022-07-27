using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides extensions to convert from non-generic IDictionary to a generic one
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class DictionaryExtensions
    {
        /// <summary>
        /// Convert the given IDictionary to IDictionary&lt;TKey, TValue&gt; with
        /// the provided key and value generator functions
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="keyGenerator"></param>
        /// <param name="valueGenerator"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
#if BUILD_PEANUTBUTTER_INTERNAL
        internal
#else
        public
#endif
            static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(
                this IDictionary dict,
                Func<DictionaryEntry, TKey> keyGenerator,
                Func<DictionaryEntry, TValue> valueGenerator
            )
        {
            var result = new Dictionary<TKey, TValue>();
            if (dict is null)
            {
                return result;
            }

            foreach (DictionaryEntry item in dict)
            {
                result[keyGenerator(item)] = valueGenerator(item);
            }

            return result;
        }

        /// <summary>
        /// Converts the given non-generic IDictionary to IDictionary&lt;TKey, TValue&gt;
        /// with hard casting of keys and values in the source to the provided
        /// types
        /// </summary>
        /// <param name="dict"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
#if BUILD_PEANUTBUTTER_INTERNAL
        internal
#else
        public
#endif
            static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(
                this IDictionary dict
            )
        {
            return dict.ToDictionary(o => (TKey) o.Key, o => (TValue) o.Value);
        }

        /// <summary>
        /// Find an item in or add an item to a dictionary
        /// - operation is thread-safe: dictionary is locked during search &amp; add
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static TValue FindOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            TKey key,
            TValue value
        )
        {
            return dict.FindOrAdd(key, () => value);
        }

        /// <summary>
        /// Find an item in or add an item to a dictionary
        /// - operation is thread-safe: dictionary is locked during search &amp; add
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="generator"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static TValue FindOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            TKey key,
            Func<TValue> generator
        )
        {
            if (dict is null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (generator is null)
            {
                throw new ArgumentNullException(nameof(generator));
            }

            if (dict is ConcurrentDictionary<TKey, TValue> concurrentDictionary)
            {
                TValue generated = default;
                var wasGenerated = false;
                concurrentDictionary.GetOrAdd(key, _ =>
                {
                    wasGenerated = true;
                    return generated = generator();
                });
                if (concurrentDictionary.TryGetValue(key, out var result))
                {
                    return result;
                }

                // item has been removed, but technically, we can give it back
                // if it was generated (or regen) - this is a threading scenario
                // in the host app - not our immediate problem
                return wasGenerated
                    ? generated
                    : generator();
            }

            lock (dict)
            {
                if (dict.TryGetValue(key, out var existing))
                {
                    return existing;
                }

                var generated = generator();
                dict.Add(key, generated);

                return generated;
            }
        }

        /// <summary>
        /// Clones a given dictionary - new collection, same items
        /// </summary>
        /// <param name="dict"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static IDictionary<TKey, TValue> Clone<TKey, TValue>(
            this IDictionary<TKey, TValue> dict
        )
        {
            return dict.ToDictionary(o => o.Key, o => o.Value);
        }

        /// <summary>
        /// Merge second dictionary into the first, producing a new dictionary output,
        /// with the second's values taking precedence over the first's
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static IDictionary<TKey, TValue> MergedWith<TKey, TValue>(
            this IDictionary<TKey, TValue> first,
            IDictionary<TKey, TValue> second
        )
        {
            return first.MergedWith(
                second,
                MergeWithPrecedence.PreferLastSeen
            );
        }

        /// <summary>
        /// Merge second dictionary into the first, producing a new dictionary output,
        /// with the provided merge-precedence
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="withPrecedence"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static IDictionary<TKey, TValue> MergedWith<TKey, TValue>(
            this IDictionary<TKey, TValue> first,
            IDictionary<TKey, TValue> second,
            MergeWithPrecedence withPrecedence
        )
        {
            var dictionaries = withPrecedence == MergeWithPrecedence.PreferFirstSeen
                ? new[] { second, first }
                : new[] { first, second };
            var result = new Dictionary<TKey, TValue>();
            foreach (var dict in dictionaries)
            {
                foreach (var kvp in dict)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }

        /// <summary>
        /// Merges the new data into the target, preferring to keep
        /// the original values in the target when also specified in
        /// the new data
        /// </summary>
        /// <param name="newData"></param>
        /// <param name="target"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        public static IDictionary<TKey, TValue> MergeInto<TKey, TValue>(
            this IDictionary<TKey, TValue> newData,
            IDictionary<TKey, TValue> target
        )
        {
            return newData.MergeInto(target, MergeIntoPrecedence.PreferTargetData);
        }

        /// <summary>
        /// Merges the new data into the target, with the
        /// specified merge preference
        /// </summary>
        /// <param name="newData"></param>
        /// <param name="target"></param>
        /// <param name="mergePrecedence"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        public static IDictionary<TKey, TValue> MergeInto<TKey, TValue>(
            this IDictionary<TKey, TValue> newData,
            IDictionary<TKey, TValue> target,
            MergeIntoPrecedence mergePrecedence
        )
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (newData is null)
            {
                return target; // nothing to add
            }

            return mergePrecedence == MergeIntoPrecedence.PreferNewData
                ? MergeWithOverwrite(newData, target)
                : MergeWithOriginalRetention(newData, target);
        }

        private static IDictionary<TKey, TValue> MergeWithOriginalRetention<TKey, TValue>(
            IDictionary<TKey, TValue> newData,
            IDictionary<TKey, TValue> target
        )
        {
            var toMerge = newData.Keys.Except(target.Keys);
            foreach (var key in toMerge)
            {
                target[key] = newData[key];
            }

            return target;
        }

        private static IDictionary<TKey, TValue> MergeWithOverwrite<TKey, TValue>(
            IDictionary<TKey, TValue> newData,
            IDictionary<TKey, TValue> target
        )
        {
            foreach (var kvp in newData)
            {
                target[kvp.Key] = kvp.Value;
            }

            return target;
        }

        /// <summary>
        /// Converts a NameValueCollection to a dictionary
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static IDictionary<string, string> ToDictionary(this NameValueCollection collection)
        {
            var result = new Dictionary<string, string>();
            foreach (var key in collection.AllKeys)
            {
                result[key] = collection[key];
            }
            return result;
        }

        /// <summary>
        /// Converts a NameValueCollection to a dictionary
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static NameValueCollection ToNameValueCollection(
            this IDictionary<string, string> dict)
        {
            var result = new NameValueCollection();
            foreach (var kvp in dict)
            {
                result[kvp.Key] = kvp.Value;
            }
            return result;
        }
    }

    /// <summary>
    /// Sets the precedence when merging data with the same keys
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        enum MergeWithPrecedence
    {
        /// <summary>
        /// Prefer the first value seen
        /// </summary>
        PreferFirstSeen,

        /// <summary>
        /// Prefer the last value seen
        /// </summary>
        PreferLastSeen
    }

    /// <summary>
    /// Sets the precedence when merging new data into an existing dictionary
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        enum MergeIntoPrecedence
    {
        /// <summary>
        /// Prefer target data over new data, ie
        /// discard the new value if already found in the target
        /// </summary>
        PreferTargetData,

        /// <summary>
        /// Prefer to new data over existing target data, ie
        /// overwrite the target value if found
        /// </summary>
        PreferNewData
    }
}