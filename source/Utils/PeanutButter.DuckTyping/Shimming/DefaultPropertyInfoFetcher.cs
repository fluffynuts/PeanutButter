using System;
using System.Reflection;

namespace PeanutButter.DuckTyping.Shimming
{
    internal class DefaultPropertyInfoFetcher: IPropertyInfoFetcher
    {
        public PropertyInfo[] GetProperties(Type srcType, BindingFlags bindingFlags)
        {
            return srcType.GetProperties(bindingFlags);
        }

        public PropertyInfo[] GetPropertiesFor(object obj, BindingFlags bindingFlags)
        {
            return GetProperties(obj.GetType(), bindingFlags);
        }
    }
}