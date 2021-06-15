using System;
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
    public interface IPropertyOrField
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
        /// Gets the value of the property or field for the provided host
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        object GetValue(object host);

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

        private readonly Func<object, object> _getValue;
        private readonly Action<object, object> _setValue;

        /// <summary>
        /// Constructs the PropertyOrField around a property
        /// </summary>
        /// <param name="prop"></param>
        public PropertyOrField(PropertyInfo prop)
        {
            _getValue = prop.GetValue;
            _setValue = prop.SetValue;

            Name = prop.Name;
            Type = prop.PropertyType;
            DeclaringType = prop.DeclaringType;
            MemberType = PropertyOrFieldTypes.Property;
            CanRead = prop.CanRead;
            CanWrite = prop.CanWrite;
        }

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
        public static implicit operator PropertyOrField(FieldInfo field)
        {
            return new PropertyOrField(field);
        }

        /// <summary>
        /// Constructs the PropertyOrField around a field
        /// </summary>
        /// <param name="field"></param>
        public PropertyOrField(FieldInfo field)
        {
            _getValue = field.GetValue;
            _setValue = field.SetValue;

            Name = field.Name;
            Type = field.FieldType;
            DeclaringType = field.DeclaringType;
            MemberType = PropertyOrFieldTypes.Field;
            CanRead = true;
            CanWrite = true;
        }

        /// <inheritdoc />
        public object GetValue(object host)
        {
            return _getValue(host);
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
        public void SetValue<T>(ref T host, object value)
        {
            var asObject = (object)host;
            _setValue(asObject, value);
            // required for referenced by-val sets to work (ie struct values)
            host = (T)asObject;
        }
    }
}