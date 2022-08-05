using System;
using System.Linq;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.AutoConversion.Converters
#else
namespace PeanutButter.DuckTyping.AutoConversion.Converters
#endif
{
    internal class GenericStringStringArrayConverter<T>
        : GenericStringConverterBase<T>,
          IConverter<string[], T[]>
    {
        public Type T1 => typeof(string[]);
        public Type T2 => typeof(T[]);

        public T[] Convert(string[] input)
        {
            return input.Select(
                s =>
                {
                    var parameters = new object[] { s, null };
                    var parsed = (bool) _tryParse.Invoke(null, parameters);
                    return parsed
                        ? (T) parameters[1]
                        : default;
                }).ToArray();
        }

        public string[] Convert(T[] input)
        {
            return input
                .Select(o => o.ToString())
                .ToArray();
        }

        public bool CanConvert(Type t1, Type t2)
        {
            return CanConvert(t1, t2, T1, T2);
        }
    }
}