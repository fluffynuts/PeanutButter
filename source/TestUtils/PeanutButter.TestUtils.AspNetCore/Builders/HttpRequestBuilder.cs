using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;

// ReSharper disable MemberCanBePrivate.Global
namespace PeanutButter.TestUtils.AspNetCore.Builders;

/// <summary>
/// Builds an http request
/// </summary>
public class HttpRequestBuilder : RandomizableBuilder<HttpRequestBuilder, HttpRequest>
{
    internal static HttpRequestBuilder CreateWithNoHttpContext()
    {
        return new HttpRequestBuilder(noContext: true);
    }

    /// <inheritdoc />
    public HttpRequestBuilder()
        : this(noContext: false)
    {
    }

    /// <summary>
    /// Default constructor: creates the builder with basics set up
    /// </summary>
    internal HttpRequestBuilder(bool noContext) : base(Actualize)
    {
        WithForm(FormBuilder.BuildDefault())
            .WithMethod(HttpMethod.Get)
            .WithScheme("http")
            .WithPath("/")
            .WithHost("localhost")
            .WithPort(80)
            .WithHeaders(HeaderDictionaryBuilder.BuildDefault())
            .WithCookies(
                RequestCookieCollectionBuilder
                    .Create()
                    .Build()
            ).WithPostBuild(request =>
            {
                if (request.Cookies is FakeRequestCookieCollection fake)
                {
                    fake.HttpRequest = request;
                }
            });
        if (!noContext)
        {
            WithHttpContext(
                () => HttpContextBuilder.Create()
                    .WithRequest(() => CurrentEntity)
                    .Build()
            );
        }
    }

    /// <summary>
    /// Constructs the fake http request
    /// </summary>
    /// <returns></returns>
    protected override HttpRequest ConstructEntity()
    {
        return new FakeHttpRequest();
    }

    /// <summary>
    /// Randomizes the output
    /// </summary>
    /// <returns></returns>
    public override HttpRequestBuilder Randomize()
    {
        return WithRandomMethod()
            .WithRandomScheme()
            .WithRandomPath()
            .WithRandomHost()
            .WithRandomPort()
            .WithRandomHeaders()
            .WithRandomCookies();
    }

    /// <summary>
    /// Selects a random http method for the request
    /// </summary>
    /// <returns></returns>
    public HttpRequestBuilder WithRandomMethod()
    {
        return WithMethod(GetRandomHttpMethod());
    }

    /// <summary>
    /// Selects a random scheme (http|https) for the request
    /// </summary>
    /// <returns></returns>
    public HttpRequestBuilder WithRandomScheme()
    {
        return WithScheme(GetRandomFrom(HttpSchemes));
    }

    /// <summary>
    /// Selects a random path for the request
    /// </summary>
    /// <returns></returns>
    public HttpRequestBuilder WithRandomPath()
    {
        return WithPath(GetRandomPath());
    }

    /// <summary>
    /// Selects a random hostname for the request
    /// </summary>
    /// <returns></returns>
    public HttpRequestBuilder WithRandomHost()
    {
        return WithHost(GetRandomHostname());
    }

    /// <summary>
    /// Selects a random port (80-10000) for the request
    /// </summary>
    /// <returns></returns>
    public HttpRequestBuilder WithRandomPort()
    {
        return WithPort(GetRandomInt(80, 10000));
    }

    /// <summary>
    /// Adds some random X- prefixed headers for the request
    /// </summary>
    /// <returns></returns>
    public HttpRequestBuilder WithRandomHeaders()
    {
        return WithRandomTimes(
            o => o.Headers[$"X-{GetRandomString(4, 8)}"] = GetRandomString()
        );
    }

    /// <summary>
    /// Adds some random cookies to the request
    /// </summary>
    /// <returns></returns>
    public HttpRequestBuilder WithRandomCookies()
    {
        return WithRandomTimes(
            o =>
            {
                if (o.Cookies is FakeRequestCookieCollection fake)
                {
                    fake[GetRandomString(4, 10)] = GetRandomString();
                }
            }
        );
    }

    private static readonly string[] HttpSchemes =
    {
        "http",
        "https"
    };

    private static void Actualize(HttpRequest built)
    {
        WarnIf(built.HttpContext is null, "no HttpContext set");
        WarnIf(built.HttpContext?.Request is null, "no HttpContext.Request set");
    }

