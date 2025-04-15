using System;
using System.Collections.Generic;
using Imported.PeanutButter.Utils;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.AutoConversion.Converters;
#else
namespace PeanutButter.DuckTyping.AutoConversion.Converters;
#endif
internal class GenericStringConverter<T>
    : GenericStringConverterBase<T>,
      IConverter<string, T>
{
    public Type T1 => typeof(string);
    public Type T2 => typeof(T);


    public virtual T Convert(string value)
    {
        var parameters = new object[]
        {
            value,
            null
        };
        var parsed = (bool)_tryParse.Invoke(null, parameters);
        return parsed
            ? TweakResult(parameters[1])
            : default;
    }

    private T TweakResult(object result)
    {
        // some types may require a little manual manipulation
        if (result is DateTime { Kind: DateTimeKind.Unspecified } dt)
        {
            result = dt.WithKind(DateTimeKind.Local);
        }
        return (T)result;
    }

    public virtual string Convert(T value)
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

    public virtual bool CanConvert(Type t1, Type t2)
    {
        return CanConvert(t1, t2, T1, T2);
    }
}

internal class GenericStringToDecimalConverter : GenericStringConverter<decimal>
{
    public override decimal Convert(string value)
    {
        var dd = new DecimalDecorator(value);
        return dd.ToDecimal();
    }
}

internal class GenericStringToFloatConverter : GenericStringConverter<float>
{
    public override float Convert(string value)
    {
        var dd = new DecimalDecorator(value);
        return (float)dd.ToDecimal();
    }
}

internal class GenericStringToDoubleConverter : GenericStringConverter<double>
{
    public override double Convert(string value)
    {
        var dd = new DecimalDecorator(value);
        return (double)dd.ToDecimal();
    }
}

internal class GenericStringToBoolConverter : GenericStringConverter<bool>
{
    public override bool Convert(string value)
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

internal class GenericStringToEnumConverter<TEnum> : GenericStringConverter<TEnum>
    where TEnum : struct
{
    public override string Convert(TEnum value)
    {
        return $"{value}";
    }

    public override TEnum Convert(string value)
    {
        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var result)
            ? result
            : default;
    }
}

internal class GenericStringToNullableEnumConverter<TEnum> : GenericStringConverter<TEnum?>
    where TEnum : struct
{
    public override string Convert(TEnum? value)
    {
        return value is null
            ? null
            : $"{value}";
    }

    public override TEnum? Convert(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<TEnum>(value, ignoreCase: true, out var result)
            ? result
            : default;
    }
}