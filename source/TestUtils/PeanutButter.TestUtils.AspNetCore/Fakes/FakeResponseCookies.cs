using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using PeanutButter.RandomGenerators;
using PeanutButter.TestUtils.AspNetCore.Builders;
using static PeanutButter.TestUtils.AspNetCore.Fakes.FakeCookie;

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
/// <summary>
/// Provides a fake response cookies service
/// </summary>
public class FakeResponseCookies : IResponseCookies, IFake
{
    /// <summary>
    /// Snapshot the current state of the cookies
    /// </summary>
    /// <returns></returns>
    public IDictionary<string, FakeCookie> Snapshot
        => Cache.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.OrdinalIgnoreCase);

    private IDictionary<string, FakeCookie> Cache
        => ProvideStore();
    
    private readonly Dictionary<string, FakeCookie> _tempStore = new(StringComparer.OrdinalIgnoreCase);

    private IDictionary<string, FakeCookie> ProvideStore()
    {
        if (HttpResponse is null)
        {
            return _tempStore;
        }
        
        Import(_tempStore);
        _tempStore.Clear();

        var currentHash = HttpResponse.Headers.GetHashCode();
        if (currentHash == _responseHeadersHash)
        {
            return _store;
        }

        _responseHeadersHash = currentHash;
        return _store = RegenerateStoreFromHeaders();
    }

    private Dictionary<string, FakeCookie> RegenerateStoreFromHeaders()
    {
        var cookieHeaders = HttpResponse.Headers.Where(
            kvp => kvp.Key.Equals(SET_COOKIE, StringComparison.OrdinalIgnoreCase)
        ).Select(kvp => kvp.Value);
        return cookieHeaders
            .Select(ParseCookies)
            .SelectMany(o => o)
            .Where(o => o is not null)
            .ToDictionary(c => c.Name, c => c);
    }

    private IEnumerable<FakeCookie> ParseCookies(StringValues stringValues)
    {
        foreach (var stringValue in stringValues)
        {
            foreach (var subValue in stringValue.Split(','))
            {
                yield return Parse(subValue);
            }
        }
    }


    private Dictionary<string, FakeCookie> _store = new();

    /// <summary>
    /// Creates an instance of FakeResponseCookies with its
    /// own internal HttpResponse that it's attached to
    /// </summary>
    public FakeResponseCookies() : this(null)
    {
    }

    /// <summary>
    /// Constructs an instance of FakeResponseCookies attached to the
    /// provided HttpResponse
    /// </summary>
    /// <param name="attachedTo"></param>
    public FakeResponseCookies(HttpResponse attachedTo)
    {
        HttpResponse = attachedTo;
        if (attachedTo is FakeHttpResponse fake)
        {
            fake.SetCookies(this);
        }
    }

    /// <summary>
    /// The HttpResponse these cookies are attached to.
    /// You may override the response, but if you provide
    /// null, it will be replaced with a new HttpResponse
    /// </summary>
    public HttpResponse HttpResponse
    {
        get => _response;
        set
        {
            var existingValues = Dump();
            _response = value ?? HttpResponseBuilder.Create()
                .WithCookies(this)
                .Build();
            _responseHeadersHash = null;
            Import(existingValues);
        }
    }

    private void Import(IDictionary<string, FakeCookie> existingValues)
    {
        if (existingValues is null)
        {
            return;
        }

        foreach (var item in existingValues)
        {
            this[item.Key] = item.Value;
        }
    }

    private HttpResponse _response;

    private int? _responseHeadersHash;

    /// <summary>
    /// Provides easier indexing into the store
    /// </summary>
    /// <param name="key"></param>
    public FakeCookie this[string key]
    {
        get => Cache[key];
        set => UpdateStore(key, value);
    }

    private void UpdateStore(string key, FakeCookie value)
    {
        if (value is null)
        {
            Cache.Remove(key);
        }
        else
        {
            Cache[key] = value;
        }

        UpdateResponseCookieHeaders(_response, this);
        var strings = Cache.Select(
            kvp => GenerateSetCookieHeaderFor(kvp.Value)
        ).ToArray();
        _response.Headers[SET_COOKIE] =
            new StringValues(
                strings
            );
    }

    private static void UpdateResponseCookieHeaders(
        HttpResponse response,
        FakeResponseCookies cookies
    )
    {
        if (response is null)
        {
            throw new Exception("no response provided");
        }

        var cookieValues = cookies?.Dump() ?? new Dictionary<string, FakeCookie>();
        var strings = cookieValues.Select(
            kvp => GenerateSetCookieHeaderFor(kvp.Value)
        ).ToArray();
        response.Headers[SET_COOKIE] =
            new StringValues(
                strings
            );
    }

    private IDictionary<string, FakeCookie> Dump()
    {
        var result = new Dictionary<string, FakeCookie>();
        foreach (var key in Keys)
        {
            result[key] = this[key];
        }

        return result;
    }

    private static string GenerateSetCookieHeaderFor(FakeCookie value)
    {
        var parts = new List<string>()
        {
            $"{WebUtility.UrlEncode(value.Name)}={WebUtility.UrlEncode(value.Value)}"
        };
        var opts = value.Options;
        if (!string.IsNullOrWhiteSpace(opts.Domain))
        {
            parts.Add($"{COOKIE_DOMAIN_KEY}={opts.Domain}");
        }

        if (!string.IsNullOrWhiteSpace(opts.Path))
        {
            parts.Add($"{COOKIE_PATH_KEY}={opts.Path}");
        }

        if (opts.MaxAge is not null)
        {
            parts.Add($"{COOKIE_MAX_AGE_KEY}={opts.MaxAge.Value.TotalSeconds}");
        }

        if (opts.Secure)
        {
            parts.Add(COOKIE_SECURE_FLAG);
        }

        if (opts.HttpOnly)
        {
            parts.Add(COOKIE_HTTP_ONLY_FLAG);
        }

        if (opts.Expires is not null)
        {
            var totalSeconds = Math.Floor((opts.Expires.Value - DateTimeOffset.Now).TotalSeconds);
            parts.Add($"Max-Age={totalSeconds}");
        }

        return string.Join("; ", parts);
    }

    /// <summary>
    /// Query if the store contains the key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(string key)
    {
        return Cache.ContainsKey(key);
    }

    /// <summary>
    /// Return all store keys
    /// </summary>
    public string[] Keys => Cache.Keys.ToArray();

    /// <summary>
    /// Return all store values
    /// </summary>
    public FakeCookie[] Values => Cache.Values.ToArray();

    /// <summary>
    /// Attempts to create a substitute for FakeResponseCookies using
    /// NSubstitute's Substitute.ForPartsOf&lt;T&gt; which means you should
    /// both be able to inspect the cookie store and assert that methods
    /// were called on the store within your code. However, there is a
    /// caveat: NSubstitute assertions seem to fail unless you've previously
    /// used NSubstitute in testing. This is most likely due to how NSubstitute
    /// is sought out via reflection and possibly manual assembly loading -
    /// so it may be an issue which is resolved in the future. For now, if you
    /// wish to verify calls, ensure that there is some other kind of NSubstitute
    /// invocation before performing assertions against this substitute.
    /// On the other hand, if you don't have NSubstitute available, you'll
    /// get a plain-old FakeResponseCookies object here.
    /// </summary>
    /// <returns></returns>
    public static IResponseCookies CreateSubstitutedIfPossible(
        HttpResponse attachedTo
    )
    {
        // try to return a substitute if possible
        // -> then assertions against setting cookies is easier
        return GenericBuilderBase.TryCreateSubstituteFor<FakeResponseCookies>(
            callThrough: true,
            new object[]
            {
                attachedTo
            },
            out var result
        )
            ? result
            : new FakeResponseCookies(attachedTo);
    }

    /// <inheritdoc />
    public virtual void Append(string key, string value)
    {
        Append(key, value, new CookieOptions());
    }

    /// <inheritdoc />
    public virtual void Append(string key, string value, CookieOptions options)
    {
        UpdateStore(key, new FakeCookie(key, value, options));
    }

    /// <inheritdoc />
    public virtual void Delete(string key)
    {
        UpdateStore(key, null);
    }

    /// <inheritdoc />
    public virtual void Delete(string key, CookieOptions options)
    {
        if (!Cache.TryGetValue(key, out var cookie))
        {
            return;
        }

        if (cookie.Options.Path != options.Path ||
            cookie.Options.Domain != options.Domain)
        {
            return;
        }

        Delete(key);
    }
}