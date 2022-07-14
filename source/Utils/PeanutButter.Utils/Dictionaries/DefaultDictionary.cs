using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable InheritdocConsiderUsage
#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils.Dictionaries
#else
namespace PeanutButter.Utils.Dictionaries
#endif
{
    /// <summary>
    /// Provides a Dictionary class which returns default values for unknown keys
    /// (like Python's defaultdict)
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class DefaultDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly Func<TValue> _defaultResolver;
        private readonly Func<TKey, TValue> _smartResolver;
        private readonly Dictionary<TKey, TValue> _actual;

        /// <summary>
        /// Always report as case- and culture- insensitive
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public IEqualityComparer<TKey> Comparer { get; private set; }
        // typeof(TKey) == typeof(string)
        //         ? StringComparer.OrdinalIgnoreCase as IEqualityComparer<TKey>
        //         : new StrictComparer<TKey>();

        /// <summary>
        /// Constructs a DefaultDictionary where missing key lookups
        /// return the default value for TValue
        /// </summary>
        public DefaultDictionary() : this(() => default(TValue))
        {
        }

        /// <summary>
        /// Constructs a DefaultDictionary where missing key lookups
        /// return the value provided by the default resolver
        /// </summary>
        /// <param name="defaultResolver"></param>
        public DefaultDictionary(
            Func<TValue> defaultResolver
        ): this(defaultResolver, null as IEqualityComparer<TKey>)
        {
        }

        /// <summary>
        /// Creates the default dictionary with the provided default resolver and key comparer
        /// </summary>
        /// <param name="defaultResolver"></param>
        /// <param name="keyComparer"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DefaultDictionary(Func<TValue> defaultResolver, IEqualityComparer<TKey> keyComparer)
        {
            _defaultResolver = defaultResolver ?? throw new ArgumentNullException(nameof(defaultResolver));
            Comparer = ResolveEqualityComparer(keyComparer);
            _actual = new Dictionary<TKey, TValue>(Comparer);
        }

        private IEqualityComparer<TKey> ResolveEqualityComparer(IEqualityComparer<TKey> provided)
        {
            if (provided is not null)
            {
                return provided;
            }

            if (typeof(TKey) == typeof(string))
            {
                return (IEqualityComparer<TKey>) StringComparer.OrdinalIgnoreCase;
            }
            
            return EqualityComparer<TKey>.Default;
        }

        /// <summary>
        /// Constructs a DefaultDictionary where missing key lookups
        /// return the value provided by the default resolver
        /// </summary>
        /// <param name="smartResolver"></param>
        public DefaultDictionary(Func<TKey, TValue> smartResolver): this(smartResolver, null as IEqualityComparer<TKey>)
        {
        }

        private DefaultDictionary(Func<TKey, TValue> smartResolver, IEqualityComparer<TKey> keyComparer)
        {
            _smartResolver = smartResolver ?? throw new ArgumentNullException(nameof(smartResolver));
            Comparer = ResolveEqualityComparer(keyComparer);
            _actual = new Dictionary<TKey, TValue>();
        }

        /// <summary>
        /// Gets an enumerator for known items in the dictionary
        /// </summary>
        /// <returns>Enumerator of TKey/TValue</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _actual.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Adds an item to the dictionary
        /// </summary>
        /// <param name="item">KeyValuePair to add</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _actual.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Removes all known items from the dictionary
        /// -> NB: you'll still get default values!
        /// </summary>
        public void Clear()
        {
            _actual.Clear();
        }

        /// <summary>
        /// Searches for an item in the dictionary
        /// </summary>
        /// <param name="item">Item to search for</param>
        /// <returns>True if found, False otherwise</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return true;
        }

        /// <summary>
        /// Copies known items to the target array
        /// </summary>
        /// <param name="array">Array to copy to</param>
        /// <param name="arrayIndex">position to start copying at</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _actual.ForEach(kvp =>
            {
                array[arrayIndex++] = kvp;
            });
        }

        /// <summary>
        /// Removes an item from the dictionary
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <returns>True if found and removed, False otherwise</returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _actual.Remove(item.Key);
        }

        /// <summary>
        /// Gives the count of known items in the dictionary
        /// </summary>
        public int Count => _actual.Count;

        /// <summary>
        /// Always returns false - DefaultDictionaries are writable!
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Always returns true - if the key is "unknown" a default can still be provided
        /// </summary>
        /// <param name="key">Key to search for</param>
        /// <returns>True</returns>
        public bool ContainsKey(TKey key)
        {
            return true;
        }

        /// <summary>
        /// Adds an item by key, value to the dictionary
        /// </summary>
        /// <param name="key">Key to add</param>
        /// <param name="value">Value to add</param>
        public void Add(TKey key, TValue value)
        {
            _actual.Add(key, value);
        }

        /// <summary>
        /// Removes an item by key from the dictionary
        /// </summary>
        /// <param name="key">Key to remove</param>
        /// <returns>True if found and removed, False otherwise</returns>
        public bool Remove(TKey key)
        {
            return _actual.Remove(key);
        }

        /// <summary>
        /// Tries to get a known value for the key, falling back on the default value
        /// </summary>
        /// <param name="key">Key to look up</param>
        /// <param name="value">(out) parameter containing looked up (or default) value</param>
        /// <returns>Always true</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return _actual.TryGetValue(key, out value) || ResolveDefaultOnto(key, out value);
        }

        private bool ResolveDefaultOnto(TKey key, out TValue value)
        {
            value = Resolve(key);
            return true;
        }

        private TValue Resolve(TKey key)
        {
            return _defaultResolver == null
                ? _smartResolver(key)
                : _defaultResolver();
        }

        /// <summary>
        /// Index into the dictionary
        /// </summary>
        /// <param name="key">Key to look up</param>
        public TValue this[TKey key]
        {
            get => _actual.TryGetValue(key, out var result)
                ? result
                : Resolve(key);
            set => _actual[key] = value;
        }

        /// <summary>
        /// Returns a collection of all known keys
        /// </summary>
        public ICollection<TKey> Keys => _actual.Keys;

        /// <summary>
        /// Returns a collection of all known values
        /// </summary>
        public ICollection<TValue> Values => _actual.Values;
    }
}