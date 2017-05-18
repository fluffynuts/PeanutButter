using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PeanutButter.DuckTyping
{
    internal class RedirectingDictionaryEnumerator<T> : IEnumerator<KeyValuePair<string, T>>
    {
        private readonly IDictionary<string, T> _data;
        private readonly Func<string, string> _keyTransform;
        private string[] _nativeKeys;
        private int _currentIndex;

        internal RedirectingDictionaryEnumerator(
            IDictionary<string, T> data,
            Func<string, string> keyTransform
        )
        {
            _data = data;
            _keyTransform = keyTransform;
            Reset();
        }

        public void Dispose()
        {
            /* does nothing */
        }

        public bool MoveNext()
        {
            _currentIndex++;
            return _currentIndex < _nativeKeys.Length;
        }

        public void Reset()
        {
            _currentIndex = 0;
            _nativeKeys = _data.Keys.ToArray();
        }

        public KeyValuePair<string, T> Current
        {
            get
            {
                var nativeKey = _nativeKeys[_currentIndex];
                var key = _keyTransform(nativeKey);
                return new KeyValuePair<string, T>(key, _data[nativeKey]);
            }
        }

        object IEnumerator.Current => Current;
    }
}