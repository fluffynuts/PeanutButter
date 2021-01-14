using System;

namespace PeanutButter.DuckTyping.AutoConversion.Converters
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