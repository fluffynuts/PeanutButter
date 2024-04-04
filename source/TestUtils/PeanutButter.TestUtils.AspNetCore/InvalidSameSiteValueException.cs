using System;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore;
#else
namespace PeanutButter.TestUtils.AspNetCore;
#endif

/// <summary>
/// Thrown when an invalid value for SameSite is found on a cookie header
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class InvalidSameSiteValueException : Exception
{
    /// <summary>
    /// Constructs the exception with the erroneous value
    /// </summary>
    /// <param name="value"></param>
    public InvalidSameSiteValueException(string value) :
        base($"Unable to parse '{value}' to a valid SameSite value (Strict, Lax, or None)")
    {
    }
}