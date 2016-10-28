using System;
using System.Reflection;

namespace PeanutButter.DuckTyping
{
    public class DefaultPropertyInfoFetcher: IPropertyInfoFetcher
    {
        public PropertyInfo[] GetProperties(Type srcType, BindingFlags bindingFlags)
        {
            return srcType.GetProperties(bindingFlags);
        }
    }
}