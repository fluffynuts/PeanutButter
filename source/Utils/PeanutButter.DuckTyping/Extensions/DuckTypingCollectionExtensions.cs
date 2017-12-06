using System.Collections.Generic;
using System.Linq;

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.DuckTyping.Extensions
{
    /// <summary>
    /// Provides ducking extensions for collections
    /// </summary>
    public static class DuckTypingCollectionExtensions
    {
        /// <summary>
        /// Attempts to DuckType a collection
        /// </summary>
        /// <param name="src">Collection to convert</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] DuckAsArrayOf<T>(
            this IEnumerable<object> src
        ) where T: class
        {
            return src.DuckAsArrayOf<T>(false);
        }
        /// <summary>
        /// Attempts to DuckType a collection
        /// </summary>
        /// <param name="src">Collection to convert</param>
        /// <param name="throwOnFailure">Throw an exception when failing</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] DuckAsArrayOf<T>(
            this IEnumerable<object> src,
            bool throwOnFailure
        ) where T: class
        {
            return src?
                    .Select(o => o.DuckAs<T>(throwOnFailure))
                    .ToArray()
                    ?? new T[0];
        }
    }
}
