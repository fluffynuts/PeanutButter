using System.Linq;

namespace PeanutButter.DuckTyping.Extensions
{
    internal static class StringExtensionsForFuzzyKeyMatching
    {
        private static string ReplaceAll(this string src, string[] search, string replaceWith)
        {
            return search.Aggregate(src, (acc, cur) => src.Replace(cur, replaceWith));
        }

        internal static bool FuzzyMatches(this string src, string other, params string[] ignoreFragments)
        {
            var left = src?.ReplaceAll(ignoreFragments, "")?.ToLowerInvariant();
            var right = other?.ReplaceAll(ignoreFragments, "")?.ToLowerInvariant();
            return left == right;
        }
    }
}