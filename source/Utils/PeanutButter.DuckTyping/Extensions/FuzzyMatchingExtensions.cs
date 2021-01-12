using System.Linq;
using System.Text.RegularExpressions;
using Imported.PeanutButter.Utils;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.Extensions
#else
namespace PeanutButter.DuckTyping.Extensions
#endif
{
    internal static class FuzzyMatchingExtensions
    {
        private static string ReplaceAll(
            this string src,
            string[] search,
            string replaceWith
        )
        {
            return search.Aggregate(src, (acc, cur) => src.Replace(cur, replaceWith));
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
    }
}