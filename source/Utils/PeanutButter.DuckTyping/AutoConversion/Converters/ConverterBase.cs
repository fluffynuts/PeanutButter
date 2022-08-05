using System;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.AutoConversion.Converters
#else
namespace PeanutButter.DuckTyping.AutoConversion.Converters
#endif
{
    internal abstract class ConverterBase
    {
        protected bool CanConvert(
            Type from,
            Type to,
            Type t1,
            Type t2
        )
        {
            return (from == t1 || from == t2) &&
                (to == t1 || to == t2) &&
                (from != to);
        }
    }
}