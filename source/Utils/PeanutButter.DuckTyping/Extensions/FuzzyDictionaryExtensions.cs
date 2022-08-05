using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.Extensions
#else
namespace PeanutButter.DuckTyping.Extensions
#endif
{
    /// <summary>
    /// Provides extensions for dealing with dictionaries in a "fuzzy" manner
    /// </summary>
    public static class FuzzyDictionaryExtensions
    {
        private static readonly Func<IDictionary<string, object>, string, string>[] FuzzyKeyResolvers =
        {
            (dict, search) => dict.Keys.FirstOrDefault(k => k == search),
            (dict, search) => dict.Keys.FirstOrDefault(k => k.FuzzyMatches(search, " ")),
            (dict, search) => dict.Keys.FirstOrDefault(k => k.FuzzyMatches(search, ".")),
            (dict, search) => dict.Keys.FirstOrDefault(k => k.FuzzyMatches(search, ":")),
            (dict, search) => dict.Keys.FirstOrDefault(k => k.FuzzyMatches(search, "_")),
            (dict, search) => dict.Keys.FirstOrDefault(k => k.FuzzyMatches(search, "-")),
            (dict, search) => dict.Keys.FirstOrDefault(k => k.FuzzyMatches(search, NotAlphaNumeric))
        };

        private static readonly Regex NotAlphaNumeric = new Regex("[^a-zA-Z0-9]");

        /// <summary>
        /// Performs increasing levels of "fuzzy matching" for a key in a dictionary:
        /// - exact match
        /// - match case insensitive
        /// - match where elements may be separated by one of: space, period, colon, underscore, dash
        ///    - eg "object.property" or "some_value" or "class::member"
        /// - match based on an alphanumeric, case-insensitive basis
        /// </summary>
        /// <param name="data"></param>
        /// <param name="propertyName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string FuzzyFindKeyFor<T>(
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

        private static string LooseFuzzyCaseInsensitiveMatch<T>(
            IDictionary<string, T> data,
            string search
        )
        {
            return data.Keys.FirstOrDefault(
                k => k.FuzzyMatches(search, NotAlphaNumeric)
            );
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

        private static readonly string[] FuzzySearch = new[]
        {
            " ",
            ".",
            ":",
            "_",
            "-"
        };

        private static string FirstFuzzyKeyMatch<T>(
            IDictionary<string, T> dict,
            string search,
            string ignore
        )
        {
            return dict.Keys.FirstOrDefault(k => k.FuzzyMatches(search, ignore));
        }
    }
}