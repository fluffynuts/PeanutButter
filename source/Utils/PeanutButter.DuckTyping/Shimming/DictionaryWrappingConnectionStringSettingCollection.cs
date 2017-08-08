using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace PeanutButter.DuckTyping.Shimming
{
    internal class DictionaryWrappingConnectionStringSettingCollection :
        IDictionary<string, object>
    {
        private readonly ConnectionStringSettingsCollection _actual;

        public DictionaryWrappingConnectionStringSettingCollection(
            ConnectionStringSettingsCollection connectionStringSettings
        )
        {
            _actual = connectionStringSettings;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return new DictionaryWrappingConnectionStringSettingCollectionEnumerator(_actual);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Clear()
        {
            _actual.Clear();
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (var item in this)
                array[arrayIndex++] = item;
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            if (!TryGetValue(item.Key, out var match))
                return false;
            return match == item.Value && Remove(item.Key);
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return TryGetValue(item.Key, out var match) &&
                   match == item.Value;
        }

        public bool ContainsKey(string key)
        {
            return Keys.Contains(key);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }


        public void Add(string key, object value)
        {
            _actual.Add(new ConnectionStringSettings(key, value as string));
        }

        public bool Remove(string key)
        {
            if (!Keys.Contains(key))
                return false;
            _actual.Remove(key);
            return true;
        }

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

        public object this[string key]
        {
            get => _actual[key].ConnectionString;
            set => _actual[key].ConnectionString = value as string;
        }

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

        public int Count => _actual.Count;

        public ICollection<object> Values =>
            GetKeys()
                .Select(k => _actual[k].ConnectionString)
                .ToArray();

        public bool IsReadOnly => false;
    }
}