using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PeanutButter.Utils.Dictionaries
{
    /// <summary>
    /// Wraps an object in a dictionary interface
    /// </summary>
    public class DictionaryWrappingObject : IDictionary<string, object>
    {
        private readonly object _wrapped;
        private PropertyOrField[] _props;
        private string[] _keys;

        /// <inheritdoc />
        public DictionaryWrappingObject(object wrapped)
        {
            _wrapped = wrapped;
        }

        private void CachePropertyInfos()
        {
            if (_props != null)
                return;
            var type = _wrapped?.GetType();
            if (type == null)
            {
                _props = new PropertyOrField[0];
                _keys = new string[0];
                return;
            }
            var flags = BindingFlags.Instance | BindingFlags.Public;
            _props = type.GetProperties(flags)
                .Select(pi => new PropertyOrField(pi))
                .Union(type.GetFields(flags).Select(fi => new PropertyOrField(fi)))
                .ToArray();
            _keys = _props.Select(p => p.Name).ToArray();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return new DictionaryWrappingObjectEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            throw new InvalidOperationException("Cannot clear properties on an object");
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            CachePropertyInfos();
            var prop = _props.FirstOrDefault(o => HasName(o, item.Key));
            return prop != null &&
                   prop.GetValue(_wrapped) == item.Value;
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            CachePropertyInfos();
            _props.ForEach((prop, i) =>
            {
                array[arrayIndex + i] = new KeyValuePair<string, object>(prop.Name, prop.GetValue(_wrapped));
            });
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new InvalidOperationException("Cannot remove properties from an object");
        }

        public int Count => GetCount();

        private int GetCount()
        {
            CachePropertyInfos();
            return _props.Length;
        }

        public bool IsReadOnly => false;

        public bool ContainsKey(string key)
        {
            CachePropertyInfos();
            return _keys.Any(o => KeysMatch(o, key));
        }

        public void Add(string key, object value)
        {
            throw new InvalidOperationException("Cannot add properties to an object");
        }

        public bool Remove(string key)
        {
            throw new InvalidOperationException("Cannot remove properties from an object");
        }

        public bool TryGetValue(string key, out object value)
        {
            CachePropertyInfos();
            var prop = FindPropertyByName(key);
            value = prop?.GetValue(_wrapped);
            return prop != null;
        }

        private PropertyOrField FindPropertyByName(string name)
        {
            return _props.FirstOrDefault(o => HasName(o, name));
        }

        public object this[string key]
        {
            get => ReadProperty(key);
            set => WriteProperty(key, value);
        }

        private void WriteProperty(string key, object value)
        {
            VerifyHasKey(key);
            var info = _props.First(o => HasName(o, key));
            info.SetValue(_wrapped, value);
        }

        private object ReadProperty(string key)
        {
            VerifyHasKey(key);
            var prop = _props.First(o => HasName(o, key));
            return prop.GetValue(_wrapped);
        }

        private void VerifyHasKey(string key)
        {
            if (Keys.Contains(key))
                return;
            throw new KeyNotFoundException(key);
        }

        private bool HasName(PropertyOrField prop, string key)
        {
            return KeysMatch(prop.Name, key);
        }

        private bool KeysMatch(string key1, string key2)
        {
            return key1 == key2; // TODO: allow case-insensitive
        }

        /// <inheritdoc />
        public ICollection<string> Keys => GetKeys();

        private ICollection<string> GetKeys()
        {
            CachePropertyInfos();
            return _keys;
        }

        public ICollection<object> Values => GetValues();

        private ICollection<object> GetValues()
        {
            CachePropertyInfos();
            return _values ?? (_values = GetPropertyAndFieldValues());
        }

        private object[] _values;

        private object[] GetPropertyAndFieldValues()
        {
            return _props.Select(p => p.GetValue(_wrapped)).ToArray();
        }

    }
}