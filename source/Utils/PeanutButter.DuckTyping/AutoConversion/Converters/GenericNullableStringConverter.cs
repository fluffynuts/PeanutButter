using System;
using System.Collections.Generic;

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

        public virtual string Convert(T? input)
        {
            return input?.ToString();
        }

        public virtual T? Convert(string value)
        {
            var parameters = new object[]
            {
                value,
                null
            };
            var parsed = (bool)_tryParse.Invoke(null, parameters);
            return parsed
                ? (T)parameters[1]
                : null as T?;
        }

        public virtual bool CanConvert(Type t1, Type t2)
        {
            return _tryParse is not null && CanConvert(t1, t2, T1, T2);
        }
    }

    internal class GenericNullableStringToBooleanConverter
        : GenericNullableStringConverter<bool>
    {
        public override bool? Convert(string value)
        {
            if (TruthyValues.Contains(value))
            {
                return true;
            }

            if (FalsyValues.Contains(value))
            {
                return false;
            }

            return base.Convert(value);
        }

        private static readonly HashSet<string> TruthyValues = new(
            new[]
            {
                "true",
                "yes",
                "1",
                "enabled"
            },
            StringComparer.OrdinalIgnoreCase
        );

        private static readonly HashSet<string> FalsyValues = new(
            new[]
            {
                "false",
                "no",
                "0",
                "disabled"
            },
            StringComparer.OrdinalIgnoreCase
        );
    }
}