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

    public static HttpContextBuilder CreateFor(
        Func<HttpRequest> requestAccessor,
        Func<HttpResponse> responseAccessor
    )
    {
        return new HttpContextBuilder(
            requestAccessor,
            responseAccessor
        );
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
        return WithResponse(() => response)
            .With(o =>
            {
                if (o.Response is FakeHttpResponse fake)
                {
                    fake.SetContextAccessor(() => CurrentEntity);
                }
            });
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
        return WithRequest(() => request)
            .With(o =>
            {
                if (o.Request is FakeHttpRequest fake)
                {
                    fake.SetContextAccessor(() => CurrentEntity);
                }
            });
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