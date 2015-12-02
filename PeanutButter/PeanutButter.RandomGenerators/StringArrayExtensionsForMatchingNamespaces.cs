using System;
using System.Linq;

namespace PeanutButter.RandomGenerators
{
    internal static class StringArrayExtensionsForMatchingNamespaces
    {
        public static int MatchIndexFor(this string[] src, string[] other)
        {
            var shortest = Math.Min(src.Length, other.Length);
            var score = 0;
            for (var i = 0; i < shortest; i++)
            {
                score += strcmp(src[i], other[i]);
            }
            score += (src.Length - other.Length);
            return score;
        }

        private static int strcmp(string first, string second)
        {
            if (first == second)
                return 0;
            var ordered = new[] {first, second}.OrderBy(s => s);
            return ordered.First() == first ? -1 : 1;
        }
    }
}