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

    internal class GenericStringArrayConverter<T>
        : GenericConverterBase<T>,
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

    internal abstract class GenericConverterBase<T>: ConverterBase
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
            var parsed = (bool) _tryParse.Invoke(null, parameters);
            return parsed
                ? (T) parameters[1]
                : null as T?;
        }

        public bool CanConvert(Type t1, Type t2)
        {
            return CanConvert(t1, t2, T1, T2);
        }
    }
}