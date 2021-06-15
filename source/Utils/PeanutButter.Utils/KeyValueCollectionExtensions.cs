using System.Collections.Generic;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides extension methods on KeyValuePairCollections
    /// </summary>
    public static class KeyValueCollectionExtensions
    {
        /// <summary>
        /// Resolves a KeyValuePair of string/string to a KeyValuePair of string/object
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static KeyValuePair<string, object> AsKeyValuePairOfStringObject(
            this KeyValuePair<string, string> src
        )
        {
            return new KeyValuePair<string, object>(
                src.Key,
                src.Value
            );
        }
    }
}