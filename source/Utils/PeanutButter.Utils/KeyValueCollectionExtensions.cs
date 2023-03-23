using System.Collections.Generic;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides extension methods on KeyValuePairCollections
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class KeyValueCollectionExtensions
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