using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

/// <summary>
/// Provides a fake http header dictionary
/// </summary>
public class FakeHeaderDictionary : StringValueMap, IHeaderDictionary, IFake
{
    /// <inheritdoc />
    public FakeHeaderDictionary(
        IDictionary<string, StringValues> store
    ): base(store)
    {
    }

    /// <inheritdoc />
    public FakeHeaderDictionary()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    private const string CONTENT_LENGTH_HEADER = "Content-Length";

    /// <inheritdoc />
    public long? ContentLength
    {
        get => Store.TryGetValue(CONTENT_LENGTH_HEADER, out var header)
            ? TryParseInt(header)
            : 0;
        set => Store[CONTENT_LENGTH_HEADER] = value?.ToString();
    }

    private static long? TryParseInt(string value)
    {
        if (value is null)
        {
            return 0;
        }

        return long.TryParse(value, out var result)
            ? result
            : 0;
    }
}