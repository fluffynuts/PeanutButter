using System;
using System.Reflection;

namespace PeanutButter.DuckTyping
{
    public interface IPropertyInfoFetcher
    {
        PropertyInfo[] GetProperties(Type srcType, BindingFlags bindingFlags);
        PropertyInfo[] GetPropertiesFor(object obj, BindingFlags bindingFlags);
    }
}