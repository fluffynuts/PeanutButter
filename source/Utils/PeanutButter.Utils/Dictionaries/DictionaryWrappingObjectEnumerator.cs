using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PeanutButter.Utils.Dictionaries
{
    internal class DictionaryWrappingObjectEnumerator : IEnumerator<KeyValuePair<string, object>>
    {
        private readonly DictionaryWrappingObject _dict;
        private int _index;
        private readonly string[] _keys;

        public DictionaryWrappingObjectEnumerator(DictionaryWrappingObject dict)
        {
            _dict = dict;
            _keys = _dict.Keys.ToArray();
            Reset();
        }

        public void Dispose()
        {
            /* intentionally left blank */
        }

        public bool MoveNext()
        {
            return ++_index < _keys.Length;
        }

        public void Reset()
        {
            _index = -1;
        }

        public KeyValuePair<string, object> Current => 
            new KeyValuePair<string, object>(_keys[_index], _dict[_keys[_index]]);

        object IEnumerator.Current => Current;
    }
}