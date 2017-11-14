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
    internal class DictionaryWrappingObject: IDictionary<string, object>
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
            _props = type.GetProperties(flags).Select(pi => new PropertyOrField(pi))
                        .Union(type.GetFields(flags).Select(fi => new PropertyOrField(fi)))
                        .ToArray();
            _keys = _props.Select(p => p.Name).ToArray();
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        public bool Remove(string key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out object value)
        {
            throw new NotImplementedException();
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
            var info = _props.First(o => HasName(o, key));
            return info.GetValue(_wrapped);
        }

        private void VerifyHasKey(string key)
        {
            if (Keys.Contains(key))
                return;
            throw new KeyNotFoundException(key);
        }

        private bool HasName(PropertyOrField prop, string key)
        {
            return prop.Name == key;    // TODO: allow case-insensitive
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