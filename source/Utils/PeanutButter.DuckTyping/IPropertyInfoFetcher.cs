using System;
using System.Reflection;

namespace PeanutButter.DuckTyping
{
    /// <summary>
    /// Interface to implement for a utility to fetch properties on a type or object
    /// </summary>
    public interface IPropertyInfoFetcher
    {
        /// <summary>
        /// Fetches properties from a type which conform to the provided binding flags
        /// </summary>
        /// <param name="srcType">Type to inspect</param>
        /// <param name="bindingFlags">Binding Flags to match</param>
        /// <returns>Array of all properties found on the type which match the given binding flags</returns>
        PropertyInfo[] GetProperties(Type srcType, BindingFlags bindingFlags);

        /// <summary>
        /// Fetches properties from an object which conform to the provided binding flags
        /// </summary>
        /// <param name="obj">Object to inspect</param>
        /// <param name="bindingFlags">Binding Flags to match</param>
        /// <returns>Array of all properties found on the object which match the given binding flags</returns>
        PropertyInfo[] GetPropertiesFor(object obj, BindingFlags bindingFlags);
    }
}