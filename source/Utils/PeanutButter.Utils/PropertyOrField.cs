using System;
using System.Diagnostics;
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

        /// <summary>
        /// Name of the property or field
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Type of the property or field
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Write access to property or field
        /// </summary>
        public bool CanWrite { get; }

        /// <summary>
        /// Read access to property or field
        /// </summary>
        public bool CanRead { get; }

        /// <summary>
        /// Is this a Property or a Field?
        /// </summary>
        public PropertyOrFieldTypes MemberType { get; }

        /// <summary>
        /// The type on which this property or field is declared
        /// </summary>
        public Type DeclaringType { get; }

        private readonly Func<object, object> _getValue;
        private readonly Action<object, object> _setValue;

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <summary>
        /// Gets the value of the property or field for the provided host
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public object GetValue(object host)
        {
            return _getValue(host);
        }

        /// <summary>
        /// Sets the value of the property or field on the provided host
        /// </summary>
        /// <param name="host"></param>
        /// <param name="value"></param>
        public void SetValue(object host, object value)
        {
            _setValue(host, value);
        }

        /// <summary>
        /// Sets the value for the field or property
        /// as found on the provided host
        /// </summary>
        /// <param name="host"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public void SetValue<T>(ref T host, object value)
        {
            var asObject = (object) host;
            _setValue(asObject, value);
            // required for referenced by-val sets to work (ie struct values)
            host = (T) asObject;
        }
    }
}