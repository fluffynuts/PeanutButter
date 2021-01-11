using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable UnusedMember.Global

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils.Dictionaries
#else
namespace PeanutButter.Utils.Dictionaries
#endif
{
    /// <summary>
    /// Provides a read/write-through shim for another dictionary with
    /// the desired case sensitivity access
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class CaseWarpingDictionaryWrapper<TValue> : IDictionary<string, TValue>
    {
        /// <inheritdoc />
        public ICollection<string> Keys => _actual.Keys;
        /// <inheritdoc />
        public ICollection<TValue> Values => _actual.Values;

        private readonly IDictionary<string, TValue> _actual;
        private Dictionary<string, string> _keyLookup;

        /// <summary>
        /// Provides read-only access to the StringComparer used to match keys
        /// </summary>
        public StringComparer Comparer { get; }

        /// <summary>
        /// Constructs a CaseWarpingDictionaryWrapper around the provided
        /// actual dictionary with no case-warping
        /// </summary>
        /// <param name="actual"></param>
        public CaseWarpingDictionaryWrapper(
            IDictionary<string, TValue> actual
        ) : this(actual, StringComparer.Ordinal)
        {
        }

        /// <summary>
        /// Constructs a CaseWarpingDictionaryWrapper around the provided
        /// actual dictionary with specified case sensitivity
        /// </summary>
        /// <param name="actual"></param>
        /// <param name="caseInsensitive"></param>
        public CaseWarpingDictionaryWrapper(
            IDictionary<string, TValue> actual,
            bool caseInsensitive
        ) : this(actual, caseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
        {
        }

        /// <summary>
        /// Constructs a CaseWarpingDictionaryWrapper around the provided
        /// actual dictionary with the provided StringComparer used to look
        /// up keys
        /// </summary>
        /// <param name="actual"></param>
        /// <param name="keyComparer"></param>
        public CaseWarpingDictionaryWrapper(
            IDictionary<string, TValue> actual,
            StringComparer keyComparer
        )
        {
            _actual = actual;
            Comparer = keyComparer;
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            return _actual.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<string, TValue> item)
        {
            _actual.Add(item);
        }

        /// <inheritdoc />
        public void Add(string key, TValue value)
        {
            _actual.Add(key, value);
        }


        /// <inheritdoc />
        public void Clear()
        {
            _actual.Clear();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, TValue> item)
        {
            return TryGetValue(item.Key, out var value) &&
                   value.Equals(item.Value);
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            _actual.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, TValue> item)
        {
            return TryGetKey(item.Key, out var match) &&
                _actual.Remove(new KeyValuePair<string, TValue>(match, item.Value));
        }

        /// <inheritdoc />
        public int Count => _actual.Count;

        /// <inheritdoc />
        public bool IsReadOnly => _actual.IsReadOnly;

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            return TryGetKey(key, out var _);
        }

        /// <inheritdoc />
        public bool Remove(string key)
        {
            return TryGetKey(key, out var match) &&
                _actual.Remove(match);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out TValue value)
        {
            value = default(TValue);
            if (!TryGetKey(key, out var match))
                return false;
            value = _actual[match];
            return true;
        }

        /// <inheritdoc />
        public TValue this[string key]
        {
            get => GetValueAt(key);
            set => SetValueAt(key, value);
        }

        private bool TryGetKey(string search, out string actual)
        {
            CacheKeys();
            if (_keyLookup.TryGetValue(search, out actual))
            {
                return true;
            }

            if (_actual.ContainsKey(search))
            {
                // special case dictionaries, eg the default value dict,
                // will be ok with this
                actual = search;
                return true;
            }
            return false;
        }

        private void SetValueAt(string key, TValue value)
        {
            if (TryGetKey(key, out var match))
            {
                _actual[match] = value;
            }
            else
            {
                _actual[key] = value;
            }
        }

        private TValue GetValueAt(string key)
        {
            if (!TryGetKey(key, out var match))
            {
                throw new KeyNotFoundException(key);
            }

            return _actual[match];
        }

        private void CacheKeys()
        {
            if (_keyLookup != null)
            {
                return;
            }

            _keyLookup = new Dictionary<string, string>(Comparer);
            _actual.ForEach(kvp => _keyLookup.Add(kvp.Key, kvp.Key));
        }
    }
}