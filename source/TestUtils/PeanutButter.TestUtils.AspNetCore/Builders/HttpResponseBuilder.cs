using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Fakes;
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

    internal HttpResponseBuilder(bool noContext) : base(Actualize)
    {
        WithStatusCode(HttpStatusCode.OK)
            .WithCookies(FakeResponseCookies.CreateSubstitutedIfPossible());
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
        Action<string, bool> handler)
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
    /// Set the status code for the response
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public HttpResponseBuilder WithStatusCode(HttpStatusCode code)
    {
        return WithStatusCode((int) code);
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
}