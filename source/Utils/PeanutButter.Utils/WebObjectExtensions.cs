using System;
using System.Collections.Generic;
using System.Linq;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides some more object extensions, for webby usages
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class WebObjectExtensions
    {
        /// <summary>
        /// Provides a query string for the given object data
        /// - empty if the object is empty or null
        /// - with a preceding ? otherwise
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string AsQueryString(
            this object o
        )
        {
            var parameters = o.AsQueryStringParameters();
            return parameters == ""
                ? parameters
                : $"?{parameters}";
        }

        /// <summary>
        /// Provides query string parameters for the given object data
        /// - empty if the object is empty or null
        /// - everything _after_ the ? on an url otherwise
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string AsQueryStringParameters(
            this object o
        )
        {
            var dict = o.AsDictionary<string, object>();
            return dict.Aggregate(
                new List<string>() as IList<string>,
                (acc, cur) => acc.And(
                    $"{HttpEncoder.UrlEncode(cur.Key)}={HttpEncoder.UrlEncode(cur.Value?.ToString())}"
                )
            ).JoinWith("&");
        }

        /// <summary>
        /// Attempts to provide a dictionary representation for the provided
        /// object. If the provided object already is
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static IDictionary<string, object> AsDictionary(
            this object obj
        )
        {
            return obj.AsDictionary<string, object>();
        }
    }
}