using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
                return Enum.TryParse<SameSiteMode>(subs[1], ignoreCase: true, out var parsed)
                    ? parsed
                    : throw new InvalidSameSiteValueException(subs[1]);
            }
        }

        return SameSiteMode.None;
    }
    
    
    /// <summary>
    /// Parses cookies from an IHeaderDictionary
    /// </summary>
    /// <param name="headers"></param>
    /// <returns></returns>
    public static IEnumerable<Cookie> ParseCookies(
        this IHeaderDictionary headers
    )
    {
        return headers.Where(
                h => h.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)
            ).Select(
                h =>
                {
                    var values = h.Value.Select(s => s.Split(","))
                        .SelectMany(o => o)
                        .ToArray();
                    return values.Select(ParseCookieHeader);
                }
            )
            .SelectMany(o => o);
    }

    private static Cookie ParseCookieHeader(
        string header
    )
    {
        var parts = TrimAll(header.Split(';'));
        return parts.Aggregate(
            new Cookie(),
            (acc, cur) =>
            {
                var subs = cur.Split('=');
                var key = subs[0].Trim();
                var value = string.Join("=", subs.Skip(1));
                if (string.IsNullOrWhiteSpace(acc.Name))
                {
                    acc.Name = WebUtility.UrlDecode(key);
                    acc.Value = WebUtility.UrlDecode(value);
                }
                else
                {
                    if (CookieMutations.TryGetValue(key, out var modifier))
                    {
                        modifier(acc, value);
                    }
                }

                return acc;
            }
        );
    }


    private static readonly Dictionary<string, Action<Cookie, string>>
        CookieMutations = new(
            StringComparer.InvariantCultureIgnoreCase
        )
        {
            ["Expires"] = SetCookieExpiration,
            ["Max-Age"] = SetCookieMaxAgeToExpires,
            ["Domain"] = SetCookieDomain,
            ["Secure"] = SetCookieSecure,
            ["HttpOnly"] = SetCookieHttpOnly,
            ["Path"] = SetCookiePath
        };

    private static void SetCookiePath(
        Cookie cookie,
        string value
    )
    {
        cookie.Path = value;
    }

    private static void SetCookieHttpOnly(
        Cookie cookie,
        string value
    )
    {
        cookie.HttpOnly = true;
    }

    private static void SetCookieSecure(
        Cookie cookie,
        string value
    )
    {
        cookie.Secure = true;
    }

    private static void SetCookieDomain(
        Cookie cookie,
        string value
    )
    {
        cookie.Domain = value;
    }

    private static void SetCookieMaxAgeToExpires(
        Cookie cookie,
        string value
    )
    {
        if (!int.TryParse(value, out var seconds))
        {
            throw new ArgumentException(
                $"Unable to parse '{value}' as an integer value"
            );
        }

        cookie.Expires = DateTime.Now.AddSeconds(seconds);
        cookie.Expired = seconds < 1;
    }

    private static void SetCookieExpiration(
        Cookie cookie,
        string value
    )
    {
        if (cookie.Expires > DateTime.MinValue)
        {
            // Max-Age takes precedence over Expires
            return;
        }

        if (!DateTime.TryParse(value, out var expires))
        {
            throw new ArgumentException(
                $"Unable to parse '{value}' as a date-time value"
            );
        }

        cookie.Expires = expires;
        cookie.Expired = expires <= DateTime.Now;
    }
    

    private static string[] TrimAll(IEnumerable<string> source)
    {
        return source.Select(s => s.Trim()).ToArray();
    }
}