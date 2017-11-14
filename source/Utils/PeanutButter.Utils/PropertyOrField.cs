using System;
using System.Reflection;

namespace PeanutButter.Utils
{
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
    enum PropertyOrFieldTypes
    {
        Property,
        Field
    }
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
    class PropertyOrField
    {
        public static PropertyOrField Create(PropertyInfo propertyInfo)
        {
            return new PropertyOrField(propertyInfo);
        }

        public static PropertyOrField Create(FieldInfo fieldInfo)
        {
            return new PropertyOrField(fieldInfo);
        }

        public string Name => _propInfo?.Name ?? _fieldInfo.Name;
        public Type Type => _propInfo?.PropertyType ?? _fieldInfo.FieldType;
        public bool CanWrite => _propInfo?.CanWrite ?? true;
        public bool CanRead => _propInfo?.CanRead ?? true;
        public PropertyOrFieldTypes MemberType 
            => _propInfo == null ? PropertyOrFieldTypes.Field : PropertyOrFieldTypes.Property;

        private PropertyInfo _propInfo;
        private FieldInfo _fieldInfo;

        public PropertyOrField(PropertyInfo prop)
        {
            _propInfo = prop;
        }

        public PropertyOrField(FieldInfo field)
        {
            _fieldInfo = field;
        }

        public object GetValue(object host)
        {
            return _fieldInfo == null
                ? _propInfo.GetValue(host)
                : _fieldInfo.GetValue(host);
        }

        public void SetValue(object host, object value)
        {
            if (_fieldInfo == null)
                _propInfo.SetValue(host, value);
            else
                _fieldInfo.SetValue(host, value);               
        }
    }
}