using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore;
#else
namespace PeanutButter.TestUtils.AspNetCore;
#endif

/// <summary>
/// Provides extensions for asp.net HttpResponse objects
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    static class HttpResponseExtensions
{
    /// <summary>
    /// ASP.NET core HttpResponse objects don't provide
    /// easy read mechanisms for cookies, so this extension
    /// method produces a lazily-loaded collection of
    /// System.Net.Cookie instances from Set-Cookie headers
    /// </summary>
    /// <param name="res"></param>
    /// <returns></returns>
    public static IEnumerable<Cookie> ParseCookies(
        this HttpResponse res
    )
    {
        return res.Headers.ParseCookies();
    }

    /// <summary>
    /// Reads the SameSite attribute for a cookie from the response headers,
    /// since System.Net.Cookie doesn't expose this
    /// </summary>
    /// <param name="res"></param>
    /// <param name="cookieName"></param>
    /// <returns></returns>
    /// <exception cref="CookieNotFoundException"></exception>
    /// <exception cref="InvalidSameSiteValueException"></exception>
    public static SameSiteMode ReadSameSiteForCookie(
        this HttpResponse res,
        string cookieName
    )
    {
        return res.Headers.ReadSameSiteForCookie(cookieName);
    }


}