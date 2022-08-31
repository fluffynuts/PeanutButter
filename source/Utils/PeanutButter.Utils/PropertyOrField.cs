using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

// ReSharper disable UnusedMember.Global

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Differentiates between PropertyOrField storage for properties or fields
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        enum PropertyOrFieldTypes
    {
        /// <summary>
        /// This member is a Property
        /// </summary>
        Property,

        /// <summary>
        /// This member is a Field
        /// </summary>
        Field
    }

    /// <summary>
    /// Represents a property or a field on an object
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        interface IPropertyOrField
    {
        /// <summary>
        /// Name of the property or field
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Type of the property or field
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Write access to property or field
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        /// Read access to property or field
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// The type on which this property or field is declared
        /// </summary>
        Type DeclaringType { get; }

        /// <summary>
        /// The type from which this property or field is read
        /// - this may not be the DeclaringType as the property
        /// or field may be inherited
        /// - this must be explicitly provided by callers
        /// </summary>
        Type HostingType { get; }

        /// <summary>
        /// Returns the ancestral distance between the DeclaringType
        /// and the HostingType (0 if they are the same type)
        /// </summary>
        int AncestralDistance { get; }

        /// <summary>
        /// Gets the value of the property or field for the provided host
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        object GetValue(object host);

        /// <summary>
        /// Attempts to get the value of the property
        /// - if the getter throws, returns false and the output exception is set
        /// - if the getter succeeds, returns true and the output value is set
        /// </summary>
        /// <param name="host"></param>
        /// <param name="value"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        bool TryGetValue(object host, out object value, out Exception exception);

        /// <summary>
        /// Sets the value of the property or field on the provided host
        /// </summary>
        /// <param name="host"></param>
        /// <param name="value"></param>
        void SetValue(object host, object value);

        /// <summary>
        /// Sets the value for the field or property
        /// as found on the provided host
        /// </summary>
        /// <param name="host"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        void SetValue<T>(ref T host, object value);

        /// <summary>
        /// Sets the value in a collection at that index, if possible
        /// </summary>
        /// <param name="host"></param>
        /// <param name="value"></param>
        /// <param name="index"></param>
        void SetValueAt(object host, object value, object index);

        /// <summary>
        /// Get the value at the provided index into a collection
        /// </summary>
        /// <param name="host"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        object GetValueAt(object host, object index);
    }

    /// <summary>
    /// Provides a single storage / representation
    /// for a Property or a Field
    /// </summary>
    [DebuggerDisplay("{MemberType} {Type} {Name} read: {CanRead} write: {CanWrite}")]
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class PropertyOrField
        : IPropertyOrField
    {
        /// <summary>
        /// Creates a PropertyOrField container for a provided PropertyInfo
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static PropertyOrField Create(PropertyInfo propertyInfo)
        {
            return new PropertyOrField(propertyInfo);
        }

        /// <summary>
        /// Creates a PropertyOrField container for a provided FieldInfo
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        public static PropertyOrField Create(FieldInfo fieldInfo)
        {
            return new PropertyOrField(fieldInfo);
        }

        private static readonly BindingFlags SearchBindingFlags =
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.Static;

        /// <summary>
        /// Attempts to find a property or field with the given name on
        /// a type - will scan public, private, static and instance properties
        /// and fields. It's up to the caller to know what do to with that (:
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <exception cref="ArgumentException">thrown when the property or field is not found</exception>
        /// <returns></returns>
        public static PropertyOrField Find(
            Type type,
            string name
        )
        {
            return TryFind(type, name)
                ?? throw new ArgumentException(
                    $"No property or field named '{name}' found on type '{type}'",
                    nameof(name)
                );
        }

        private static readonly ConcurrentDictionary<Tuple<Type, string>, PropertyOrField>
            FindCache = new();

        /// <summary>
        /// Attempts to find a property or field with the given name on
        /// a type - will scan public, private, static and instance properties
        /// and fields. It's up to the caller to know what do to with that (:
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PropertyOrField TryFind(
            Type type,
            string name
        )
        {
            var cacheKey = Tuple.Create(type, name);
            if (FindCache.TryGetValue(cacheKey, out var result))
            {
                return result;
            }

            var propInfo = type.GetProperties(
                SearchBindingFlags
            ).FirstOrDefault(pi => pi.Name == name);
            if (propInfo is not null)
            {
                result = Create(propInfo);
                FindCache.TryAdd(cacheKey, result);
                return result;
            }

            var fieldInfo = type.GetFields(
                SearchBindingFlags
            ).FirstOrDefault(fi => fi.Name == name);

            if (fieldInfo is null)
            {
                FindCache.TryAdd(cacheKey, null);
                return null;
            }

            result = Create(fieldInfo);
            FindCache.TryAdd(cacheKey, result);
            return result;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public Type Type { get; }

        /// <inheritdoc />
        public bool CanWrite { get; }

        /// <inheritdoc />
        public bool CanRead { get; }

        /// <summary>
        /// Is this a Property or a Field?
        /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
        internal
#else
        public
#endif
            PropertyOrFieldTypes MemberType { get; }

        /// <inheritdoc />
        public Type DeclaringType { get; }

        /// <inheritdoc />
        public Type HostingType { get; }

        /// <inheritdoc />
        public int AncestralDistance { get; }

        private readonly Func<object, object> _getValue;
        private readonly Func<object, object[], object> _getValueIndexed;
        private readonly Action<object, object> _setValue;

        /// <summary>
        /// Constructs the PropertyOrField around a property
        /// </summary>
        /// <param name="prop"></param>
        public PropertyOrField(
            PropertyInfo prop
        ) : this(prop, prop.DeclaringType)
        {
        }

        /// <summary>
        /// Constructs the PropertyOrField around a property, relative
        /// to an hosting type (ie, without assuming that the DeclaringType
        /// is the hosting type for the property)
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="hostingType"></param>
        public PropertyOrField(
            PropertyInfo prop,
            Type hostingType
        )
        {
            if (prop is null)
            {
                throw new ArgumentNullException(nameof(prop));
            }

            PropertyInfo = prop;

            _getValue = prop.GetValue;
            _getValueIndexed = prop.GetValue;
            _setValue = prop.SetValue;

            Name = prop.Name;
            Type = prop.PropertyType;
            DeclaringType = prop.DeclaringType;
            MemberType = PropertyOrFieldTypes.Property;
            CanRead = prop.CanRead;
            CanWrite = prop.CanWrite;
            HostingType = hostingType ?? throw new ArgumentNullException(nameof(hostingType));
            AncestralDistance = CalculateAncestralDistance();
        }

        /// <summary>
        ///  the provided prop
        /// </summary>
        public PropertyInfo PropertyInfo { get; }

        /// <summary>
        /// Implicitly converts a PropertyInfo object to a PropertyOrField
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static implicit operator PropertyOrField(PropertyInfo prop)
        {
            return new PropertyOrField(prop);
        }

        /// <summary>
        /// Implicitly converts a FieldInfo object to a FieldOrField
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static implicit operator PropertyOrField(
            FieldInfo field
        )
        {
            return new PropertyOrField(field);
        }

        /// <summary>
        /// Constructs the PropertyOrField around a field
        /// </summary>
        /// <param name="field"></param>
        public PropertyOrField(
            FieldInfo field
        ) : this(field, field.DeclaringType)
        {
        }

        /// <summary>
        /// Constructs the PropertyOrField around a field
        /// </summary>
        /// <param name="field"></param>
        /// <param name="hostingType"></param>
        public PropertyOrField(
            FieldInfo field,
            Type hostingType
        )
        {
            if (field is null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            _getValue = field.GetValue;
            _setValue = field.SetValue;

            Name = field.Name;
            Type = field.FieldType;
            DeclaringType = field.DeclaringType;
            MemberType = PropertyOrFieldTypes.Field;
            CanRead = true;
            CanWrite = true;
            HostingType = hostingType ?? throw new ArgumentNullException(nameof(hostingType));
            AncestralDistance = CalculateAncestralDistance();
        }

        /// <inheritdoc />
        public object GetValue(object host)
        {
            return _getValue(host);
        }

        /// <inheritdoc />
        public object GetValueAt(object host, object index)
        {
            if (index is null)
            {
                throw new InvalidOperationException("Index may not be null");
            }

            var collection = _getValue(host);
            if (collection is IList list)
            {
                return list[RequireIntegerIndex(index)];
            }

            if (collection is IDictionary dict)
            {
                return dict[index];
            }

            var wrapper = new EnumerableWrapper(collection);
            if (wrapper.IsValid)
            {
                var i = 0;
                var intIndex = RequireIntegerIndex(index);
                foreach (var item in wrapper)
                {
                    if (i++ == intIndex)
                    {
                        return item;
                    }
                }

                throw new IndexOutOfRangeException();
            }

            throw new InvalidOperationException(
                $"Unable to index into '{Name}'"
            );
        }

        /// <inheritdoc />
        public bool TryGetValue(object host, out object value, out Exception exception)
        {
            try
            {
                exception = default;
                value = GetValue(host);
                return true;
            }
            catch (Exception ex)
            {
                value = default;
                exception = ex;
                return false;
            }
        }

        /// <inheritdoc />
        public void SetValue(object host, object value)
        {
            if (value is null)
            {
                if (!Type.IsNullableType())
                {
                    throw new ArgumentException($"Cannot set type {Type} to null");
                }

                _setValue(host, null);
                return;
            }

            if (!value.TryImplicitlyCastTo(Type, out var castValue))
            {
                throw new ArgumentException(
                    $"Cannot set value '{value}' of type {value.GetType()} for target {Type}"
                );
            }

            _setValue(host, castValue);
        }

        /// <inheritdoc />
        public void SetValueAt(object host, object value, object index)
        {
            if (index is null)
            {
                throw new InvalidOperationException("Index may not be null");
            }

            var collection = _getValue(host);
            if (collection is IList list)
            {
                list[RequireIntegerIndex(index)] = value;
                return;
            }

            if (collection is IDictionary dict)
            {
                dict[index] = value;
                return;
            }

            throw new NotSupportedException(
                $"Setting indexed values on items of type '{Type}' is not supported"
            );
        }

        private int RequireIntegerIndex(object idx)
        {
            try
            {
                return (int) idx;
            }
            catch (InvalidCastException)
            {
                throw new NotSupportedException(
                    $"Indexing int '{Name}' requires an integer index"
                );
            }
        }


        /// <inheritdoc />
        public void SetValue<T>(ref T host, object value)
        {
            var asObject = (object) host;
            _setValue(asObject, value);
            // required for referenced by-val sets to work (ie struct values)
            host = (T) asObject;
        }

        private int CalculateAncestralDistance()
        {
            if (DeclaringType == HostingType)
            {
                return 0;
            }

            var result = 0;
            var current = HostingType;
            while (
                current != typeof(object) &&
                current != DeclaringType
            )
            {
                result++;
                current = current?.BaseType;
            }

            if (current is null && DeclaringType != typeof(object))
            {
                throw new ArgumentException(
                    $"{DeclaringType} is not an ancestor of {HostingType}"
                );
            }

            return result;
        }
    }
}