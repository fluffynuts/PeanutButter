using System;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.AutoConversion.Converters
#else
namespace PeanutButter.DuckTyping.AutoConversion.Converters
#endif
{
    internal class GenericNullableStringConverter<T>
        : GenericStringConverterBase<T>,
          IConverter<string, T?> where T : struct
    {
        public Type T1 => typeof(string);
        public Type T2 => typeof(T?);

        public string Convert(T? input)
        {
            return input?.ToString();
        }

        public T? Convert(string value)
        {
            var parameters = new object[] { value, null };
            var parsed = (bool) _tryParse.Invoke(null, parameters);
            return parsed
                ? (T) parameters[1]
                : null as T?;
        }

        public bool CanConvert(Type t1, Type t2)
        {
            return _tryParse is not null && CanConvert(t1, t2, T1, T2);
        }
    }
}