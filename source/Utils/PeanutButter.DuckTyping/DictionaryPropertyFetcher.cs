using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace PeanutButter.DuckTyping
{
    public class DictionaryPropertyFetcher : IPropertyInfoFetcher
    {
        public PropertyInfo[] GetProperties(Type srcType, BindingFlags bindingFlags)
        {
            return srcType.GetProperties();
        }

        public PropertyInfo[] GetPropertiesFor(object obj, BindingFlags bindingFlags)
        {
            var asDictionary = obj as Dictionary<string, object>;
            return asDictionary != null
                ? GetDictionaryPropertiesFor(asDictionary)
                : GetProperties(obj.GetType(), bindingFlags);
        }

        private PropertyInfo[] GetDictionaryPropertiesFor(Dictionary<string, object> asDictionary)
        {
            return asDictionary.Select(kvp =>
                new DictionaryPropertyInfo(kvp.Key, kvp.Value?.GetType()) as PropertyInfo
                ).ToArray();
        }
    }

    public class DictionaryPropertyInfo : PropertyInfo
    {
        public DictionaryPropertyInfo(string propertyName, Type propertyType)
        {
            Name = propertyName;
            PropertyType = propertyType;
            CanRead = true;
            CanWrite = true;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return new object[0];
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return false;
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return new MethodInfo[0];
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return null;
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return null;
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return new ParameterInfo[0];
        }

        public override string Name { get; }
        public override Type DeclaringType { get; }
        public override Type ReflectedType { get; }
        public override Type PropertyType { get; }
        public override PropertyAttributes Attributes { get; }
        public override bool CanRead { get; }
        public override bool CanWrite { get; }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return new object[0];
        }
    }
}