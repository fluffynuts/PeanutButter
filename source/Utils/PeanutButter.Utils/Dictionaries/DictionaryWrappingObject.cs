using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable MemberCanBePrivate.Global

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils.Dictionaries
#else
namespace PeanutButter.Utils.Dictionaries
#endif
{
    /// <summary>
    /// Wraps an object in a dictionary interface
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class DictionaryWrappingObject : IDictionary<string, object>
    {
        private readonly object _wrapped;
        private PropertyOrField[] _props;
        private Dictionary<string, string> _keys;
        private readonly Dictionary<string, object> _memberCache = new();
        private readonly Dictionary<object, DictionaryWrappingObject> _wrapperCache = new();

        /// <summary>
        /// The string comparer used to locate keys
        /// </summary>
        public StringComparer Comparer { get; }

        /// <summary>
        /// Provides a mechanism to reflectively read members on an object
        /// via an IDictionary&lt;string, object&gt; interface
        /// </summary>
        /// <param name="wrapped"></param>
        public DictionaryWrappingObject(object wrapped)
            : this(wrapped, StringComparer.Ordinal)
        {
        }

        /// <summary>
        /// Provides a mechanism to reflectively read members on an object
        /// via an IDictionary&lt;string, object&gt; interface
        /// </summary>
        /// <param name="wrapped"></param>
        /// <param name="wrapRecursively">
        /// When wrapping recursively, all properties are returned
        /// as DictionaryWrappingObject instances so they can be
        /// interrogated as dictionaries
        /// </param>
        public DictionaryWrappingObject(
            object wrapped,
            bool wrapRecursively
        ) : this(
            wrapped,
            StringComparer.Ordinal,
            wrapRecursively
        )
        {
        }

        /// <summary>
        /// Provides a mechanism to reflectively read members on an object
        /// via an IDictionary&lt;string, object&gt; interface
        /// </summary>
        /// <param name="wrapped"></param>
        /// <param name="keyComparer">
        /// Specify the key-comparer to use when reaching into objects
        /// - the default is case-sensitive, ordinal, but you can make
        ///   your wrapper instance more "forgiving" with a case-insensitive
        ///   key-comparer
        /// </param>
        public DictionaryWrappingObject(
            object wrapped,
            StringComparer keyComparer
        ) : this(wrapped, keyComparer, false)
        {
        }

        private DictionaryWrappingObject(
            object wrapped,
            StringComparer keyComparer,
            bool wrapRecursively,
            Dictionary<object, DictionaryWrappingObject> wrapperCache
        ) : this(wrapped, keyComparer, wrapRecursively)
        {
            _wrapperCache = wrapperCache;
        }

        /// <summary>
        /// Provides a mechanism to reflectively read members on an object
        /// via an IDictionary&lt;string, object&gt; interface
        /// </summary>
        /// <param name="wrapped"></param>
        /// <param name="wrapRecursively">
        /// When wrapping recursively, all properties are returned
        /// as DictionaryWrappingObject instances so they can be
        /// interrogated as dictionaries
        /// </param>
        /// <param name="keyComparer">
        /// Specify the key-comparer to use when reaching into objects
        /// - the default is case-sensitive, ordinal, but you can make
        ///   your wrapper instance more "forgiving" with a case-insensitive
        ///   key-comparer
        /// </param>
        public DictionaryWrappingObject(
            object wrapped,
            StringComparer keyComparer,
            bool wrapRecursively
        )
        {
            Comparer = keyComparer;
            _wrapped = wrapped;
            _wrapRecursively = wrapRecursively;

            _propertyReader = wrapRecursively
                ? ReadWrappedProperty
                : ReadObjectProperty;
            _wrapperCache[wrapped] = this;
        }

        private void CachePropertyInfos()
        {
            if (_props != null)
                return;
            var type = _wrapped?.GetType();
            if (type == null)
            {
                _props = new PropertyOrField[0];
                _keys = new Dictionary<string, string>(Comparer);
                return;
            }

            var flags = BindingFlags.Instance | BindingFlags.Public;
            _props = type.GetProperties(flags)
                .Select(pi => new PropertyOrField(pi))
                .Union(type.GetFields(flags).Select(fi => new PropertyOrField(fi)))
                .ToArray();
            _keys = _props
                .Select(p => new KeyValuePair<string, string>(p.Name, p.Name))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, Comparer);
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return new DictionaryWrappingObjectEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        /// <inheritdoc />
        public void Clear()
        {
            throw new InvalidOperationException("Cannot clear properties on an object");
        }

        /// <inheritdoc />
        public bool Contains(KeyValuePair<string, object> item)
        {
            CachePropertyInfos();
            var prop = _props.FirstOrDefault(o => HasName(o, item.Key));
            return prop != null &&
                prop.GetValue(_wrapped) == item.Value;
        }

        /// <inheritdoc />
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            CachePropertyInfos();
            _props.ForEach((prop, i) =>
            {
                array[arrayIndex + i] = new KeyValuePair<string, object>(prop.Name, prop.GetValue(_wrapped));
            });
        }

        /// <inheritdoc />
        public bool Remove(KeyValuePair<string, object> item)
        {
            throw new InvalidOperationException("Cannot remove properties from an object");
        }

        /// <inheritdoc />
        public int Count => GetCount();

        private int GetCount()
        {
            CachePropertyInfos();
            return _props.Length;
        }

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public bool ContainsKey(string key)
        {
            CachePropertyInfos();
            return _keys.ContainsKey(key);
        }

        /// <inheritdoc />
        public void Add(string key, object value)
        {
            throw new InvalidOperationException("Cannot add properties to an object");
        }

        /// <inheritdoc />
        public bool Remove(string key)
        {
            throw new InvalidOperationException("Cannot remove properties from an object");
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
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
            return _propertyReader(key);
        }

        private object ReadObjectProperty(string key)
        {
            var prop = _props.First(o => HasName(o, key));
            return prop.GetValue(_wrapped);
        }

        private object ReadWrappedProperty(string key)
        {
            if (_memberCache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var rawValue = ReadObjectProperty(key);
            var result = rawValue?.GetType().IsPrimitiveOrImmutable() ?? false
                ? rawValue
                : FindOrCreateWrapperFor(rawValue);
            return _memberCache[key] = result;
        }

        private object FindOrCreateWrapperFor(object rawValue)
        {
            if (rawValue is null)
            {
                return null;
            }

            if (_wrapperCache.TryGetValue(rawValue, out var result))
            {
                return result;
            }

            var enumerableWrapper = new EnumerableWrapper<object>(rawValue);
            if (enumerableWrapper.IsValid)
            {
                return LazilyEnumerate(enumerableWrapper);
            }

            return _wrapperCache[rawValue]
                = new DictionaryWrappingObject(
                    rawValue,
                    Comparer,
                    _wrapRecursively,
                    _wrapperCache
                );
        }

        private IEnumerable<IDictionary<string, object>> LazilyEnumerate(
            IEnumerable<object> objects
        )
        {
            foreach (var item in objects)
            {
                yield return FindOrCreateWrapperFor(item) as DictionaryWrappingObject;
            }
        }

        private void VerifyHasKey(string key)
        {
            CachePropertyInfos();
            if (_keys.ContainsKey(key))
            {
                return;
            }

            throw new KeyNotFoundException(key);
        }

        private bool HasName(PropertyOrField prop, string key)
        {
            return KeysMatch(prop.Name, key);
        }

        private bool KeysMatch(string key1, string key2)
        {
            return Comparer.Equals(key1, key2);
        }

        /// <inheritdoc />
        public ICollection<string> Keys => GetKeys();

        private ICollection<string> GetKeys()
        {
            CachePropertyInfos();
            return _keys.Select(kvp => kvp.Value).ToArray();
        }

        /// <inheritdoc />
        public ICollection<object> Values => GetValues();

        private ICollection<object> GetValues()
        {
            CachePropertyInfos();
            return _values ??= GetPropertyAndFieldValues();
        }

        private object[] _values;
        private readonly bool _wrapRecursively;
        private readonly Func<string, object> _propertyReader;

        private object[] GetPropertyAndFieldValues()
        {
            return _props.Select(p => p.GetValue(_wrapped)).ToArray();
        }
    }
}