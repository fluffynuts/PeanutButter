#if NETSTANDARD
#else
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils.Dictionaries
#else
namespace PeanutButter.Utils.Dictionaries
#endif
{
    /// <summary>
    /// Provides a convenient IDictionary wrapper for ConnectionStringCollections
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class DictionaryWrappingConnectionStringSettingCollection
        : DictionaryWrappingConnectionStringSettingCollection<string>
    {
        /// <inheritdoc />
        public DictionaryWrappingConnectionStringSettingCollection(
            ConnectionStringSettingsCollection connectionStringSettings,
            bool isCaseInsensitive) : base(connectionStringSettings, isCaseInsensitive)
        {
        }

        /// <inheritdoc />
        public DictionaryWrappingConnectionStringSettingCollection(
            ConnectionStringSettingsCollection connectionStringSettings
        ) : base(connectionStringSettings)
        {
        }

        /// <inheritdoc />
        public DictionaryWrappingConnectionStringSettingCollection(
            ConnectionStringSettingsCollection connectionStrings,
            StringComparer keyComparer
        ) : base(connectionStrings, keyComparer)
        {
        }
    }

    /// <summary>
    /// Provides a mechanism for wrapping read-write access to a ConnectionStringSettingsCollection
    /// in the IDictionary&lt;string, string&gt; interface to simplify shimming other types
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class DictionaryWrappingConnectionStringSettingCollection<TValue>
        : IDictionary<string, TValue>
        where TValue : class
    {
        private readonly ConnectionStringSettingsCollection _actual;

        /// <summary>
        /// Wrap the provided settings collection with or without case sensitivity
        /// </summary>
        /// <param name="connectionStringSettings"></param>
        /// <param name="isCaseInsensitive"></param>
        public DictionaryWrappingConnectionStringSettingCollection(
            ConnectionStringSettingsCollection connectionStringSettings,
            bool isCaseInsensitive
        ) : this(connectionStringSettings,
            isCaseInsensitive
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal)
        {
        }

        /// <summary>
        /// Wrap the provided connection string settings with case-sensitive,
        /// ordinal comparison for keys
        /// </summary>
        /// <param name="connectionStringSettings"></param>
        public DictionaryWrappingConnectionStringSettingCollection(
            ConnectionStringSettingsCollection connectionStringSettings
        ) : this(connectionStringSettings, StringComparer.Ordinal)
        {
        }

        /// <summary>
        /// Wrap the provided connection string settings with the provided
        /// keyComparer for set / retrieval
        /// </summary>
        /// <param name="connectionStrings"></param>
        /// <param name="keyComparer"></param>
        public DictionaryWrappingConnectionStringSettingCollection(
            ConnectionStringSettingsCollection connectionStrings,
            StringComparer keyComparer
        )
        {
            Comparer = keyComparer;
            _actual = connectionStrings;
        }


        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            return new DictionaryWrappingConnectionStringSettingCollectionEnumerator<TValue>(_actual);
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Clear()
        {
            _actual.Clear();
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            foreach (var item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, TValue> item)
        {
            if (!TryGetValue(item.Key, out var match))
                return false;
            return match == item.Value && Remove(item.Key);
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, TValue> item)
        {
            return TryGetValue(item.Key, out var match) &&
                match == item.Value;
        }

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return Keys.Contains(key);
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<string, TValue> item)
        {
            Add(item.Key, item.Value);
        }


        /// <inheritdoc />
        public void Add(string key, TValue value)
        {
            _actual.Add(new ConnectionStringSettings(key, value as string));
        }

        /// <inheritdoc />
        public bool Remove(string key)
        {
            if (!Keys.Contains(key))
                return false;
            _actual.Remove(key);
            return true;
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out TValue value)
        {
            if (!Keys.Contains(key))
            {
                value = null;
                return false;
            }

            value = _actual[key].ConnectionString as TValue;
            return true;
        }

        /// <inheritdoc />
        public TValue this[string key]
        {
            get => _actual[key].ConnectionString as TValue;
            set => _actual[key].ConnectionString = value as string;
        }

        /// <inheritdoc />
        public ICollection<string> Keys => GetKeys();

        private ICollection<string> GetKeys()
        {
            var result = new List<string>();
            for (var i = 0; i < _actual.Count; i++)
            {
                result.Add(_actual[i].Name);
            }

            return result.ToArray();
        }

        /// <inheritdoc />
        public int Count => _actual.Count;

        /// <inheritdoc />
        public ICollection<TValue> Values =>
            GetKeys()
                .Select(k => _actual[k].ConnectionString as TValue)
                .ToArray();

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <summary>
        /// Provides read-only access to the string comparer being used for key
        /// comparison
        /// </summary>
        public StringComparer Comparer { get; }
    }

    internal class DictionaryWrappingConnectionStringSettingCollectionEnumerator<TValue>
        : IEnumerator<KeyValuePair<string, TValue>>
        where TValue : class
    {
        private readonly ConnectionStringSettingsCollection _actual;
        private readonly string[] _keys;
        private int _currentIndex;

        public DictionaryWrappingConnectionStringSettingCollectionEnumerator(
            ConnectionStringSettingsCollection actual
        )
        {
            _actual = actual;
            _keys = KeysOf(actual).ToArray();
            Reset();
        }

        private IEnumerable<string> KeysOf(
            ConnectionStringSettingsCollection collection
        )
        {
            for (var i = 0; i < _actual.Count; i++)
                yield return collection[i].Name;
        }

        public void Dispose()
        {
            /* does nothing, on purpose */
        }

        public bool MoveNext()
        {
            return ++_currentIndex < _keys.Length;
        }

        public void Reset()
        {
            _currentIndex = -1;
        }

        public KeyValuePair<string, TValue> Current
            => new(
                _keys[_currentIndex], _actual[_keys[_currentIndex]].ConnectionString as TValue
            );

        object IEnumerator.Current => Current;
    }
}
#endif