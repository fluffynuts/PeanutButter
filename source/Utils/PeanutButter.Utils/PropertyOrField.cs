using System;
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
        public string Name => _propInfo?.Name ?? _fieldInfo.Name;
        /// <summary>
        /// Type of the property or field
        /// </summary>
        public Type Type => _propInfo?.PropertyType ?? _fieldInfo.FieldType;
        /// <summary>
        /// Write access to property or field
        /// </summary>
        public bool CanWrite => _propInfo?.CanWrite ?? true;
        /// <summary>
        /// Read access to property or field
        /// </summary>
        public bool CanRead => _propInfo?.CanRead ?? true;
        /// <summary>
        /// Is this a Property or a Field?
        /// </summary>
        public PropertyOrFieldTypes MemberType
            => _propInfo == null ? PropertyOrFieldTypes.Field : PropertyOrFieldTypes.Property;

        public Type DeclaringType => _propInfo?.DeclaringType ?? _fieldInfo?.DeclaringType;

        private readonly PropertyInfo _propInfo;
        private readonly FieldInfo _fieldInfo;

        /// <inheritdoc />
        public PropertyOrField(PropertyInfo prop)
        {
            _propInfo = prop;
        }

        /// <inheritdoc />
        public PropertyOrField(FieldInfo field)
        {
            _fieldInfo = field;
        }

        /// <summary>
        /// Gets the value of the property or field for the provided host
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public object GetValue(object host)
        {
            return _fieldInfo == null
                ? _propInfo.GetValue(host)
                : _fieldInfo.GetValue(host);
        }

        /// <summary>
        /// Sets the value of the property or field on the provided host
        /// </summary>
        /// <param name="host"></param>
        /// <param name="value"></param>
        public void SetValue(object host, object value)
        {
            if (_fieldInfo == null)
                _propInfo.SetValue(host, value);
            else
                _fieldInfo.SetValue(host, value);
        }
    }
}