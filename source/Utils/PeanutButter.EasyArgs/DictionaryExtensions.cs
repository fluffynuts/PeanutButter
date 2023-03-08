using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PeanutButter.Utils;

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

        internal static string FuzzyFindKeyFor<T>(
            this IDictionary<string, T> data,
            string propertyName
        )
        {
            if (data is null || propertyName is null)
            {
                return null;
            }

            return ExactKeyMatch(data, propertyName)
                ?? CaseInsensitiveMatch(data, propertyName)
                ?? FuzzyCaseSensitiveMatch(data, propertyName)
                ?? LooseFuzzyCaseSensitiveMatch(data, propertyName)
                ?? FuzzyCaseInsensitiveMatch(data, propertyName)
                ?? LooseFuzzyCaseInsensitiveMatch(data, propertyName);
        }

        private static string ExactKeyMatch<T>(
            IDictionary<string, T> data,
            string search
        )
        {
            return data.ContainsKey(search)
                ? search
                : null;
        }

        private static string CaseInsensitiveMatch<T>(
            IDictionary<string, T> data,
            string search
        )
        {
            return data.Keys.FirstOrDefault(
                k => k.Equals(search, StringComparison.InvariantCultureIgnoreCase)
            );
        }

        private static string FuzzyCaseInsensitiveMatch<T>(
            IDictionary<string, T> data,
            string search
        )
        {
            return FuzzySearch.Aggregate(
                null as string,
                (acc, cur) =>
                    acc ?? data.Keys.FirstOrDefault(
                        k => k.FuzzyMatches(search, cur)
                    )
            );
        }

        private static string FuzzyCaseSensitiveMatch<T>(
            IDictionary<string, T> data,
            string search
        )
        {
            return FuzzySearch.Aggregate(
                null as string,
                (acc, cur) =>
                    acc ?? data.Keys.FirstOrDefault(
                        k => k.FuzzyCaseSensitiveMatches(search, cur)
                    )
            );
        }

        internal static bool FuzzyCaseSensitiveMatches(
            this string src,
            string other,
            params string[] ignoreFragments
        )
        {
            var left = src?.ReplaceAll(ignoreFragments, "");
            var right = other?.ReplaceAll(ignoreFragments, "");
            return left == right;
        }

        private static string LooseFuzzyCaseInsensitiveMatch<T>(
            IDictionary<string, T> data,
            string search
        )
        {
            return data.Keys.FirstOrDefault(
                k => k.FuzzyMatches(search, NotAlphaNumeric)
            );
        }

        internal static bool FuzzyMatches(
            this string src,
            string other,
            params Regex[] ignorePatterns
        )
        {
            var left = src?.RegexReplaceAll("", ignorePatterns)?.ToLowerInvariant();
            var right = other?.RegexReplaceAll("", ignorePatterns)?.ToLowerInvariant();
            return left == right;
        }

        private static string LooseFuzzyCaseSensitiveMatch<T>(
            IDictionary<string, T> data,
            string search
        )
        {
            return data.Keys.FirstOrDefault(
                k => k.FuzzyCaseSensitiveMatches(search, NotAlphaNumeric)
            );
        }

        private static readonly Regex NotAlphaNumeric = new Regex("[^a-zA-Z0-9]");

        internal static bool FuzzyCaseSensitiveMatches(
            this string src,
            string other,
            params Regex[] ignorePatterns
        )
        {
            var left = src?.RegexReplaceAll("", ignorePatterns);
            var right = other?.RegexReplaceAll("", ignorePatterns);
            return left == right;
        }

        private static readonly string[] FuzzySearch = new[]
        {
            " ",
            ".",
            ":",
            "_",
            "-"
        };

        internal static bool FuzzyMatches(
            this string src,
            string other,
            params string[] ignoreFragments
        )
        {
            var left = src?.ReplaceAll(ignoreFragments, "")?.ToLowerInvariant();
            var right = other?.ReplaceAll(ignoreFragments, "")?.ToLowerInvariant();
            return left == right;
        }
    }
}