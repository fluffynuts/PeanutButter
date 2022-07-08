using System.Linq;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

/// <summary>
/// Provides a fake query collection
/// </summary>
public class FakeQueryCollection : StringValueMap, IQueryCollection, IFake
{
    /// <inheritdoc />
    public FakeQueryCollection()
    {
    }

    /// <inheritdoc />
    public FakeQueryCollection(string queryString)
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