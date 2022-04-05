using System.Collections.Generic;
using PeanutButter.DuckTyping.Extensions;

#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs
#else
namespace PeanutButter.EasyArgs
#endif
{
    internal static class DictionaryExtensions
    {
        internal static bool TryGetValueFuzzy<T>(
            this IDictionary<string, T> dict,
            string key,
            out T value
        )
        {
            var matchedKey = dict.FuzzyFindKeyFor(key);
            if (matchedKey is null)
            {
                value = default;
                return false;
            }

            value = dict[matchedKey];
            return true;
        }
    }
}