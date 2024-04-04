using System;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore;
#else
namespace PeanutButter.TestUtils.AspNetCore;
#endif

/// <summary>
/// Thrown when unable to cast an abstract type to the faked implementation
/// </summary>
/// <typeparam name="TExpected"></typeparam>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class InvalidImplementationException<TExpected> : Exception
{
    /// <inheritdoc />
    public InvalidImplementationException(
        object implementation
    ) : base(GenerateMessageFor<TExpected>(implementation))
    {
    }

    private static string GenerateMessageFor<T>(
        object implementation
    )
    {
        var expected = typeof(T);
        return implementation is null
            ? $"implementation is null for expected type {expected}"
            : $"invalid implementation {implementation} for expected type {expected}";
    }
}