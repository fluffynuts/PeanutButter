using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils.Dictionaries
#else
namespace PeanutButter.Utils.Dictionaries
#endif
{
    /// <summary>
    /// Provides a wrapping read-write layer around another dictionary effectively
    ///     allowing transparent rename of the keys
    /// </summary>
    /// <typeparam name="TKey">Type of keys</typeparam>
    /// <typeparam name="TValue">Type of values stored</typeparam>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class ExpiringDictionary<TKey, TValue>
        : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _actual;
        private readonly ISlidingWindow<TKey> _keys;

        /// <summary>
        /// Provides a dictionary which expires elements
        /// based on their age
        /// </summary>
        /// <param name="ttl"></param>
        /// <exception cref="ArgumentException"></exception>
        public ExpiringDictionary(
            TimeSpan ttl
        )
        {
            if (ttl.TotalSeconds < 0)
            {
                throw new ArgumentException(
                    "ttl must be a positive timespan",
                    nameof(ttl)
                );
            }

            _actual = new Dictionary<TKey, TValue>();
            _keys = new SlidingWindow<TKey>(ttl);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            IDictionary<TKey, TValue> snapshot;
            lock (_actual)
            {
                Trim();
                snapshot = _actual.Clone();
            }

            return snapshot.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Store(item.Key, item.Value);
        }

        /// <inheritdoc />
        public void Clear()
        {
            lock (_actual)
            {
                _actual.Clear();
            }
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (!_keys.Contains(item.Key))
            {
                return false;
            }

            lock (_actual)
            {
                // may have expired between the key check and now
                return _keys.Contains(item.Key) &&
                    _actual.TryGetValue(item.Key, out var stored) &&
                    AreEqual(stored, item.Value);
            }
        }

        private static bool AreEqual(
            object left,
            object right
        )
        {
            if (left is null && right is null)
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            lock (_actual)
            {
                Trim();
                _actual.CopyTo(array, arrayIndex);
            }
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            lock (_actual)
            {
                if (!_actual.Remove(item))
                {
                    return false;
                }

                _keys.Remove(item.Key);
                return true;
            }
        }

        /// <inheritdoc />
        public int Count => _keys.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public void Add(TKey key, TValue value)
        {
            lock (_actual)
            {
                _actual.Add(key, value);
            }

            _keys.Add(key);
        }

        /// <inheritdoc />
        public bool ContainsKey(TKey key)
        {
            return _keys.Contains(key);
        }

        /// <inheritdoc />
        public bool Remove(TKey key)
        {
            return _keys.Remove(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (!_keys.Contains(key))
            {
                value = default;
                return false;
            }

            lock (_actual)
            {
                return _actual.TryGetValue(key, out value);
            }
        }

        /// <inheritdoc />
        public TValue this[TKey key]
        {
            get
            {
                return TryGetValue(key, out var result)
                    ? result
                    : throw new KeyNotFoundException(
                        $"""
                         The given key '{key}' was not present in the dictionary
                         """
                    );
            }

            set => Store(key, value);
        }

        private void Store(TKey key, TValue value)
        {
            lock (_actual)
            {
                _actual[key] = value;
                TrimIfNecessary();
            }

            _keys.Add(key);
        }

        private void TrimIfNecessary()
        {
            lock (_actual)
            {
                if (_keys.Count - _actual.Count < 10)
                {
                    return;
                }

                Trim();
            }
        }

        private void Trim()
        {
            lock (_actual)
            {
                var validKeys = _keys.AsHashSet();
                var items = _actual.ToArray();
                foreach (var kvp in items)
                {
                    if (!validKeys.Contains(kvp.Key))
                    {
                        _actual.Remove(kvp.Key);
                    }
                }
            }
        }

        /// <inheritdoc />
        public ICollection<TKey> Keys => _keys.ToArray();

        /// <inheritdoc />
        public ICollection<TValue> Values
        {
            get
            {
                lock (_actual)
                {
                    return _actual.Values;
                }
            }
        }
    }
}