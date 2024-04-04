using System;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore;
#else
namespace PeanutButter.TestUtils.AspNetCore;
#endif

/// <summary>
/// Thrown when querying cookie information by
/// cookie name, when that cookie is not found
/// on the hosting request or response
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class CookieNotFoundException : Exception
{
    /// <summary>
    /// Constructs the exception
    /// </summary>
    /// <param name="cookieName"></param>
    public CookieNotFoundException(
        string cookieName
    ) : base($"The cookie '{cookieName}' was not found")
    {
    }
}