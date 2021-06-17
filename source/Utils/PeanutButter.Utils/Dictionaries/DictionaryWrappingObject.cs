﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
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
    /// Defines an object which is wrapping another object and can be unwrapped with the
    /// provided methods
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        interface IWrapper
    {
        /// <summary>
        /// Unwrap the wrapped object as a plain old object
        /// </summary>
        /// <returns></returns>
        object Unwrap();

        /// <summary>
        /// Unwrap the wrapped object with a hard cast to T (buyer beware!)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Unwrap<T>();

        /// <summary>
        /// Attempts to unwrap a wrapped value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool TryUnwrap<T>(out T result);
    }

    /// <summary>
    /// Wraps an object in a dictionary interface
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class DictionaryWrappingObject : IDictionary<string, object>, IWrapper
    {
        /// <inheritdoc />
        public object Unwrap() => _wrapped;

        /// <inheritdoc />
        public T Unwrap<T>() => (T)_wrapped;

        /// <inheritdoc />
        public bool TryUnwrap<T>(out T result)
        {
            try
            {
                result = Unwrap<T>();
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }

        private readonly object _wrapped;
        private IPropertyOrField[] _props;
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
            bool forceWrappingDictionariesWithoutStringKeys,
            Dictionary<object, DictionaryWrappingObject> wrapperCache
        ) : this(wrapped, keyComparer, wrapRecursively, forceWrappingDictionariesWithoutStringKeys)
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
        ) : this(wrapped, keyComparer, wrapRecursively, false)
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
        /// <param name="keyComparer">
        /// Specify the key-comparer to use when reaching into objects
        /// - the default is case-sensitive, ordinal, but you can make
        ///   your wrapper instance more "forgiving" with a case-insensitive
        ///   key-comparer
        /// </param>
        /// <param name="forceWrappingDictionariesWithoutStringKeys">
        /// Ordinarily, if this wrapper encounters something implementing IDictionary&lt;,&gt;,
        /// then it will attempt to wrap into the key-value collection if, and only if
        /// the keys for that collection are strings. This means that normally, if provided
        /// a dictionary with non-string keys, wrapping will fail with an ArgumentException. If
        /// you're sure you'd like to wrap the actual properties of a dictionary, not the values
        /// in the dictionary, force wrapping by setting this to true.</param>
        public DictionaryWrappingObject(
            object wrapped,
            StringComparer keyComparer,
            bool wrapRecursively,
            bool forceWrappingDictionariesWithoutStringKeys
        )
        {
            Comparer = keyComparer;
            _wrapped = WrapIfRequired(wrapped);
            _wrapRecursively = wrapRecursively;
            _forceWrappingDictionariesWithoutStringKeys = forceWrappingDictionariesWithoutStringKeys;

            _propertyReader = wrapRecursively
                ? ReadWrappedProperty
                : ReadObjectProperty;
            if (_wrapped is not null)
            {
                _wrappedType = _wrapped.GetType();
                _wrapperCache[wrapped] = this;
            }
        }

        private object WrapIfRequired(object original)
        {
            if (original is null)
            {
                return null;
            }

            switch (original)
            {
                case NameValueCollection nvc:
                    return new DictionaryWrappingNameValueCollection(nvc);
                #if NETSTANDARD
                #else
                case ConnectionStringSettingsCollection connectionStringSettings:
                    return new DictionaryWrappingConnectionStringSettingCollection(connectionStringSettings);
                #endif
                default:
                    return original;
            }
        }

        private void CachePropertyInfos()
        {
            if (_props is not null)
            {
                return;
            }

            if (_wrappedType is null)
            {
                _props = new IPropertyOrField[] { };
                _keys = new Dictionary<string, string>(Comparer);
                return;
            }

            if (IsDictionaryTypeWithStringKeys(_wrappedType))
            {
                CacheDictionaryPropertyInfos();
                return;
            }

            var flags = BindingFlags.Instance | BindingFlags.Public;
            _props = _wrappedType.GetProperties(flags)
                .Select(pi => new PropertyOrField(pi))
                .Union(_wrappedType.GetFields(flags).Select(fi => new PropertyOrField(fi)))
                .ToArray();
            _keys = _props
                .Select(p => new KeyValuePair<string, string>(p.Name, p.Name))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, Comparer);
        }

        private bool IsDictionaryTypeWithStringKeys(Type type)
        {
            var interfaces = type.GetAllImplementedInterfaces();
            var genericInterface = interfaces.FirstOrDefault(
                i => i.IsGenericType() &&
                    i.GetGenericTypeDefinition() == typeof(IDictionary<,>)
            );
            if (genericInterface is null)
            {
                return false;
            }

            var genericParams = genericInterface.GetGenericArguments();
            if (genericParams[0] == typeof(string))
            {
                return interfaces.Any(interfaceType =>
                    interfaceType.IsGenericType() &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IDictionary<,>) &&
                    interfaceType.GetGenericArguments().First() == typeof(string)
                );
            }

            if (_forceWrappingDictionariesWithoutStringKeys)
            {
                return false;
            }

            throw new ArgumentException(
                $"Attempted to wrap a dictionary with non-string keys. If this is intentional, then set the relevant flag at construction time."
            );
        }
        
        private void CacheDictionaryPropertyInfos()
        {
            _keys = _wrapped.Get<IEnumerable<string>>(nameof(Keys))
                .Select(p => new KeyValuePair<string, string>(p, p))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value, Comparer);
            var valuesType = _wrapped.Get<object>(nameof(Values))
                .GetType();
            var valueType = valuesType.GetCollectionItemType();

            if (valueType is null)
            {
                throw new InvalidOperationException(
                    $"Can't determine the value type for the dictionary"
                );
            }

            var itemGetter = _wrappedType.GetMethod("get_Item") ?? throw new ArgumentException(
                $"Expected to wrap a dictionary, but required method 'get_Item' is missing"
            );
            var itemSetter = _wrappedType.GetMethod("set_Item") ?? throw new ArgumentException(
                $"Expected to wrap a dictionary, but required method 'set_Item' is missing"
            );
            _props = _keys.Keys.Select(k =>
            {
                return new FakeProperty(
                    k,
                    valueType,
                    true, // FIXME: what about read-only dictionaries? `IsReadOnly` appears to be eluding {object}.Get<bool>()
                    true,
                    _wrapped.GetType(),
                    (host) =>
                    {
                        return itemGetter.Invoke(host, new object[] { k });
                    },
                    (host, value) =>
                    {
                        itemSetter.Invoke(host, new object[] { k, value });
                    }
                ) as IPropertyOrField;
            }).ToArray();
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

        private IPropertyOrField FindPropertyByName(string name)
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
            var targetType = info.Type;
            var valueType = value?.GetType();
            if (targetType == valueType)
            {
                info.SetValue(_wrapped, value);
                return;
            }

            if (value is DictionaryWrappingObject wrapper)
            {
                WriteProperty(key, wrapper._wrapped);
                return;
            }

            throw new ArgumentException(
                $@"Attempt to set property '{key}' to value of type '{valueType}' will fail: target type is '{targetType}'"
            );
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

            var enumerableWrapper = new EnumerableEnumerableWrapper<object>(rawValue);
            if (enumerableWrapper.IsValid)
            {
                return LazilyEnumerate(enumerableWrapper);
            }

            return _wrapperCache[rawValue]
                = new DictionaryWrappingObject(
                    rawValue,
                    Comparer,
                    _wrapRecursively,
                    _forceWrappingDictionariesWithoutStringKeys,
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

        private bool HasName(IPropertyOrField prop, string key)
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
        private readonly bool _forceWrappingDictionariesWithoutStringKeys;
        private readonly Func<string, object> _propertyReader;
        private Type _wrappedType;

        private object[] GetPropertyAndFieldValues()
        {
            return _props.Select(p => p.GetValue(_wrapped)).ToArray();
        }
    }

    internal class FakeProperty : IPropertyOrField
    {
        private readonly Func<object, object> _valueGetter;
        private readonly Action<object, object> _valueSetter;
        public string Name { get; }
        public Type Type { get; }
        public bool CanWrite { get; }
        public bool CanRead { get; }
        public Type DeclaringType { get; }

        public object GetValue(object host)
        {
            if (!CanRead)
            {
                throw new InvalidOperationException(
                    $"Cannot read property {Name}"
                );
            }

            return _valueGetter(host);
        }

        public void SetValue(object host, object value)
        {
            if (!CanWrite)
            {
                throw new InvalidOperationException(
                    $"Cannot write property {Name}"
                );
            }

            _valueSetter(host, value);
        }

        public void SetValue<T>(ref T host, object value)
        {
            if (!Type.IsAssignableFrom(typeof(T)))
            {
                throw new InvalidOperationException(
                    $"Cannot set property of type {Type} from value of type {typeof(T)}"
                );
            }

            SetValue(host, value);
        }

        public FakeProperty(
            string name,
            Type type,
            bool canWrite,
            bool canRead,
            Type declaringType,
            Func<object, object> valueGetter,
            Action<object, object> valueSetter
        )
        {
            Name = name;
            Type = type;
            CanWrite = canWrite;
            CanRead = canRead;
            DeclaringType = declaringType;
            _valueGetter = valueGetter;
            _valueSetter = valueSetter;
        }
    }
}