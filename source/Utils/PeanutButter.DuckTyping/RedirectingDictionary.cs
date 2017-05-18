using System;
using System.Collections;
using System.Collections.Generic;
using TransformFunc = System.Func<string, string>;

namespace PeanutButter.DuckTyping
{
    /// <summary>
    /// Provides a wrapping read-write layer around another dictionaryeffectively
    ///     allowing transparent rename of the keys
    /// </summary>
    /// <typeparam name="TValue">Type of values stored</typeparam>
    public class RedirectingDictionary<TValue>
        : IDictionary<string, TValue>
    {
        private readonly IDictionary<string, TValue> _data;
        private readonly TransformFunc _keyTransform;

        /// <summary>
        /// Constructs a new RedirectingDictionary
        /// </summary>
        /// <param name="data">Data to wrap</param>
        /// <param name="keyTransform">Function to transform keys from those used against this object to native ones in the data parameter</param>
        /// <exception cref="ArgumentNullException">Thrown if null data or key transform are supplied </exception>
        public RedirectingDictionary(
            IDictionary<string, TValue> data,
            TransformFunc keyTransform
        )
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (keyTransform == null) throw new ArgumentNullException(nameof(keyTransform));
            _data = data;
            _keyTransform = keyTransform;
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            return new RedirectingDictionaryEnumerator<TValue>(_data, _keyTransform);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<string, TValue> item)
        {
            // TODO: find a way to make it such that the given item is also updated
            //      when the dictionary is updated -- currently it won't be
            _data.Add(new KeyValuePair<string, TValue>(_keyTransform(item.Key), item.Value));
        }

        /// <inheritdoc />
        public void Clear()
        {
            _data.Clear();
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, TValue> item)
        {
            return false;
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, TValue> item)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public int Count => _data.Count;

        /// <inheritdoc />
        public bool IsReadOnly => _data.IsReadOnly;

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Add(string key, TValue value)
        {
            _data.Add(_keyTransform(key), value);
        }

        /// <inheritdoc />
        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out TValue value)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public TValue this[string key]
        {
            get
            {
                var nativeKey = _keyTransform(key);
                return _data[nativeKey];
            }
            set
            {
                if (IsReadOnly)
                    throw new InvalidOperationException("Collection is read-only");
                var nativeKey = _keyTransform(key);
                _data[nativeKey] = value;
            }
        }

        /// <inheritdoc />
        public ICollection<string> Keys { get; }

        /// <inheritdoc />
        public ICollection<TValue> Values { get; }
    }
}