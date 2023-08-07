using System.Collections.Generic;
using System.Collections.Specialized;
using Imported.PeanutButter.Utils.Dictionaries;
using TransformFunc = System.Func<string, string>;

// ReSharper disable MemberCanBePrivate.Global

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.Extensions
#else
namespace PeanutButter.DuckTyping.Extensions
#endif
{
    /// <summary>
    /// Provides duck-typing around NameValueCollections
    /// </summary>
#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
    internal
#else
    public
#endif
        static class DuckTypingNameValueCollectionExtensions
    {
        internal static IDictionary<string, object> ToDictionary(
            this NameValueCollection src,
            bool caseInsensitive = false
        )
        {
            return new DictionaryWrappingNameValueCollection<object>(src, caseInsensitive);
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

        /// <summary>
        /// Forces ducking onto the NameValueCollection. Missing properties return default values when queried and can be set with new values.
        /// </summary>
        /// <param name="src">NameValueCollection to operate on</param>
        /// <typeparam name="T">Interface to duck to</typeparam>
        /// <returns>New instance of an object implementing the required interface, if possible</returns>
        public static T ForceDuckAs<T>(this NameValueCollection src)
        {
            return src.ToDictionary().ForceDuckAs<T>();
        }

        /// <summary>
        /// Forces fuzzy ducking onto the NameValueCollection. Missing properties return default values when queried and can be set with new values.
        /// </summary>
        /// <param name="src">NameValueCollection to operate on</param>
        /// <typeparam name="T">Interface to duck to</typeparam>
        /// <returns>New instance of an object implementing the required interface, if possible</returns>
        public static T ForceFuzzyDuckAs<T>(this NameValueCollection src)
        {
            return src.ToDictionary(true).ForceFuzzyDuckAs<T>();
        }

        /// <summary>
        /// Performs ducking with property name redirection
        /// </summary>
        /// <param name="src">Dictionary to duck</param>
        /// <param name="toNativeTransform">Func to transform from keys corresponding to T's interface properties to keys that are found in src</param>
        /// <param name="fromNativeTransform">Reverse of toNativeTransform</param>
        /// <typeparam name="T">Interface to duck this dictionary as</typeparam>
        /// <returns>New instance of an object implementing T, passing through to the dictionary, or null if unable to duck</returns>
        public static T DuckAs<T>(
            this NameValueCollection src,
            TransformFunc toNativeTransform,
            TransformFunc fromNativeTransform
        ) where T : class
        {
            return src.DuckAs<T>(toNativeTransform, fromNativeTransform, false);
        }

        /// <summary>
        /// Performs ducking with property name redirection
        /// </summary>
        /// <param name="src">Dictionary to duck</param>
        /// <param name="toNativeTransform">Func to transform from keys corresponding to T's interface properties to keys that are found in src</param>
        /// <param name="fromNativeTransform">Reverse of toNativeTransform</param>
        /// <param name="throwOnError">Flag to throw exception on error instead of just silent failure</param>
        /// <typeparam name="T">Interface to duck this dictionary as</typeparam>
        /// <returns>New instance of an object implementing T, passing through to the dictionary, or null if unable to duck</returns>
        public static T DuckAs<T>(
            this NameValueCollection src,
            TransformFunc toNativeTransform,
            TransformFunc fromNativeTransform,
            bool throwOnError
        ) where T : class
        {
            var redirector = new RedirectingDictionary<object>(
                new DictionaryWrappingNameValueCollection<object>(src, false),
                toNativeTransform,
                fromNativeTransform
            );
            return redirector.DuckAs<T>(throwOnError);
        }

        /// <summary>
        /// Performs fuzzy ducking with property name redirection
        /// </summary>
        /// <param name="src">Dictionary to duck</param>
        /// <param name="toNativeTransform">Func to transform from keys corresponding to T's interface properties to keys that are found in src</param>
        /// <param name="fromNativeTransform">Reverse of toNativeTransform</param>
        /// <typeparam name="T">Interface to duck this dictionary as</typeparam>
        /// <returns>New instance of an object implementing T, passing through to the dictionary, or null if unable to duck</returns>
        public static T FuzzyDuckAs<T>(
            this NameValueCollection src,
            TransformFunc toNativeTransform,
            TransformFunc fromNativeTransform
        ) where T : class
        {
            return src.FuzzyDuckAs<T>(
                toNativeTransform,
                fromNativeTransform,
                false
            );
        }

        /// <summary>
        /// Performs fuzzy ducking with property name redirection
        /// </summary>
        /// <param name="src">Dictionary to duck</param>
        /// <param name="toNativeTransform">Func to transform from keys corresponding to T's interface properties to keys that are found in src</param>
        /// <param name="fromNativeTransform">Reverse of toNativeTransform</param>
        /// <param name="throwOnError">Flag to throw exception on error instead of just silent failure</param>
        /// <typeparam name="T">Interface to duck this dictionary as</typeparam>
        /// <returns>New instance of an object implementing T, passing through to the dictionary, or null if unable to duck</returns>
        public static T FuzzyDuckAs<T>(
            this NameValueCollection src,
            TransformFunc toNativeTransform,
            TransformFunc fromNativeTransform,
            bool throwOnError
        ) where T : class
        {
            var redirector = new RedirectingDictionary<object>(
                new DictionaryWrappingNameValueCollection<object>(src, false),
                toNativeTransform,
                fromNativeTransform
            );
            return redirector.FuzzyDuckAs<T>(throwOnError);
        }
    }
}