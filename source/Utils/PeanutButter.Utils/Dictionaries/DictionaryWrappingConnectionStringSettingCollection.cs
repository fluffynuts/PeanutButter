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
    /// Provides a mechanism for wrapping read-write access to a ConnectionStringSettingsCollection
    /// in the IDictionary&lt;string, object&gt; interface to simplify shimming other types
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class DictionaryWrappingConnectionStringSettingCollection : IDictionary<string, object>
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
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return new DictionaryWrappingConnectionStringSettingCollectionEnumerator(_actual);
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
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (var item in this)
                array[arrayIndex++] = item;
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, object> item)
        {
            if (!TryGetValue(item.Key, out var match))
                return false;
            return match == item.Value && Remove(item.Key);
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, object> item)
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
        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }


        /// <inheritdoc />
        public void Add(string key, object value)
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
        public bool TryGetValue(string key, out object value)
        {
            if (!Keys.Contains(key))
            {
                value = null;
                return false;
            }

            value = _actual[key].ConnectionString;
            return true;
        }

        /// <inheritdoc />
        public object this[string key]
        {
            get => _actual[key].ConnectionString;
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
        public ICollection<object> Values =>
            GetKeys()
                .Select(k => _actual[k].ConnectionString)
                .ToArray();

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <summary>
        /// Provides read-only access to the string comparer being used for key
        /// comparison
        /// </summary>
        public StringComparer Comparer { get; }
    }

    internal class DictionaryWrappingConnectionStringSettingCollectionEnumerator
        : IEnumerator<KeyValuePair<string, object>>
    {
        private readonly ConnectionStringSettingsCollection _actual;
        private readonly string[] _keys;
        private int _currentIndex;

        public DictionaryWrappingConnectionStringSettingCollectionEnumerator(ConnectionStringSettingsCollection actual)
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

        public KeyValuePair<string, object> Current
            => new KeyValuePair<string, object>(
                _keys[_currentIndex], _actual[_keys[_currentIndex]].ConnectionString
            );

        object IEnumerator.Current => Current;
    }
}
#endif