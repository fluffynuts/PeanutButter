using System;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.RandomGenerators;
#else
namespace PeanutButter.RandomGenerators;
#endif

/// <summary>
/// Exception thrown when the method GetAnother is unable to find another
/// random value different from the exclusion value specified
/// </summary>
/// <typeparam name="T"></typeparam>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class CannotGetAnotherDifferentRandomValueException<T> : Exception
{
    /// <summary>
    /// Value which was specified to be avoided
    /// </summary>
    public T Value { get; private set; }

    /// <summary>
    /// Constructs a new instance of the exception, storing the unwanted value
    /// </summary>
    /// <param name="unwantedValue">Value which was to be avoided when generating a new value</param>
    public CannotGetAnotherDifferentRandomValueException(T unwantedValue) :
        base(
            $"Unable to get a value different from {unwantedValue} after {RandomValueGen.MAX_DIFFERENT_RANDOM_VALUE_ATTEMPTS} attempts )':"
        )
    {
        Value = unwantedValue;
    }
}