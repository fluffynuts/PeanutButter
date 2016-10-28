using System;
using System.Collections.Generic;
using System.Reflection;

namespace PeanutButter.DuckTyping
{
    public class DictionaryPropertyFetcher : IPropertyInfoFetcher
    {
        private readonly Dictionary<string, object> _data;

        public DictionaryPropertyFetcher(Dictionary<string, object> data)
        {
            _data = data;
        }

        public PropertyInfo[] GetProperties(Type srcType, BindingFlags bindingFlags)
        {
            return new PropertyInfo[0];
        }
    }
}