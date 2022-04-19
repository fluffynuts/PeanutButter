using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

// TODO: magic with implicit operators between TValue and string
// -> currently, these collections are only used to house [string,string] and
//      [string,object] where, really, "object" means "string cast to object"

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils.Dictionaries
#else
namespace PeanutButter.Utils.Dictionaries
#endif
{
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class DictionaryWrappingNameValueCollection
        : DictionaryWrappingNameValueCollection<string>
    {
        public DictionaryWrappingNameValueCollection(
            NameValueCollection data,
            bool caseInsensitive
        ) : base(data, caseInsensitive)
        {
        }

        public DictionaryWrappingNameValueCollection(
            NameValueCollection data
        ) : base(data)
        {
        }

        public DictionaryWrappingNameValueCollection(
            NameValueCollection data,
            StringComparer comparer
        ) : base(data, comparer)
        {
        }
    }

    /// <summary>
    /// Wraps a NameValueCollection in an IDictionary interface
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class DictionaryWrappingNameValueCollection<TValue>
        : IDictionary<string, TValue>
        where TValue : class
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
        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            return new DictionaryWrappingNameValueCollectionEnumerator<TValue>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<string, TValue> item)
        {
            _data.Add(item.Key, item.Value as string);
        }

        /// <inheritdoc />
        public void Clear()
        {
            _data.Clear();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, TValue> item)
        {
            var key = GetKeyFor(item.Key);
            if (key == null || !_data.AllKeys.Contains(key))
            {
                return false;
            }

            return _data[key] == item.Value as string;
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
        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            foreach (var kvp in this)
            {
                array[arrayIndex++] = kvp;
            }
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, TValue> item)
        {
            if (!Contains(item))
            {
                return false;
            }

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
        public void Add(string key, TValue value)
        {
            _data.Add(key, value as string);
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
        public bool TryGetValue(string key, out TValue value)
        {
            key = GetKeyFor(key);
            if (key is null)
            {
                value = null;
                return false;
            }

            value = _data[key] as TValue;
            return true;
        }

        /// <inheritdoc />
        public TValue this[string key]
        {
            get
            {
                key = GetKeyFor(key);
                return _data[key] as TValue;
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
        public ICollection<TValue> Values => _data.AllKeys.Select(k => _data[k] as TValue).ToArray();
    }

    // TODO: add explicit tests around this class, which is currently only tested by indirection
    /// <summary>
    /// Wraps a NameValueCollection in a Dictionary interface
    /// </summary>
    internal class DictionaryWrappingNameValueCollectionEnumerator<TValue>
        : IEnumerator<KeyValuePair<string, TValue>>
        where TValue : class
    {
        internal DictionaryWrappingNameValueCollection<TValue> Data => _data;
        private readonly DictionaryWrappingNameValueCollection<TValue> _data;
        private string[] _keys;
        private int _current;

        /// <summary>
        /// Provides an IEnumerator for a DictionaryWrappingNameValueCollection
        /// </summary>
        /// <param name="data"></param>
        public DictionaryWrappingNameValueCollectionEnumerator(
            DictionaryWrappingNameValueCollection<TValue> data
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

        public KeyValuePair<string, TValue> Current
        {
            get
            {
                RefreshKeys();
                if (_current >= _keys.Length)
                    throw new InvalidOperationException("Current index is out of bounds");
                return new KeyValuePair<string, TValue>(
                    _keys[_current],
                    _data[_keys[_current]]
                );
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