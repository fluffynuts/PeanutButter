using System.Collections.Generic;
using System.Collections.Specialized;
using PeanutButter.DuckTyping.Shimming;
using Shared = PeanutButter.DuckTyping.Extensions.DuckTypingExtensionSharedMethods;
using TransformFunc = System.Func<string, string>;

namespace PeanutButter.DuckTyping.Extensions
{
    /// <summary>
    /// Provides duck-typing extension methods around dictionary objects
    /// </summary>
    public static class DuckTypingDictionaryExtensions
    {
        /// <summary>
        /// Forces approximate ducking around a dictionary. Will "create" underlying
        /// "properties" as required. Will attempt to convert to and from the underlying
        /// types as required. Will match properties case-insensitive.
        /// </summary>
        /// <param name="src"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ForceFuzzyDuckAs<T>(this IDictionary<string, object> src)
        {
            return Shared.ForceDuckAs<T>(src, true, true);
        }

        /// <summary>
        /// Forces approximate ducking around a dictionary. Will "create" underlying
        /// "properties" as required. Will attempt to convert to and from the underlying
        /// types as required. Will match properties case-insensitive.
        /// </summary>
        /// <param name="src"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T ForceFuzzyDuckAs<T>(this object src)
        {
            return Shared.ForceDuckAs<T>(
                src,
                true,
                true);
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
            return Shared.ForceDuckAs<T>(src, false, true);
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
            this IDictionary<string, object> src,
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
            this IDictionary<string, object> src,
            TransformFunc toNativeTransform,
            TransformFunc fromNativeTransform,
            bool throwOnError
        ) where T : class
        {
            var redirector = new RedirectingDictionary<object>(
                src,
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
            this IDictionary<string, object> src,
            TransformFunc toNativeTransform,
            TransformFunc fromNativeTransform
        ) where T : class
        {
            return src.FuzzyDuckAs<T>(toNativeTransform, fromNativeTransform, false);
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
            this IDictionary<string, object> src,
            TransformFunc toNativeTransform,
            TransformFunc fromNativeTransform,
            bool throwOnError
        ) where T : class
        {
            var redirector = new RedirectingDictionary<object>(
                src,
                toNativeTransform,
                fromNativeTransform
            );
            return redirector.FuzzyDuckAs<T>(throwOnError);
        }

        /// <summary>
        /// Fuzzy-Ducks a Dictionary (or part thereof) using a key prefix
        /// to allow "namespacing" of config
        /// </summary>
        /// <param name="src">IDictionary to operate on</param>
        /// <param name="keyPrefix">key prefix to take into account</param>
        /// <typeparam name="T">Interface to duck onto</typeparam>
        /// <returns>new instance of T, wrapping the Dictionary, or null if unduckable</returns>
        public static T FuzzyDuckAs<T>(
            this IDictionary<string, object> src,
            string keyPrefix
        ) where T : class
        {
            return src.FuzzyDuckAs<T>(keyPrefix, false);
        }

        /// <summary>
        /// Fuzzy-Ducks a Dictionary (or part thereof) using a key prefix
        /// to allow "namespacing" of config
        /// </summary>
        /// <param name="src">IDictionary to operate on</param>
        /// <param name="keyPrefix">key prefix to take into account</param>
        /// <param name="throwOnError">flag: when unduckable, throw instead of just returning null</param>
        /// <typeparam name="T">Interface to duck onto</typeparam>
        /// <returns>new instance of T, wrapping the Dictionary, or null if unduckable</returns>
        public static T FuzzyDuckAs<T>(
            this IDictionary<string, object> src,
            string keyPrefix,
            bool throwOnError
        ) where T : class
        {
            return src.FuzzyDuckAs<T>(
                k => $"{keyPrefix}{k}",
                k => k.RemoveAtStart(keyPrefix),
                throwOnError
            );
        }

        /// <summary>
        /// Ducks a Dictionary (or part thereof) using a key prefix
        /// to allow "namespacing" of config
        /// </summary>
        /// <param name="src">IDictionary to operate on</param>
        /// <param name="keyPrefix">key prefix to take into account</param>
        /// <typeparam name="T">Interface to duck onto</typeparam>
        /// <returns>new instance of T, wrapping the Dictionary, or null if unduckable</returns>
        public static T DuckAs<T>(
            this IDictionary<string, object> src,
            string keyPrefix
        ) where T : class
        {
            return src.DuckAs<T>(keyPrefix, false);
        }

        /// <summary>
        /// Ducks a Dictionary (or part thereof) using a key prefix
        /// to allow "namespacing" of config
        /// </summary>
        /// <param name="src">IDictionary to operate on</param>
        /// <param name="keyPrefix">key prefix to take into account</param>
        /// <param name="throwOnError">flag: when unduckable, throw instead of just returning null</param>
        /// <typeparam name="T">Interface to duck onto</typeparam>
        /// <returns>new instance of T, wrapping the Dictionary, or null if unduckable</returns>
        public static T DuckAs<T>(
            this IDictionary<string, object> src,
            string keyPrefix,
            bool throwOnError
        ) where T : class
        {
            return src.DuckAs<T>(
                k => $"{keyPrefix}{k}",
                k => k.RemoveAtStart(keyPrefix),
                throwOnError
            );
        }

        /// <summary>
        /// Fuzzy-Ducks a NameValueCollection (or part thereof) using a key prefix
        /// to allow "namespacing" of config
        /// </summary>
        /// <param name="src">NameValueCollection to operate on</param>
        /// <param name="keyPrefix">key prefix to take into account</param>
        /// <typeparam name="T">Interface to duck onto</typeparam>
        /// <returns>new instance of T, wrapping the NameValueCollection, or null if unduckable</returns>
        public static T FuzzyDuckAs<T>(
            this NameValueCollection src,
            string keyPrefix
        ) where T : class
        {
            return src.FuzzyDuckAs<T>(keyPrefix, false);
        }

        /// <summary>
        /// Fuzzy-Ducks a NameValueCollection (or part thereof) using a key prefix
        /// to allow "namespacing" of config
        /// </summary>
        /// <param name="src">NameValueCollection to operate on</param>
        /// <param name="keyPrefix">key prefix to take into account</param>
        /// <param name="throwOnError">flag: when unduckable, throw instead of just returning null</param>
        /// <typeparam name="T">Interface to duck onto</typeparam>
        /// <returns>new instance of T, wrapping the NameValueCollection, or null if unduckable</returns>
        public static T FuzzyDuckAs<T>(
            this NameValueCollection src,
            string keyPrefix,
            bool throwOnError
        ) where T : class
        {
            return src.FuzzyDuckAs<T>(
                k => $"{keyPrefix}{k}",
                k => k.RemoveAtStart(keyPrefix),
                throwOnError
            );
        }

        /// <summary>
        /// Ducks a NameValueCollection (or part thereof) using a key prefix
        /// to allow "namespacing" of config
        /// </summary>
        /// <param name="src">NameValueCollection to operate on</param>
        /// <param name="keyPrefix">key prefix to take into account</param>
        /// <typeparam name="T">Interface to duck onto</typeparam>
        /// <returns>new instance of T, wrapping the NameValueCollection, or null if unduckable</returns>
        public static T DuckAs<T>(
            this NameValueCollection src,
            string keyPrefix
        ) where T : class
        {
            return src.DuckAs<T>(keyPrefix, false);
        }

        /// <summary>
        /// Ducks a NameValueCollection (or part thereof) using a key prefix
        /// to allow "namespacing" of config
        /// </summary>
        /// <param name="src">NameValueCollection to operate on</param>
        /// <param name="keyPrefix">key prefix to take into account</param>
        /// <param name="throwOnError">flag: when unduckable, throw instead of just returning null</param>
        /// <typeparam name="T">Interface to duck onto</typeparam>
        /// <returns>new instance of T, wrapping the NameValueCollection, or null if unduckable</returns>
        public static T DuckAs<T>(
            this NameValueCollection src,
            string keyPrefix,
            bool throwOnError
        ) where T : class
        {
            return src.DuckAs<T>(
                k => $"{keyPrefix}{k}",
                k => k.RemoveAtStart(keyPrefix),
                throwOnError
            );
        }

        private static string RemoveAtStart(this string src, string prefix)
        {
            return src.StartsWith(prefix)
                ? src.Substring(prefix.Length)
                : src;
        }
    }
}