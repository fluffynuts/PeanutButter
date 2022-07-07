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

    internal static HttpContextBuilder CreateWithRequestFactory(
        Func<HttpRequest> requestFactory
    )
    {
        return new(requestFactory);
    }

    public override HttpContextBuilder Randomize()
    {
        // FIXME
        throw new NotImplementedException();
    }

    public HttpContextBuilder() : this(requestFactory: null)
    {
    }

    internal HttpContextBuilder(Func<HttpRequest> requestFactory)
    {
        WithFeatures(new FakeFeatureCollection())
            .WithResponse(new FakeHttpResponse())
            .WithConnection(GetRandom<FakeConnectionInfo>())
            .WithUser(GetRandom<ClaimsPrincipal>());
        if (requestFactory is null)
        {
            WithRequest(HttpRequestBuilder.BuildDefault);
        }
        else
        {
            WithRequest(requestFactory);
        }
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
        FakeHttpResponse response
    )
    {
        return WithResponse(() => response);
    }

    public HttpContextBuilder WithResponse(
        Func<HttpResponse> accessor
    )
    {
        return With<FakeHttpContext>(
            o => o.SetResponseAccessor(accessor)
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
            o => o.SetRequestAccessor(requestAccessor)
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