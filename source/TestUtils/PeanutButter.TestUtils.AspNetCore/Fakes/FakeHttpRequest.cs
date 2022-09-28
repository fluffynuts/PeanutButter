using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using PeanutButter.TestUtils.AspNetCore.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

/// <summary>
/// Implements a fake http request
/// </summary>
public class FakeHttpRequest : HttpRequest, IFake
{
    /// <inheritdoc />
    public override Task<IFormCollection> ReadFormAsync(
        CancellationToken cancellationToken = new CancellationToken()
    )
    {
        return Task.FromResult<IFormCollection>(
            new FakeFormCollection()
        );
    }

    /// <inheritdoc />
    public FakeHttpRequest()
    {
        _query = CreateFakeQueryCollection();
    }

    /// <inheritdoc />
    public override HttpContext HttpContext =>
        _httpContext ??= _httpContextAccessor?.Invoke();

    private HttpContext _httpContext;
    private Func<HttpContext> _httpContextAccessor;

    /// <inheritdoc />
    public override string Method { get; set; }

    /// <inheritdoc />
    public override string Scheme { get; set; }

    /// <inheritdoc />
    public override bool IsHttps
    {
        get => Scheme?.ToLower() == "https";
        set
        {
            Scheme = value
                ? "https"
                : "http";
            Protocol = GuessProtocolFor(Scheme);
        }
    }

    internal static string GuessProtocolFor(string scheme)
    {
        return scheme?.ToLower() switch
        {
            "http" => "HTTP/1.1",
            "https" => "HTTP/2",
            _ => scheme
        };
    }

    /// <inheritdoc />
    public override HostString Host { get; set; } = new("localhost");

    /// <inheritdoc />
    public override PathString PathBase { get; set; } = new("");

    /// <inheritdoc />
    public override PathString Path { get; set; } = new("/");

    /// <inheritdoc />
    public override QueryString QueryString
    {
        get => _queryString;
        set
        {
            _queryString = value;
            _query = new FakeQueryCollection(_queryString.ToString());
        }
    }

    private QueryString _queryString = new("");

    /// <inheritdoc />
    public override IQueryCollection Query
    {
        get => _query ??= CreateFakeQueryCollection();
        set
        {
            if (_query is FakeQueryCollection fake1)
            {
                fake1.OnChanged -= UpdateQueryStringFromQuery;
            }

            _query = value ?? new FakeQueryCollection();
            if (_query is FakeQueryCollection fake2)
            {
                fake2.OnChanged += UpdateQueryStringFromQuery;
            }

            UpdateQueryStringFromQuery();
        }
    }

    private FakeQueryCollection CreateFakeQueryCollection()
    {
        var result = new FakeQueryCollection();
        result.OnChanged += UpdateQueryStringFromQuery;
        return result;
    }

    private void UpdateQueryStringFromQuery(object sender, StringValueMapChangedEventArgs args)
    {
        UpdateQueryStringFromQuery();
    }

    private void UpdateQueryStringFromQuery()
    {
        _queryString = GenerateQueryStringFrom(_query);
    }

    private IQueryCollection _query;
    private IHeaderDictionary _headers = new FakeHeaderDictionary();

    /// <inheritdoc />
    public override string Protocol { get; set; }

    /// <inheritdoc />
    public override IHeaderDictionary Headers
        => _headers ??= new FakeHeaderDictionary();

    /// <inheritdoc />
    public override IRequestCookieCollection Cookies { get; set; }
        = new FakeRequestCookieCollection();

    /// <inheritdoc />
    public override long? ContentLength
    {
        get => Body.Length;
        set => Body.SetLength(value ?? 0);
    }

    /// <inheritdoc />
    public override string ContentType { get; set; } = "";

    /// <inheritdoc />
    public override Stream Body
    {
        get => _body ??= new MemoryStream();
        set
        {
            _body = value ?? new MemoryStream();
            UpdateFormFromBody();
        }
    }

    private Stream _body = new MemoryStream();

    /// <inheritdoc />
    public override bool HasFormContentType =>
        (Form?.Keys.Count ?? 0) > 0;

    /// <inheritdoc />
    public override IFormCollection Form
    {
        get => _form;
        set
        {
            _form = value ?? new FakeFormCollection();
            UpdateBodyForForm();
        }
    }

    private void UpdateFormFromBody()
    {
        _form = new FormDecoder().Decode(_body);
        ContentType = SelectContentTypeForFormOrBody();
    }

    private void UpdateBodyForForm()
    {
        ContentType = SelectContentTypeForFormOrBody();
        var encoder = ContentType switch
        {
            MULTIPART_FORM => new MultiPartBodyEncoder() as IFormEncoder,
            URLENCODED_FORM => new UrlEncodedBodyEncoder(),
            _ => new NullBodyEncoder()
        };
        _body = encoder.Encode(Form);
    }

    private const string MULTIPART_FORM = "multipart/form-data";
    private const string URLENCODED_FORM = "application/x-www-form-urlencoded";

    private string SelectContentTypeForFormOrBody()
    {
        if (_form.Files.Any())
        {
            return MULTIPART_FORM;
        }

        if (_form.Keys.Any())
        {
            return "application/x-www-form-urlencoded";
        }

        if ((_body?.Length ?? 0) == 0)
        {
            return "text/plain";
        }

        // assume json (if not set), for now
        return ContentType ?? "application/json";
    }

    private IFormCollection _form = new FakeFormCollection();

    /// <summary>
    /// Sets the http context for the request
    /// </summary>
    /// <param name="context"></param>
    public void SetContext(HttpContext context)
    {
        SetContextAccessor(() => context);
    }

    /// <summary>
    /// Sets the http context accessor for the request
    /// </summary>
    /// <param name="accessor"></param>
    public void SetContextAccessor(Func<HttpContext> accessor)
    {
        _httpContext = null;
        _httpContextAccessor = accessor;
    }

    private QueryString GenerateQueryStringFrom(IQueryCollection query)
    {
        var parts = new List<string>();
        foreach (var key in query.Keys)
        {
            parts.Add($"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(query[key])}");
        }

        return parts.Any()
            ? new QueryString($"?{string.Join("&", parts)}")
            : new QueryString();
    }

    /// <summary>
    /// Sets the headers collection for the request
    /// </summary>
    /// <param name="headers"></param>
    public void SetHeaders(IHeaderDictionary headers)
    {
        _headers = headers;
    }

    /// <summary>
    /// Sets the url components on this request from the provided Url
    /// </summary>
    /// <param name="url"></param>
    public void SetUrl(string url)
    {
        SetUrl(new Uri(url));
    }

    /// <summary>
    /// Sets the url components on this request from the provided Url
    /// </summary>
    /// <param name="url"></param>
    public void SetUrl(Uri url)
    {
        global::HttpRequestExtensions.SetUrl(this, url);
    }
}