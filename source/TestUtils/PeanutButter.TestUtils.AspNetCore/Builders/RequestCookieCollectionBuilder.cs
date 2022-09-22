using System;
using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Builders;

// ReSharper disable once ClassNeverInstantiated.Global
/// <summary>
/// Builds a fake request cookie collection
/// </summary>
public class RequestCookieCollectionBuilder
    : RandomizableBuilder<RequestCookieCollectionBuilder, IRequestCookieCollection>
{
    /// <summary>
    /// Builds a request cookie collection
    /// </summary>
    public RequestCookieCollectionBuilder() : base(Actualize)
    {
    }

    private static void Actualize(IRequestCookieCollection requestCookieCollection)
    {
        var requestCookies = requestCookieCollection as FakeRequestCookieCollection;
        WarnIf(requestCookies is null, "request cookies is not a FakeRequestCookieCollection");
        WarnIf(requestCookies?.HttpRequest is null, "request cookies not set up with any associated request");
    }

    /// <summary>
    /// Constructs the fake cookie collection
    /// </summary>
    /// <returns></returns>
    protected override IRequestCookieCollection ConstructEntity()
    {
        return new FakeRequestCookieCollection();
    }

    /// <summary>
    /// Produces a random cookie collection when part of the build pipeline
    /// </summary>
    /// <returns></returns>
    public override RequestCookieCollectionBuilder Randomize()
    {
        return WithRandomTimes<FakeRequestCookieCollection>(
            o => o[GetRandomString(4)] = GetRandomString(4, 20)
        );
    }
}