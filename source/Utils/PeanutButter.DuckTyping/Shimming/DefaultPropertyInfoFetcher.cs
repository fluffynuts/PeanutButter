using System;
using System.Reflection;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.Shimming
#else
namespace PeanutButter.DuckTyping.Shimming
#endif
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