    /// <summary>
    /// Sets the body for the request. If possible, form elements
    /// are derived from the body.
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithBody(string body)
    {
        return WithBody(Encoding.UTF8.GetBytes(body));
    }

    /// <summary>
    /// Sets the body for the request. If possible, form elements
    /// are derived from the body.
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithBody(byte[] body)
    {
        return WithBody(new MemoryStream(body));
    }

    /// <summary>
    /// Sets the body for the request. If possible, form elements
    /// are derived from the body.
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithBody(Stream body)
    {
        return With(
            o => o.Body = body
        );
    }

    /// <summary>
    /// Sets a cookie on the request. Will overwrite an existing cookie
    /// with the same name.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithCookie(string name, string value)
    {
        return With(
            o =>
            {
                CookieUtil.GenerateCookieHeader(
                    new Dictionary<string, string>()
                    {
                        [name] = value
                    },
                    o,
                    overwrite: false
                );
            }
        );
    }

    /// <summary>
    /// Sets a bunch of cookies on the request. Will overwrite
    /// existing cookies with the same name. Will _not_ remove
    /// any other existing cookies.
    /// </summary>
    /// <param name="cookies"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithCookies(
        IDictionary<string, string> cookies
    )
    {
        return With(
            o =>
            {
                CookieUtil.GenerateCookieHeader(
                    cookies,
                    o,
                    overwrite: false
                );
            }
        );
    }

    /// <summary>
    /// Clears cookies on the request
    /// </summary>
    /// <returns></returns>
    public HttpRequestBuilder WithNoCookies()
    {
        return With(
            o =>
            {
                o.Headers.Remove(CookieUtil.HEADER);
            });
    }

    /// <summary>
    /// Sets the cookie collection on the request
    /// </summary>
    /// <param name="cookies"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithCookies(IRequestCookieCollection cookies)
    {
        return With(
            o => o.Cookies = cookies
        );
    }

    /// <summary>
    /// Clears headers on the request
    /// </summary>
    /// <returns></returns>
    public HttpRequestBuilder WithNoHeaders()
    {
        return With(
            o => o.Headers.Clear()
        );
    }

    /// <summary>
    /// Sets a header on the request. Any existing header with
    /// the same name is overwritten.
    /// </summary>
    /// <param name="header"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithHeader(
        string header,
        string value
    )
    {
        return With(
            o => o.Headers[header] = value
        );
    }

    /// <summary>
    /// Sets a bunch of headers on the request. Existing cookies
    /// with the same names are overwritten. Other existing
    /// cookies are left intact.
    /// </summary>
    /// <param name="headers"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public HttpRequestBuilder WithHeaders(
        IDictionary<string, string> headers
    )
    {
        return With(
            o =>
            {
                if (headers is null)
                {
                    throw new ArgumentNullException(nameof(headers));
                }

                foreach (var kvp in headers)
                {
                    o.Headers[kvp.Key] = kvp.Value;
                }
            }
        );
    }

    /// <summary>
    /// Sets the header dictionary on the request
    /// </summary>
    /// <param name="headers"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithHeaders(IHeaderDictionary headers)
    {
        return With<FakeHttpRequest>(
            o => o.SetHeaders(headers)
        );
    }

    /// <summary>
    /// Sets the query collection on the request
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithQuery(IQueryCollection query)
    {
        return With(
            o => o.Query = query
        );
    }

    /// <summary>
    /// Sets a query parameter on the request
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithQueryParameter(string key, string value)
    {
        return With(
            o => o.Query.As<FakeQueryCollection>()[key] = value
        );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithPath(string path)
    {
        return With(
            o => o.Path = new PathString(SanitisePath(path))
        );
    }

    private static string SanitisePath(string path)
    {
        path ??= "";
        return !path.StartsWith("/")
            ? $"/{path}"
            : path;
    }

    /// <summary>
    /// Sets the base path on the request
    /// </summary>
    /// <param name="basePath"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithBasePath(string basePath)
    {
        return With(
            o => o.PathBase = new PathString(SanitisePath(basePath))
        );
    }

    /// <summary>
    /// Sets the host on the path
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithHost(HostString host)
    {
        return With(
            o => o.Host = host
        );
    }

    /// <summary>
    /// Sets the port on the request
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithPort(int port)
    {
        return With(
            o => o.Host = new HostString(o.Host.Host, port)
        );
    }

    /// <summary>
    /// Sets the host on the request
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithHost(string host)
    {
        return With(
            o => o.Host = o.Host.Port is null
                ? new HostString(host)
                : new HostString(host, o.Host.Port.Value)
        );
    }

    /// <summary>
    /// Sets the query string on the request
    /// </summary>
    /// <param name="queryString"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithQueryString(
        string queryString
    )
    {
        return WithQueryString(new QueryString(queryString));
    }

    /// <summary>
    /// Sets the query string on the request
    /// </summary>
    /// <param name="queryString"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithQueryString(
        QueryString queryString
    )
    {
        return With(
            o => o.QueryString = queryString
        );
    }

    /// <summary>
    /// Sets the scheme on the request
    /// </summary>
    /// <param name="scheme"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithScheme(string scheme)
    {
        return With(
            o => o.Scheme = scheme
        ).With(o => o.Protocol = FakeHttpRequest.GuessProtocolFor(o.Scheme));
    }

    /// <summary>
    /// Sets the method on the request
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithMethod(HttpMethod method)
    {
        return WithMethod(method.ToString());
    }

    /// <summary>
    /// Sets the method on the request
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithMethod(string method)
    {
        return With(o => o.Method = (method ?? "get").ToUpper());
    }

    /// <summary>
    /// Sets the http context on the request
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithHttpContext(
        HttpContext context
    )
    {
        return WithHttpContext(() => context);
    }

    /// <summary>
    /// Sets the http context accessor on the request
    /// </summary>
    /// <param name="accessor"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithHttpContext(
        Func<HttpContext> accessor
    )
    {
        return With<FakeHttpRequest>(
            o => o.SetContextAccessor(accessor),
            nameof(FakeHttpRequest.HttpContext)
        );
    }

    /// <summary>
    /// Sets the form on the request
    /// </summary>
    /// <param name="formCollection"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithForm(IFormCollection formCollection)
    {
        return With(
            o => o.Form = formCollection
        );
    }

    /// <summary>
    /// Set a field on the form of the request
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithFormField(
        string key,
        string value
    )
    {
        return With(
            o => o.Form.As<FakeFormCollection>()[key] = value
        );
    }

    /// <summary>
    /// Adds a file to the form of the request
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithFormFile(IFormFile file)
    {
        return With(
            o => o.Form.As<FakeFormCollection>()
                .Files.As<FakeFormFileCollection>()
                .Add(file)
        );
    }

    /// <summary>
    /// Adds a form file with string content and the provided name and filename
    /// </summary>
    /// <param name="content"></param>
    /// <param name="name"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithFormFile(
        string content,
        string name,
        string fileName
    )
    {
        return WithFormFile(
            Encoding.UTF8.GetBytes(content),
            name,
            fileName
        );
    }

    /// <summary>
    /// Adds a form file with binary content and the provided name and filename
    /// </summary>
    /// <param name="content"></param>
    /// <param name="name"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithFormFile(
        byte[] content,
        string name,
        string fileName
    )
    {
        return WithFormFile(
            new MemoryStream(content),
            name,
            fileName
        );
    }

    /// <summary>
    /// Set the full url for the request
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithUrl(string url)
    {
        return WithUrl(new Uri(url));
    }

    /// <summary>
    /// Set the full url for the request
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithUrl(Uri url)
    {
        return With(o => o.As<FakeHttpRequest>().SetUrl(url));
    }

    private static readonly Dictionary<string, int> DefaultPorts = new(StringComparer.OrdinalIgnoreCase)
    {
        ["http"] = 80,
        ["https"] = 443
    };

    /// <summary>
    /// Adds a form file with text content and the provided name and filename
    /// </summary>
    /// <param name="content"></param>
    /// <param name="name"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithFormFile(
        Stream content,
        string name,
        string fileName
    )
    {
        return With(
            o => o.Form.Files.As<FakeFormFileCollection>()
                .Add(new FakeFormFile(content, name, fileName))
        );
    }

    /// <summary>
    /// Facilitates easier http context mutations
    /// </summary>
    /// <param name="mutator"></param>
    /// <returns></returns>
    public HttpRequestBuilder WithHttpContextMutator(
        Action<FakeHttpContext> mutator
    )
    {
        return With(o => mutator(o.HttpContext.As<FakeHttpContext>()));
    }
}