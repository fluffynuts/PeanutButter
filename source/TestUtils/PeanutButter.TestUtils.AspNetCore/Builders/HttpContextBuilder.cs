using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Builders;

public class HttpContextBuilder : Builder<HttpContextBuilder, HttpContext>
{
    protected override HttpContext ConstructEntity()
    {
        return new FakeHttpContext();
    }

    public override HttpContextBuilder Randomize()
    {
        // FIXME
        throw new NotImplementedException();
    }

    public HttpContextBuilder() : this(null, null)
    {
    }

    public HttpContextBuilder(
        Func<HttpRequest> requestAccessor,
        Func<HttpResponse> responseAccessor
    ) : base(Actualize)
    {
        WithFeatures(new FakeFeatureCollection())
            .WithResponse(
                responseAccessor ??
                (() => HttpResponseBuilder.Create()
                    .Build())
            )
            .WithConnection(GetRandom<FakeConnectionInfo>())
            .WithUser(GetRandom<ClaimsPrincipal>())
            .WithRequest(
                requestAccessor ??
                (() => HttpRequestBuilder.Create()
                    .Build())
            );
    }

    private static void Actualize(HttpContext context)
    {
        WarnIf(context.Response is null, "context.Response is null");
        WarnIf(context.Response?.HttpContext is null, "context.Response.HttpContext is null");
        WarnIf(context.Request is null, "context.Request is null");
        WarnIf(context.Request?.HttpContext is null, "context.Request.HttpContext is null");
    }

    public HttpContextBuilder WithRequestServices(IServiceProvider serviceProvider)
    {
        return With(
            o => o.RequestServices = serviceProvider
        );
    }

    public HttpContextBuilder WithItem(object key, object value)
    {
        return With(
            o => o.Items[key] = value
        );
    }

    public HttpContextBuilder WithWebSockets(WebSocketManager webSocketManager)
    {
        return With<FakeHttpContext>(
            o => o.SetWebSockets(webSocketManager)
        );
    }

    public HttpContextBuilder WithFeature<TFeature>(TFeature feature)
    {
        return With(
            o => o.Features.Set(feature)
        );
    }

    public HttpContextBuilder WithUser(ClaimsPrincipal principal)
    {
        return With(
            o => o.User = principal
        );
    }

    public HttpContextBuilder WithConnection(
        ConnectionInfo connectionInfo
    )
    {
        return With<FakeHttpContext>(
            o => o.SetConnection(connectionInfo)
        );
    }

    public HttpContextBuilder WithResponse(
        HttpResponse response
    )
    {
        return WithResponse(() => response);
    }

    public HttpContextBuilder WithResponse(
        Func<HttpResponse> accessor
    )
    {
        return With<FakeHttpContext>(
            o => o.SetResponseAccessor(accessor),
            nameof(FakeHttpContext.Response)
        );
    }

    public HttpContextBuilder WithRequest(
        HttpRequest request
    )
    {
        return WithRequest(() => request);
    }

    public HttpContextBuilder WithRequest(
        Func<HttpRequest> requestAccessor
    )
    {
        return With<FakeHttpContext>(
            o => o.SetRequestAccessor(requestAccessor),
            nameof(FakeHttpContext.Request)
        );
    }

    public HttpContextBuilder WithFeatures(
        IFeatureCollection features
    )
    {
        return With<FakeHttpContext>(
            o => o.SetFeatures(features ?? new FakeFeatureCollection())
        );
    }

    public HttpContextBuilder WithFormFile(string expected, string name, string fileName = null)
    {
        return With(
            o =>
            {
                var request = o.Request as FakeHttpRequest ??
                    throw new InvalidImplementationException(o.Request, nameof(o.Request));
                var files = request.Form?.Files as FakeFormFileCollection
                    ?? throw new InvalidImplementationException(request?.Form?.Files, "Request.Form.Files");
                files.Add(new FakeFormFile(expected, name, fileName));
            });
    }
}