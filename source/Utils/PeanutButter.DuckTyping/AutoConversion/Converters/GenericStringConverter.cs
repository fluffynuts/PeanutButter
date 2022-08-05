using System;
using System.Collections.Generic;
using Imported.PeanutButter.Utils;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.AutoConversion.Converters
#else
namespace PeanutButter.DuckTyping.AutoConversion.Converters
#endif
{
    internal class GenericStringConverter<T>
        : GenericStringConverterBase<T>,
          IConverter<string, T>
    {
        public Type T1 => typeof(string);
        public Type T2 => typeof(T);

        public T Convert(string value)
        {
            var parameters = new object[] { value, null };
            var parsed = (bool) _tryParse.Invoke(null, parameters);
            return parsed
                ? (T) parameters[1]
                : default;
        }

        public string Convert(T value)
        {
            try
            {
                return value.ToString();
            }
            catch
            {
                return null;
            }
        }

        public bool CanConvert(Type t1, Type t2)
        {
            return CanConvert(t1, t2, T1, T2);
        }
    }
}