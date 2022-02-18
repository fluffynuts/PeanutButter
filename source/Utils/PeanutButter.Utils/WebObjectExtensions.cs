﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PeanutButter.Utils.Dictionaries;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides some more object extensions, for webby usages
    /// </summary>
    public static class WebObjectExtensions
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
                    $"{HttpEncoder.UrlEncode((string)cur.Key)}={HttpEncoder.UrlEncode((string)cur.Value?.ToString())}"
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

        /// <summary>
        /// Attempts to provide a dictionary representation for the provided
        /// object. If the provided object already implements IDictionary&lt;TKey, TValue&gt;
        /// then you'll get the same instance back - be careful to clone it if you don't
        /// want to mutate the original
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static IDictionary<TKey, TValue> AsDictionary<TKey, TValue>(
            this object obj
        )
        {
            switch (obj)
            {
                case null:
                    return new Dictionary<TKey, TValue>();
                case IDictionary<TKey, TValue> dict:
                    return dict;
                case IDictionary dict:
                    return dict.ToDictionary<TKey, TValue>();
                default:
                    var type = obj.GetType();
                    if (type.IsPrimitiveOrImmutable())
                    {
                        throw new NotSupportedException(
                            $"Cannot convert object of type {type} to a dictionary"
                        );
                    }

                    if (typeof(TKey) == typeof(string) &&
                        typeof(TValue) == typeof(object))
                    {
                        return new DictionaryWrappingObject(obj)
                            as IDictionary<TKey, TValue>;
                    }

                    throw new NotSupportedException(
                        "Arbitrary objects may only be represented by Dictionary<string, object>"
                    );
            }
        }
    }
}