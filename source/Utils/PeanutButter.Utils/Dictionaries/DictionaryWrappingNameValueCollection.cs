using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils.Dictionaries
#else
namespace PeanutButter.Utils.Dictionaries
#endif
{
    /// <summary>
    /// Wraps a NameValueCollection in an IDictionary interface
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class DictionaryWrappingNameValueCollection : IDictionary<string, object>
    {
        private readonly NameValueCollection _data;
        /// <summary>
        /// Comparer to use for the keys of this dictionary
        /// </summary>
        public StringComparer Comparer { get; }

        /// <summary>
        /// Construct this dictionary with a NameValueCollection to wrap,
        /// specifying whether or not key lookups are to be case-sensitive
        /// </summary>
        /// <param name="data"></param>
        /// <param name="caseInsensitive">Flag: is this collection to treat keys case-insensitive?</param>
        public DictionaryWrappingNameValueCollection(
            NameValueCollection data,
            bool caseInsensitive
        ) : this(data, caseInsensitive
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal)
        {
        }

        /// <summary>
        /// Construct this dictionary with a NameValueCollection to wrap
        /// </summary>
        /// <param name="data"></param>
        public DictionaryWrappingNameValueCollection(
            NameValueCollection data
        ) : this(data, StringComparer.Ordinal)
        {
        }

        /// <summary>
        /// Construct this dictionary with a NameValueCollection to wrap,
        /// specifying the StringComparer to use when comparing requested
        /// keys with available keys
        /// </summary>
        /// <param name="data"></param>
        /// <param name="comparer"></param>
        public DictionaryWrappingNameValueCollection(
            NameValueCollection data,
            StringComparer comparer
        )
        {
            _data = data;
            Comparer = comparer;
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return new DictionaryWrappingNameValueCollectionEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<string, object> item)
        {
            _data.Add(item.Key, item.Value as string ?? item.Value?.ToString());
        }

        /// <inheritdoc />
        public void Clear()
        {
            _data.Clear();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, object> item)
        {
            var key = GetKeyFor(item.Key);
            if (key == null || !_data.AllKeys.Contains(key))
                return false;
            return _data[key] == item.Value?.ToString();
        }

        private string GetKeyFor(string key)
        {
            return _data.AllKeys.FirstOrDefault(k => KeysMatch(k, key));
        }

        private bool KeysMatch(string one, string other)
        {
            return Comparer.Equals(one, other);
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (var kvp in this)
            {
                array[arrayIndex++] = kvp;
            }
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, object> item)
        {
            if (!Contains(item))
                return false;
            var key = GetKeyFor(item.Key);
            _data.Remove(key);
            return true;
        }

        /// <inheritdoc />
        public int Count => _data.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return GetKeyFor(key) != null;
        }

        /// <inheritdoc />
        public void Add(string key, object value)
        {
            _data.Add(key, value as string ?? value?.ToString());
        }

        /// <inheritdoc />
        public bool Remove(string key)
        {
            var result = _data.AllKeys.Contains(key);
            if (result)
                _data.Remove(key);
            return result;
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out object value)
        {
            key = GetKeyFor(key);
            if (key == null)
            {
                value = null;
                return false;
            }

            value = _data[key];
            return true;
        }

        /// <inheritdoc />
        public object this[string key]
        {
            get
            {
                key = GetKeyFor(key);
                return _data[key];
            }
            set
            {
                key = GetKeyFor(key) ?? key; // allow adding items
                _data[key] = value?.ToString(); // TODO: could be better
            }
        }

        /// <inheritdoc />
        public ICollection<string> Keys => _data.AllKeys;

        /// <inheritdoc />
        public ICollection<object> Values => _data.AllKeys.Select(k => _data[k]).ToArray();
    }
    
    // TODO: add explicit tests around this class, which is currently only tested by indirection
    /// <summary>
    /// Wraps a NameValueCollection in a Dictionary interface
    /// </summary>
    internal class DictionaryWrappingNameValueCollectionEnumerator : IEnumerator<KeyValuePair<string, object>>
    {
        internal DictionaryWrappingNameValueCollection Data => _data;
        private readonly DictionaryWrappingNameValueCollection _data;
        private string[] _keys;
        private int _current;

        /// <summary>
        /// Provides an IEnumerator for a DictionaryWrappingNameValueCollection
        /// </summary>
        /// <param name="data"></param>
        public DictionaryWrappingNameValueCollectionEnumerator(
            DictionaryWrappingNameValueCollection data
        )
        {
            _data = data;
            Reset();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            /* empty on purpose, just need to implement IEnumerator */
        }

        /// <inheritdoc />
        public bool MoveNext()
        {
            RefreshKeys();
            _current++;
            return _current < _keys.Length;
        }

        /// <inheritdoc />
        public void Reset()
        {
            _current = -1;
            _keys = new string[] { };
        }

        public KeyValuePair<string, object> Current
        {
            get
            {
                RefreshKeys();
                if (_current >= _keys.Length)
                    throw new InvalidOperationException("Current index is out of bounds");
                return new KeyValuePair<string, object>(_keys[_current], _data[_keys[_current]]);
            }
        }

        private void RefreshKeys()
        {
            if (!_keys.Any())
                _keys = _data.Keys.ToArray();
        }

        object IEnumerator.Current => Current;
    }
    
}