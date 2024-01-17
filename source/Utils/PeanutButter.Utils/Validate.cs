using System;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils;
#else
namespace PeanutButter.Utils;
#endif
/// <summary>
/// Common validations; perhaps someday this should be made public?
/// </summary>
internal static class Validate
{
    public static Validator<T> That<T>(T value)
    {
        return That(value, nameof(value));
    }

    public static Validator<T> That<T>(T value, string parameterName)
    {
        return new Validator<T>(value, parameterName);
    }
}

internal class Validator<T>
{
    public T Value { get; }
    public string ParameterName { get; }

    public Validator(
        T value,
        string parameterName = null
    )
    {
        Value = value;
        ParameterName = parameterName ?? "value";
    }

    public ValidatorAnd<T> IsNotNull()
    {
        if (Value is null)
        {
            throw new ArgumentNullException(ParameterName);
        }

        return new ValidatorAnd<T>(Value);
    }
}

internal class ValidatorAnd<T> : Validator<T>
{
    public ValidatorAnd<T> And =>
        new ValidatorAnd<T>(Value);

    public ValidatorAnd(T value)
        : base(value)
    {
    }

    public Validator<TNext> That<TNext>(TNext value)
    {
        return That<TNext>(value, nameof(value));
    }

    public Validator<TNext> That<TNext>(
        TNext value,
        string parameterName
    )
    {
        return new Validator<TNext>(value, parameterName);
    }
}