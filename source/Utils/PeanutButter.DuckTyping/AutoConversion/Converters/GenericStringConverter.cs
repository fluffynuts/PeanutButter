using System;
using System.Linq;
using System.Reflection;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping.AutoConversion.Converters
{
    internal class GenericStringConverter<T> 
        : GenericConverterBase<T>, 
          IConverter<string, T>
    {
        public Type T1 => typeof(string);
        public Type T2 => typeof(T);

        public T Convert(string value)
        {
            var parameters = new object[] {value, null};
            var parsed = (bool) _tryParse.Invoke(null, parameters);
            if (parsed)
            {
                return (T) parameters[1];
            }

            return default(T);
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
    }

    internal abstract class GenericConverterBase<T>
    {
        protected readonly MethodInfo _tryParse = GetTryParseMethod();
        private static MethodInfo GetTryParseMethod()
        {
            return typeof(T)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(mi => mi.IsTryParseMethod());
        }
    }

    internal class GenericNullableStringConverter<T>
        : GenericConverterBase<T>,
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
            var parsed = (bool)_tryParse.Invoke(null, parameters);
            return parsed
                ? (T) parameters[1]
                : null as T?;
        }
    }
}