using System.Collections.Generic;
using System.Collections.Specialized;
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.DuckTyping.Extensions
{
    /// <summary>
    /// Provides duck-typing around NameValueCollections
    /// </summary>
    public static class DuckTypingNameValueCollectionExtensions
    {
        internal static IDictionary<string, object> ToDictionary(this NameValueCollection src)
        {
            return new DictionaryWrappingNameValueCollection(src);
        }

        /// <summary>
        /// Ducks a NameValueCollection onto an interface
        /// </summary>
        /// <param name="src">NameValueCollection to operate on</param>
        /// <typeparam name="T">Interface you wish to work through</typeparam>
        /// <returns>New instance of an object implementing the required interface, if possible; otherwise null</returns>
        public static T DuckAs<T>(this NameValueCollection src)
            where T : class
        {
            return src.DuckAs<T>(false);
        }

        /// <summary>
        /// Ducks a NameValueCollection onto an interface
        /// </summary>
        /// <param name="src">NameValueCollection to operate on</param>
        /// <param name="throwOnError">Flag: throw an exception instead of just returning null</param>
        /// <typeparam name="T">Interface you wish to work through</typeparam>
        /// <returns>New instance of an object implementing the required interface</returns>
        public static T DuckAs<T>(this NameValueCollection src, bool throwOnError)
            where T : class
        {
            return src
                .ToDictionary()
                .DuckAs<T>(throwOnError);
        }

        /// <summary>
        /// Fuzzy-Ducks a NameValueCollection onto an interface, not caring about case or whitespace in property names
        /// </summary>
        /// <param name="src">NameValueCollection to operate on</param>
        /// <typeparam name="T">Interface you wish to work through</typeparam>
        /// <returns>New instance of an object implementing the required interface, if possible; otherwise null</returns>
        public static T FuzzyDuckAs<T>(this NameValueCollection src)
            where T : class
        {
            return src.FuzzyDuckAs<T>(false);
        }

        /// <summary>
        /// Fuzzy-Ducks a NameValueCollection onto an interface, not caring about case or whitespace in property names
        /// </summary>
        /// <param name="src">NameValueCollection to operate on</param>
        /// <param name="throwOnError">Flag: throw an exception if cannot duck instead of just returning null</param>
        /// <typeparam name="T">Interface you wish to work through</typeparam>
        /// <returns>New instance of an object implementing the required interface, if possible</returns>
        public static T FuzzyDuckAs<T>(this NameValueCollection src, bool throwOnError)
            where T : class
        {
            return src
                .ToDictionary()
                .FuzzyDuckAs<T>(throwOnError);
        }
    }
}