using System;

namespace PeanutButter.DuckTyping.Comparers
{
    internal static class Comparers
    {
        public static StringComparer NonFuzzyComparer => StringComparer.InvariantCulture;
        public static StringComparer FuzzyComparer => StringComparer.OrdinalIgnoreCase;
    }
}