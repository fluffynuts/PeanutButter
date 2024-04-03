using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.Utils;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Builders;

/// <summary>
/// Builds a fake http response
/// </summary>
public class HttpResponseBuilder : RandomizableBuilder<HttpResponseBuilder, HttpResponse>
{
    internal static HttpResponseBuilder CreateWithNoHttpContext()
    {
        return new HttpResponseBuilder(noContext: true);
    }

    /// <inheritdoc />
    public HttpResponseBuilder()
        : this(noContext: false)
    {
    }

    // ReSharper disable once MemberCanBePrivate.Global
    internal HttpResponseBuilder(bool noContext) : base(Actualize)
    {
        WithStatusCode(HttpStatusCode.OK)
            .WithCookies(FakeResponseCookies.CreateSubstitutedIfPossible);
        if (!noContext)
        {
            WithHttpContext(
                () => HttpContextBuilder.Create()
                    .WithResponse(() => CurrentEntity)
                    .Build()
            );
        }
    }

    /// <summary>
    /// Sets the HasStarted flag on the response
    /// HttpResponses have "started" when the headers have already
    /// been sent to the client
    /// </summary>
    /// <param name="hasStarted"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithHasStarted(bool hasStarted)
    {
        return With<FakeHttpResponse>(
            o => o.SetHasStarted(hasStarted)
        );
    }

    /// <summary>
    /// Add a handler for when OnStarting is called
    /// </summary>
    /// <param name="handler"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithOnStartingHandler(
        Action<Func<object, Task>, object> handler
    )
    {
        return With<FakeHttpResponse>(
            o => o.AddOnStartingHandler(handler)
        );
    }

    /// <summary>
    /// Add a handler for when the response completes
    /// </summary>
    /// <param name="handler"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithOnCompletedHandler(
        Action<Func<object, Task>, object> handler
    )
    {
        return With<FakeHttpResponse>(
            o => o.AddOnCompletedHandler(handler)
        );
    }

    /// <summary>
    /// Add a handler for when the response redirect
    /// </summary>
    /// <param name="handler"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithRedirectHandler(
        Action<string, bool> handler
    )
    {
        return With<FakeHttpResponse>(
            o => o.AddRedirectHandler(handler)
        );
    }

    /// <summary>
    /// Set the response cookies (opaque service which can only _set_ cookies)
    /// </summary>
    /// <param name="cookies"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithCookies(IResponseCookies cookies)
    {
        return With<FakeHttpResponse>(
            o => o.SetCookies(cookies)
        );
    }

    /// <summary>
    /// Set the response cookies
    /// </summary>
    /// <param name="generator"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithCookies(
        Func<FakeHttpResponse, IResponseCookies> generator
    )
    {
        return With<FakeHttpResponse>(
            o => o.SetCookies(generator(o))
        );
    }

    /// <summary>
    /// Set the status code for the response
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithStatusCode(HttpStatusCode code)
    {
        return WithStatusCode((int)code);
    }

    /// <summary>
    /// Set the status code for the response
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithStatusCode(int code)
    {
        return With(
            o => o.StatusCode = code
        );
    }

    private static void Actualize(HttpResponse o)
    {
        WarnIf(o.HttpContext is null, "o.HttpContext is null");
        WarnIf(o.HttpContext?.Response is null, "o.HttpContext.Response is null");
    }

    /// <summary>
    /// Constructs the fake http response
    /// </summary>
    /// <returns></returns>
    protected override HttpResponse ConstructEntity()
    {
        return new FakeHttpResponse();
    }

    /// <summary>
    /// Randomizes the response
    /// </summary>
    /// <returns></returns>
    public override HttpResponseBuilder Randomize()
    {
        return WithStatusCode(GetRandom<HttpStatusCode>());
    }

    /// <summary>
    /// Sets the http context on the response
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithHttpContext(HttpContext context)
    {
        return WithHttpContext(() => context);
    }

    /// <summary>
    /// Sets the http context accessor on the response
    /// </summary>
    /// <param name="accessor"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithHttpContext(
        Func<HttpContext> accessor
    )
    {
        return With<FakeHttpResponse>(
            o => o.SetContextAccessor(accessor),
            nameof(FakeHttpResponse.HttpContext)
        );
    }

    /// <summary>
    /// Sets the body from utf8-encoded text
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithBody(
        string body
    )
    {
        return WithBody(body, Encoding.UTF8);
    }

    /// <summary>
    /// Sets the body from the provided text, using the provided encoding
    /// </summary>
    /// <param name="body"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithBody(
        string body,
        Encoding encoding
    )
    {
        return WithBody(encoding.GetBytes(body));
    }

    /// <summary>
    /// Sets the body from a stream
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithBody(
        Stream body
    )
    {
        return WithBody(body.ReadAllBytes());
    }

    /// <summary>
    /// Sets the body from some bytes
    /// </summary>
    /// <param name="body"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithBody(
        byte[] body
    )
    {
        return With(o => o.Body.Write(body));
    }

    /// <summary>
    /// Adds a cookie to the response, with default cookie options
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithCookie(string key, string value)
    {
        return WithCookie(key, value, new CookieOptions());
    }

    /// <summary>
    /// Adds a cookie to the response with the provided cookie options
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithCookie(
        string key,
        string value,
        CookieOptions options
    )
    {
        return With(
            o => o.Cookies.Append(
                key,
                value,
                options
            )
        );
    }

    /// <summary>
    /// Sets a header on the response
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithHeader(
        string key,
        string value
    )
    {
        return With(o => o.Headers[key] = value);
    }

    /// <summary>
    /// Sets one or more headers on the response
    /// </summary>
    /// <param name="headers"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithHeaders(
        IDictionary<string, string> headers
    )
    {
        return headers.Aggregate(
            this,
            (acc, cur) => acc.WithHeader(cur.Key, cur.Value)
        );
    }

    /// <summary>
    /// Sets one or more headers on the response
    /// </summary>
    /// <param name="headers"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithHeaders(
        NameValueCollection headers
    )
    {
        return headers.AllKeys.Aggregate(
            this,
            (acc, cur) => acc.WithHeader(cur, headers[cur])
        );
    }
}