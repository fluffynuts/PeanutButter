using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable InheritdocConsiderUsage
#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils.Dictionaries
#else
namespace PeanutButter.Utils.Dictionaries
#endif
{
    /// <summary>
    /// Flags which may be set when constructing a DefaultDictionary
    /// to control operations
    /// </summary>
    [Flags]
    public enum DefaultDictionaryFlags
    {
        /// <summary>
        /// No options set
        /// </summary>
        None = 0,

        /// <summary>
        /// When set, default values which are resolved via the
        /// resolver function will be stored for future use in the
        /// internal data store - they will not be regenerated on
        /// each successive request
        /// </summary>
        CacheResolvedDefaults,

        /// <summary>
        /// When set, ContainsKey will return false for keys which
        /// are actually missing from the internal data store, even
        /// though reading via the indexer will return the calculated
        /// default value
        /// </summary>
        ReportMissingKeys
    }

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
        class DefaultDictionary<TKey, TValue>
        : IDictionary<TKey, TValue>,
          IDictionary
    {
        private readonly Func<TValue> _defaultResolver;
        private readonly bool _storeResolvedDefaults;
        private readonly HashSet<TKey> _generatedKeys = new();
        private readonly Func<TKey, TValue> _smartResolver;
        private readonly Dictionary<TKey, TValue> _actual;
        private readonly bool _reportMissingKeys;

        /// <summary>
        /// Always report as case- and culture- insensitive
        /// </summary>
        public IEqualityComparer<TKey> Comparer { get; }

        /// <summary>
        /// Create the default dictionary with the default resolver
        /// (returning default(TValue)) and the provided flags
        /// </summary>
        /// <param name="flags"></param>
        public DefaultDictionary(
            DefaultDictionaryFlags flags
        ) : this(() => default, flags)
        {
        }

        /// <summary>
        /// Constructs a DefaultDictionary where missing key lookups
        /// return the default value for TValue
        /// </summary>
        public DefaultDictionary() : this(() => default)
        {
        }

        /// <summary>
        /// Constructs a DefaultDictionary where missing key lookups
        /// return the value provided by the default resolver
        /// </summary>
        /// <param name="defaultResolver"></param>
        public DefaultDictionary(
            Func<TValue> defaultResolver
        ) : this(defaultResolver, keyComparer: null)
        {
        }

        /// <summary>
        /// Creates the default dictionary with the provided default resolver,
        /// optionally storing resolved defaults for later re-use
        /// </summary>
        /// <param name="defaultResolver"></param>
        /// <param name="flags"></param>
        public DefaultDictionary(
            Func<TValue> defaultResolver,
            DefaultDictionaryFlags flags
        ) : this(defaultResolver, null, flags)
        {
        }

        /// <summary>
        /// Creates the default dictionary with the provided default resolver and key comparer
        /// </summary>
        /// <param name="defaultResolver"></param>
        /// <param name="keyComparer"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DefaultDictionary(
            Func<TValue> defaultResolver,
            IEqualityComparer<TKey> keyComparer
        ) : this(defaultResolver, keyComparer, DefaultDictionaryFlags.None)
        {
        }

        /// <summary>
        /// Creates the default dictionary with the provided resolver and key comparer,
        /// choosing to store (and re-serve) resolved defaults when storeResolvedDefaults
        /// is true
        /// </summary>
        /// <param name="defaultResolver"></param>
        /// <param name="keyComparer"></param>
        /// <param name="flags"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public DefaultDictionary(
            Func<TValue> defaultResolver,
            IEqualityComparer<TKey> keyComparer,
            DefaultDictionaryFlags flags
        )
        {
            _defaultResolver = defaultResolver ?? throw new ArgumentNullException(nameof(defaultResolver));
            _storeResolvedDefaults = flags.HasFlag(DefaultDictionaryFlags.CacheResolvedDefaults);
            _reportMissingKeys = flags.HasFlag(DefaultDictionaryFlags.ReportMissingKeys);
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
        public DefaultDictionary(Func<TKey, TValue> smartResolver)
            : this(smartResolver, null)
        {
        }

        private DefaultDictionary(Func<TKey, TValue> smartResolver, IEqualityComparer<TKey> keyComparer)
        {
            _smartResolver = smartResolver ?? throw new ArgumentNullException(nameof(smartResolver));
            Comparer = ResolveEqualityComparer(keyComparer);
            _actual = new Dictionary<TKey, TValue>(Comparer);
        }

        /// <summary>
        /// Gets an enumerator for known items in the dictionary
        /// </summary>
        /// <returns>Enumerator of TKey/TValue</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _actual.GetEnumerator();
        }

        /// <inheritdoc />
        public void Remove(object key)
        {
            if (key is TKey k)
            {
                _actual.Remove(k);
            }
        }

        /// <inheritdoc />
        public object this[object key]
        {
            get => TryGetWithKey(key);
            set => TrySetWithKey(key, value);
        }

        private void TrySetWithKey(object key, object value)
        {
            if (key is not TKey k)
            {
                throw new InvalidOperationException(
                    $"Key {key} is of invalid type for this store"
                );
            }

            if (value is not TValue v)
            {
                throw new InvalidOperationException(
                    $"Value {value} is of invalid type for this store"
                );
            }

            this[k] = v;
        }

        private object TryGetWithKey(object key)
        {
            if (key is TKey k)
            {
                return this[k];
            }

            throw new KeyNotFoundException($"Not found (invalid key type): {key}");
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

        /// <inheritdoc />
        public bool Contains(object key)
        {
            return key is TKey k &&
                ContainsKey(k);
        }

        /// <inheritdoc />
        public void Add(object key, object value)
        {
            if (key is not TKey k)
            {
                throw new InvalidOperationException(
                    $"Cannot add key '{key}': invalid type"
                );
            }

            if (value is not TValue v)
            {
                throw new InvalidOperationException(
                    $"Cannot add value '{value}': invalid type"
                );
            }

            _actual.Add(k, v);
        }

        /// <summary>
        /// Removes all known items from the dictionary
        /// -> NB: you'll still get default values!
        /// </summary>
        public void Clear()
        {
            _actual.Clear();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            throw new NotImplementedException();
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

        /// <inheritdoc />
        public void CopyTo(Array array, int index)
        {
            foreach (var item in _actual)
            {
                array.SetValue(item, index++);
            }
        }

        /// <summary>
        /// Gives the count of known items in the dictionary
        /// </summary>
        public int Count => _actual.Count;

        /// <inheritdoc />
        public object SyncRoot { get; } = new();

        /// <inheritdoc />
        public bool IsSynchronized => false;

        // ReSharper disable once ConvertToAutoProperty
        ICollection IDictionary.Values => _actual.Values;

        /// <summary>
        /// Always returns false - DefaultDictionaries are writable!
        /// </summary>
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public bool IsFixedSize => false;

        /// <summary>
        /// Always returns true - if the key is "unknown" a default can still be provided
        /// </summary>
        /// <param name="key">Key to search for</param>
        /// <returns>True</returns>
        public bool ContainsKey(TKey key)
        {
            if (_reportMissingKeys)
            {
                return !_generatedKeys.Contains(key) && 
                    _actual.ContainsKey(key);
            }

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
            return StoreIfNecessary(
                key,
                _defaultResolver == null
                    ? _smartResolver(key)
                    : _defaultResolver()
            );
        }

        private TValue StoreIfNecessary(
            TKey key,
            TValue resolved
        )
        {
            return _storeResolvedDefaults
                ? Store(key, resolved)
                : resolved;
        }

        private TValue Store(TKey key, TValue resolved)
        {
            _generatedKeys.Add(key);
            _actual[key] = resolved;
            return resolved;
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
            set 
            { 
                _generatedKeys.Remove(key);
                _actual[key] = value; 
            }
        }

        /// <summary>
        /// Returns a collection of all known keys
        /// </summary>
        public ICollection<TKey> Keys => _actual.Keys;

        ICollection IDictionary.Keys => _actual.Keys;

        /// <summary>
        /// Returns a collection of all known values
        /// </summary>
        public ICollection<TValue> Values => _actual.Values;
    }
}