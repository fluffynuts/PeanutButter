using System;
using System.Linq;

namespace PeanutButter.FluentMigrator.MigrationDumping
{
    internal static class LocalStringExtensions
    {
        // I don't want to bring in a reliance on PB.Utils just for one method (yet)
        public static bool ContainsOneOf(this string haystack, params string[] needles)
        {
            if (needles.Length == 0)
                throw new ArgumentException("No needles provided to search haystack for");
            if (haystack == null)
                return false;
            haystack = haystack.ToLower();
            return needles.Any(s => haystack.Contains(s.ToLower()));
        }

        public static bool ContainsAllOf(this string haystack, params string[] needles)
        {
            if (needles.Length == 0)
                throw new ArgumentException("No needles provided to search haystack for");
            if (haystack == null)
                return false;
            haystack = haystack.ToLower();
            return needles.Any(s => haystack.Contains(s.ToLower()));
        }

        public static bool MultiContains(
            string haystack,
            string[] needles,
            Func<string, bool> operation
        )
        {

            if (needles.Length == 0)
                throw new ArgumentException("No needles provided to search haystack for");
            if (haystack == null)
                return false;
            haystack = haystack.ToLower();
            return operation(haystack);
        }
    }
}