using System;
using Microsoft.AspNetCore.Http;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore;
#else
namespace PeanutButter.TestUtils.AspNetCore;
#endif

/// <summary>
/// Provides convenience HttpRequestExtensions
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    static class HttpRequestExtensions
{
    /// <summary>
    /// Groks the full url for an HttpRequest into an Uri
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static Uri FullUrl(
        this HttpRequest request
    )
    {
        return new Uri(
            $@"{
                request.Scheme
            }://{
                request.Host
            }{
                request.Path
            }{request.QueryString}"
        );
    }

    /// <summary>
    /// Reads the SameSite attribute for a cookie from the request headers,
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