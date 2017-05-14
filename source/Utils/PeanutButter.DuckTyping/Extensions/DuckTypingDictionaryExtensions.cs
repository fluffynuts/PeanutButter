using System.Collections.Generic;
using Shared = PeanutButter.DuckTyping.Extensions.DuckTypingExtensionSharedMethods;

namespace PeanutButter.DuckTyping.Extensions
{
    /// <summary>
    /// Provides duck-typing extension methods around dictionary objects
    /// </summary>
    public static class DuckTypingDictionaryExtensions
    {
        /// <summary>
        /// Forces approxmiate ducking around a dictionary. Will "create" underlying
        /// "properties" as required. Will attempt to convert to and from the underlying
        /// types as required. Will match properties case-insensitive.
        /// </summary>
        /// <param name="src"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ForceFuzzyDuckAs<T>(this IDictionary<string, object> src)
        {
            return Shared.ForceDuckAs<T>(src, true);
        }

        /// <summary>
        /// Forces ducking around a dictionary. This will expect matching of property
        /// names (case-sensitive) and types when they are "implemented". Otherwise they
        /// will be created as required.
        /// </summary>
        /// <param name="src"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ForceDuckAs<T>(this IDictionary<string, object> src)
        {
            return Shared.ForceDuckAs<T>(src, false);
        }
    }
}