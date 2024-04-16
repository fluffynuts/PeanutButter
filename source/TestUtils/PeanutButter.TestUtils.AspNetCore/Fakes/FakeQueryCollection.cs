using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
#else
namespace PeanutButter.TestUtils.AspNetCore.Fakes;
#endif

/// <summary>
/// Provides a fake query collection
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class FakeQueryCollection : StringValueMap, IQueryCollection, IFake
{
    /// <inheritdoc />
    public FakeQueryCollection() : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    /// <inheritdoc />
    public FakeQueryCollection(string queryString) : base(StringComparer.OrdinalIgnoreCase)
    {
        queryString ??= "";
        queryString = queryString.Trim();
        if (queryString == "")
        {
            return;
        }

        if (queryString[0] == '?')
        {
            queryString = queryString.Substring(1);
        }

        var parts = queryString.Split('&');
        foreach (var part in parts)
        {
            var sub = part.Split('=');
            var key = sub[0];
            var value = string.Join("=", sub.Skip(1));
            Store[key] = value;
        }
    }
}