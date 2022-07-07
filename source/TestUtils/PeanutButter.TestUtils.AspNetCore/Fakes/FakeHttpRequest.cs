using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

public class FakeHttpRequest : HttpRequest
{
    public override Task<IFormCollection> ReadFormAsync(
        CancellationToken cancellationToken = new CancellationToken()
    )
    {
        return Task.FromResult<IFormCollection>(
            new FakeFormCollection()
        );
    }

    public override HttpContext HttpContext =>
        _httpContext ??= _httpContextAccessor();

    private HttpContext _httpContext;
    private Func<HttpContext> _httpContextAccessor;
    public override string Method { get; set; }
    public override string Scheme { get; set; }

    public override bool IsHttps
    {
        get => Scheme?.ToLower() == "https";
        set => Scheme = value
            ? "https"
            : "http";
    }

    public override HostString Host { get; set; }
    public override PathString PathBase { get; set; }
    public override PathString Path { get; set; }

    public override QueryString QueryString
    {
        get => _queryString;
        set
        {
            _queryString = value;
            _query = new FakeQueryCollection(_queryString.ToString());
        }
    }

    private QueryString _queryString;

    public override IQueryCollection Query
    {
        get => _query ??= new FakeQueryCollection();
        set
        {
            _query = value ?? new FakeQueryCollection();
            _queryString = GenerateQueryStringFrom(_query);
        }
    }

    private IQueryCollection _query;
    private IHeaderDictionary _headers;

    public override string Protocol
    {
        get => Scheme;
        set => Scheme = value;
    }

    public override IHeaderDictionary Headers
        => _headers ??= new FakeHeaderDictionary();

    public override IRequestCookieCollection Cookies { get; set; } = new FakeRequestCookieCollection();

    public override long? ContentLength
    {
        get => Body.Length;
        set => Body.SetLength(value ?? 0);
    }

    public override string ContentType { get; set; }

    public override Stream Body
    {
        get => _body ??= new MemoryStream();
        set 
        {
            _body = value ?? new MemoryStream(); 
            UpdateFormFromBody();
        }
    }

    private Stream _body;

    public override bool HasFormContentType =>
        (Form?.Keys.Count ?? 0) > 0;

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

    private IFormCollection _form;

    public void SetContext(HttpContext context)
    {
        SetContextAccessor(() => context);
    }

    public void SetContextAccessor(Func<HttpContext> accessor)
    {
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

    public void SetHeaders(IHeaderDictionary headers)
    {
        _headers = headers;
    }
}