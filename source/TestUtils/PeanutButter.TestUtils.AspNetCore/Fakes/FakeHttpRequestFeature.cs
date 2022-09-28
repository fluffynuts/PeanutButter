using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using PeanutButter.TestUtils.AspNetCore;
using PeanutButter.TestUtils.AspNetCore.Fakes;

/// <summary>
/// Provides a faked implementation of IHttpRequestFeature
/// </summary>
public class FakeHttpRequestFeature : IHttpRequestFeature
{
    private readonly Func<HttpRequest> _factory;

    /// <summary>
    /// Constructs a faked HttpRequestFeature tied to the
    /// HttpRequest provided by the factory
    /// </summary>
    /// <param name="factory"></param>
    /// <exception cref="NotImplementedException"></exception>
    public FakeHttpRequestFeature(Func<HttpRequest> factory)
    {
        _factory = factory;
    }

    private HttpRequest Request => _factory() ?? throw new InvalidOperationException(
        "No request available"
    );

    /// <inheritdoc />
    public string Protocol
    {
        get => Request.Protocol;
        set => Request.Protocol = value;
    }

    /// <inheritdoc />
    public string Scheme
    {
        get => Request.Scheme;
        set => Request.Scheme = value;
    }

    /// <inheritdoc />
    public string Method
    {
        get => Request.Method;
        set => Request.Method = value;
    }

    /// <inheritdoc />
    public string PathBase
    {
        get => Request.PathBase;
        set => Request.PathBase = value;
    }

    /// <inheritdoc />
    public string Path
    {
        get => Request.Path;
        set => Request.Path = value;
    }

    /// <inheritdoc />
    public string QueryString
    {
        get => Request.QueryString.ToString();
        set => Request.QueryString = new QueryString(value);
    }

    /// <inheritdoc />
    public string RawTarget
    {
        get => $"{Request.Method?.ToUpper() ?? "GET"} {Request.FullUrl()} HTTP/1.1";
        set => throw new NotImplementedException(
            "Not (yet) implemented, because doing so requires parsing any one of the forms at https://www.rfc-editor.org/rfc/rfc7230#section-5.3"
        );
    }

    /// <inheritdoc />
    public IHeaderDictionary Headers
    {
        get => Request.Headers;
        set
        {
            if (Request is not FakeHttpRequest fake)
            {
                throw new NotSupportedException(
                    $"Headers may only be completely replaced if the underlying response is a {nameof(FakeHttpRequest)}"
                );
            }

            fake.SetHeaders(value);
        }
    }

    /// <inheritdoc />
    public Stream Body
    {
        get => Request.Body;
        set => Request.Body = value;
    }
}