using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
                MergePrecedence.Last
            );
        }

        /// <summary>
        /// Merge second dictionary into the first, producing a new dictionary output,
        /// with the provided merge-precedence
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="precedence"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static IDictionary<TKey, TValue> MergedWith<TKey, TValue>(
            this IDictionary<TKey, TValue> first,
            IDictionary<TKey, TValue> second,
            MergePrecedence precedence
        )
        {
            var dictionaries = precedence == MergePrecedence.First
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
    }

#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    /// <summary>
    /// Sets the precedence when merging data with the same keys
    /// </summary>
    public
#endif
        enum MergePrecedence
    {
        /// <summary>
        /// Prefer the first value seen
        /// </summary>
        First,

        /// <summary>
        /// Prefer the last value seen
        /// </summary>
        Last
    }
}