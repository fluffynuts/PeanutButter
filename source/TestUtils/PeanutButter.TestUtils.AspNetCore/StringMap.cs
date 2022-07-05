using System.Collections;
using System.Collections.Generic;

namespace PeanutButter.TestUtils.AspNetCore
{
    public class StringMap : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly Dictionary<string, string> _store = new();

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _store.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _store.Count;
        public ICollection<string> Keys => _store.Keys;

        public bool ContainsKey(string key)
        {
            return _store.ContainsKey(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return _store.TryGetValue(key, out value);
        }

        public string this[string key]
        {
            get => _store[key];
            set => _store[key] = value;
        }
    }
}