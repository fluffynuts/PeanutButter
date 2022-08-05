using System;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.Comparers
#else
namespace PeanutButter.DuckTyping.Comparers
#endif
{
    internal static class Comparers
    {
        public static StringComparer NonFuzzyComparer => StringComparer.InvariantCulture;
        public static StringComparer FuzzyComparer => StringComparer.OrdinalIgnoreCase;
    }
}