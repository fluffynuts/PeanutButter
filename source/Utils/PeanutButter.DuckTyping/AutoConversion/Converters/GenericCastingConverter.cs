using System;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.AutoConversion.Converters
#else
namespace PeanutButter.DuckTyping.AutoConversion.Converters
#endif
{
    internal class GenericCastingConverter<T1, T2>
        : IConverter<T1, T2>
    {
        public bool IsInitialised => true;
        private static readonly Type T1Type = typeof(T1);
        private static readonly Type T2Type = typeof(T2);
        
        public bool CanConvert(Type t1, Type t2)
        {
            return (t1 == T1Type || t1 == T2Type) &&
                (t2 == T1Type || t2 == T2Type);
        }

        public T1 Convert(T2 input)
        {
            return (T1) System.Convert.ChangeType(input, typeof(T1));
        }

        public T2 Convert(T1 input)
        {
            return (T2) System.Convert.ChangeType(input, typeof(T2));
        }
    }
}