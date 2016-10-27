using System;

namespace PeanutButter.DuckTyping
{
    internal static class Comparers
    {
        public static StringComparer NonFuzzyComparer => StringComparer.InvariantCulture;
        public static StringComparer FuzzyComparer => StringComparer.OrdinalIgnoreCase;
    }
}