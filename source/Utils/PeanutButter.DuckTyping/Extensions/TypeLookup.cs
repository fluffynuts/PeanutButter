using System;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.Extensions
#else
namespace PeanutButter.DuckTyping.Extensions
#endif
{
    internal class TypeLookup
    {
        public Type Type { get; set; }
        public Type FuzzyType { get; set; }
    }
}