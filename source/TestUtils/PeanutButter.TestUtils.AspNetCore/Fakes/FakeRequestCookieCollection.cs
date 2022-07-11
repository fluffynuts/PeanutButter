using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

/// <summary>
/// Provides a fake cookie collection
/// </summary>
public class FakeRequestCookieCollection
    : StringMap, IRequestCookieCollection, IFake
{
}