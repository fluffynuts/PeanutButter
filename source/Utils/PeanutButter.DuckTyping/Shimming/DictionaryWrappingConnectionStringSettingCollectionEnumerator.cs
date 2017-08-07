using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace PeanutButter.DuckTyping.Shimming
{
    internal class DictionaryWrappingConnectionStringSettingCollectionEnumerator :
        IEnumerator<KeyValuePair<string, string>>
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

        public KeyValuePair<string, string> Current 
            => new KeyValuePair<string, string>(
                _keys[_currentIndex], _actual[_keys[_currentIndex]].ConnectionString
            );

        object IEnumerator.Current => Current;
    }
}