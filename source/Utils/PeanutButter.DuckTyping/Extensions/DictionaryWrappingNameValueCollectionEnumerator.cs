using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PeanutButter.DuckTyping.Extensions
{
    // TODO: add expllicit tests around this class, which is currently only tested by indirection
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