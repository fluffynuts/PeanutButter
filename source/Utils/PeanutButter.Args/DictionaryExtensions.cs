using System.Collections.Generic;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.Args
{
    public static class DictionaryExtensions
    {
        public static bool TryGetValueFuzzy<T>(
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