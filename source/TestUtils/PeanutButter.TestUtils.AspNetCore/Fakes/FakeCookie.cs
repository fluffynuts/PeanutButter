using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

// ReSharper disable MemberCanBePrivate.Global

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
#else
namespace PeanutButter.TestUtils.AspNetCore.Fakes;
#endif

/// <summary>
/// Implements a fake cookie holder
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class FakeCookie : IFake
{
    /// <summary>
    /// The name of the cookie
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The value of the cookie
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// The options for the cookie
    /// </summary>
    public CookieOptions Options { get; }

    /// <summary>
    /// Constructs a fake response cookie with the
    /// default options
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public FakeCookie(
        string name,
        string value
    ) : this(name, value, new CookieOptions())
    {
    }

    /// <summary>
    /// Constructs the cookie container
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public FakeCookie(
        string name,
        string value,
        CookieOptions options
    )
    {
        Name = name;
        Value = value;
        Options = options;
    }

    /// <summary>
    /// Parse a raw set-cookie header
    /// </summary>
    /// <param name="rawCookie"></param>
    /// <returns></returns>
    public static FakeCookie Parse(string rawCookie)
    {
        if (string.IsNullOrWhiteSpace(rawCookie))
        {
            return null;
        }

        var parts = rawCookie.Split(
            new[]
            {
                ";"
            },
            StringSplitOptions.RemoveEmptyEntries
        );
        var sub = parts[0].Split('=');
        var name = sub[0];
        var value = string.Join("=", sub.Skip(1));
        var options = new CookieOptions();
        foreach (var part in parts.Skip(1))
        {
            var trimmed = part.Trim();
            var pair = trimmed.Split(
                new[]
                {
                    "="
                },
                StringSplitOptions.RemoveEmptyEntries
            );
            if (pair.Length < 2)
            {
                foreach (var parser in SimpleOptionsParsers)
                {
                    parser(trimmed, options);
                }

                continue;
            }

            var subKey = pair[0].Trim();
            var subValue = string.Join("=", pair.Skip(1)).Trim();

            foreach (var parser in PairedOptionsParsers)
            {
                parser(subKey, subValue, options);
            }
        }

        return new FakeCookie(name, value, options);
    }

    private static readonly Action<string, CookieOptions>[] SimpleOptionsParsers =
    [
        ParseHttpOnlyFlag,
        ParseSecureFlag
    ];

    private static void ParseSecureFlag(string value, CookieOptions opts)
    {
        if (AreEqual(value, COOKIE_SECURE_FLAG))
        {
            opts.Secure = true;
        }
    }

    private static void ParseHttpOnlyFlag(string value, CookieOptions opts)
    {
        if (AreEqual(value, COOKIE_HTTP_ONLY_FLAG))
        {
            opts.HttpOnly = true;
        }
    }

    private static readonly Action<string, string, CookieOptions>[] PairedOptionsParsers =
    [
        SetDomain,
        SetPath,
        SetSameSite,
        SetMaxAge
    ];

    private static void SetMaxAge(string key, string value, CookieOptions opts)
    {
        if (AreEqual(key, COOKIE_MAX_AGE_KEY))
        {
            var seconds = int.TryParse(value, out var parsed)
                ? parsed
                : 0;
            opts.MaxAge = TimeSpan.FromSeconds(seconds);
        }
    }

    private static void SetPath(string key, string value, CookieOptions opts)
    {
        if (AreEqual(key, COOKIE_PATH_KEY))
        {
            opts.Path = value;
        }
    }

    private static void SetSameSite(string key, string value, CookieOptions opts)
    {
        if (AreEqual(key, COOKIE_SAME_SITE_KEY))
        {
            opts.SameSite = ParseSameSite(value);
        }
    }

    private static void SetDomain(string key, string value, CookieOptions opts)
    {
        if (AreEqual(key, COOKIE_DOMAIN_KEY))
        {
            opts.Domain = value;
        }
    }

    private static bool AreEqual(string left, string right)
    {
        return left.Equals(right, StringComparison.OrdinalIgnoreCase);
    }

    private static SameSiteMode ParseSameSite(string subValue)
    {
        return Enum.TryParse<SameSiteMode>(subValue, ignoreCase: true, out var parsed)
            ? parsed
            : SameSiteMode.Lax;
    }

    internal const string COOKIE_DOMAIN_KEY = "Domain";
    internal const string COOKIE_PATH_KEY = "Path";
    internal const string COOKIE_HTTP_ONLY_FLAG = "HttpOnly";
    internal const string COOKIE_SECURE_FLAG = "Secure";
    internal const string COOKIE_SAME_SITE_KEY = "SameSite";
    internal const string COOKIE_MAX_AGE_KEY = "Max-Age";
    internal const string SET_COOKIE = "Set-Cookie";
}