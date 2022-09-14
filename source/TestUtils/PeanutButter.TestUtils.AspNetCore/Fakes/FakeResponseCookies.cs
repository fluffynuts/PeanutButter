using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using PeanutButter.RandomGenerators;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
/// <summary>
/// Provides a fake response cookies service
/// </summary>
public class FakeResponseCookies : IResponseCookies, IFake
{
    /// <summary>
    /// Use to query what cookies have been set
    /// </summary>
    public IDictionary<string, FakeCookie> Store
        => _store;

    private readonly Dictionary<string, FakeCookie> _store = new();

    /// <summary>
    /// Provides easier indexing into the store
    /// </summary>
    /// <param name="key"></param>
    public FakeCookie this[string key] 
    { 
        get => Store[key];
        set => Store[key] = value;
    }

    /// <summary>
    /// Query if the store contains the key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool ContainsKey(string key)
    {
        return Store.ContainsKey(key);
    }
    
    /// <summary>
    /// Return all store keys
    /// </summary>
    public string[] Keys => Store.Keys.ToArray();
    
    /// <summary>
    /// Return all store values
    /// </summary>
    public FakeCookie[] Values => Store.Values.ToArray();

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
    public static IResponseCookies CreateSubstitutedIfPossible()
    {
        // try to return a substitute if possible
        // -> then assertions against setting cookies is easier
        return GenericBuilderBase.TryCreateSubstituteFor<FakeResponseCookies>(
            callThrough: true,
            out var result
        )
            ? result
            : new FakeResponseCookies();
    }

    /// <inheritdoc />
    public virtual void Append(string key, string value)
    {
        Append(key, value, new CookieOptions());
    }

    /// <inheritdoc />
    public virtual void Append(string key, string value, CookieOptions options)
    {
        Store[key] = new FakeCookie(key, value, options);
    }

    /// <inheritdoc />
    public virtual void Delete(string key)
    {
        Store.Remove(key);
    }

    /// <inheritdoc />
    public virtual void Delete(string key, CookieOptions options)
    {
        if (!Store.TryGetValue(key, out var cookie))
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