using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using PeanutButter.Utils;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore;
#else
namespace PeanutButter.TestUtils.AspNetCore;
#endif

/// <summary>
/// Provides extensions for asp.net HttpResponse objects
/// </summary>
public static class HttpResponseExtensions
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
        return res.Headers.Where(
                h => h.Key.Equals("Set-Cookie", StringComparison.OrdinalIgnoreCase)
            ).Select(h => h.Value.Select(ParseCookieHeader))
            .SelectMany(o => o);
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


    private static Cookie ParseCookieHeader(
        string header
    )
    {
        var parts = header.Split(';').Trim();
        return parts.Aggregate(
            new Cookie(),
            (acc, cur) =>
            {
                var subs = cur.Split('=');
                var key = subs[0].Trim();
                var value = string.Join("=", subs.Skip(1));
                if (string.IsNullOrWhiteSpace(acc.Name))
                {
                    acc.Name = key;
                    acc.Value = value;
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
            ["Max-Age"] = SetCookieMaxAge,
            ["Domain"] = SetCookieDomain,
            ["Secure"] = SetCookieSecure,
            ["HttpOnly"] = SetCookieHttpOnly,
            ["SameSite"] = SetCookieSameSite,
            ["Path"] = SetCookiePath
        };

    private static void SetCookiePath(
        Cookie cookie,
        string value
    )
    {
        cookie.Path = value;
    }

    private static void SetCookieSameSite(
        Cookie cookie,
        string value
    )
    {
        // Cookie object doesn't natively support the SameSite property, yet
        cookie.SetMetadata("SameSite", value);
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

    private static void SetCookieMaxAge(
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
        cookie.SetMetadata("Max-Age", seconds);
    }

    private static void SetCookieExpiration(
        Cookie cookie,
        string value
    )
    {
        if (cookie.TryGetMetadata<int>("Max-Age", out _))
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
}