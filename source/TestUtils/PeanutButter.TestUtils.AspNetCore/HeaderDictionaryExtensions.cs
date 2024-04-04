using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore;
#else
namespace PeanutButter.TestUtils.AspNetCore;
#endif
/// <summary>
/// Provides extensions for the IHeaderDictionary properties
/// on HttpRequest and HttpResponse
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    static class HeaderDictionaryExtensions
{
    /// <summary>
    /// Reads the SameSite attribute for a cookie from the headers,
    /// since System.Net.Cookie doesn't expose this
    /// </summary>
    /// <param name="headers"></param>
    /// <param name="cookieName"></param>
    /// <returns></returns>
    /// <exception cref="CookieNotFoundException"></exception>
    /// <exception cref="InvalidSameSiteValueException"></exception>
    public static SameSiteMode ReadSameSiteForCookie(
        this IHeaderDictionary headers,
        string cookieName
    )
    {
        var cookieHeader = headers.Where(
            h => h.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)
        ).Select(
            header => header.Value.FirstOrDefault(s => s.StartsWith($"{cookieName}="))
        ).FirstOrDefault();
        if (cookieHeader is null)
        {
            throw new CookieNotFoundException(cookieName);
        }

        var parts = TrimAll(cookieHeader.Split(";"));
        foreach (var part in parts)
        {
            var subs = TrimAll(part.Split('='));
            if (subs[0].Equals("SameSite", StringComparison.OrdinalIgnoreCase))
            {
                return Enum.TryParse<SameSiteMode>(subs[1], out var parsed)
                    ? parsed
                    : throw new InvalidSameSiteValueException(subs[1]);
            }
        }

        return SameSiteMode.None;
    }

    private static string[] TrimAll(IEnumerable<string> source)
    {
        return source.Select(s => s.Trim()).ToArray();
    }
}