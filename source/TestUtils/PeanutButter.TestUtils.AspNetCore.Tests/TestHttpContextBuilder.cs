using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.TestUtils.AspNetCore.Utils;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestHttpContextBuilder
{
    [TestFixture]
    public class DefaultBuild
    {
        [Test]
        public void ShouldBuildAnHttpContext()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result)
                .Not.To.Be.Null();
            Expect(result)
                .To.Be.An.Instance.Of<HttpContext>();
        }

        [Test]
        public void ShouldSetFeatures()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Features)
                .Not.To.Be.Null();
        }

        [Test]
        public void ShouldSetRequest()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Request)
                .Not.To.Be.Null();
            Expect(result.Request.HttpContext)
                .To.Be(result);
        }

        [Test]
        public void ShouldSetResponse()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Response)
                .Not.To.Be.Null();
            Expect(result.Response.HttpContext)
                .To.Be(result);
        }

        [Test]
        public void ShouldSetWebSocketManager()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.WebSockets)
                .Not.To.Be.Null();
        }

        [Test]
        public void ShouldSetConnection()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Connection)
                .Not.To.Be.Null();
        }

        [Test]
        public void ShouldSetUser()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.User)
                .Not.To.Be.Null();
        }

        [Test]
        public void ShouldSetItems()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Items)
                .Not.To.Be.Null();
        }

        [Test]
        public void ShouldSetMinimalServiceProvider()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.RequestServices)
                .Not.To.Be.Null();
            Expect(result.RequestServices.GetService(typeof(AService)))
                .To.Be.An.Instance.Of<AService>();
        }

        [Test]
        public void ShouldSetRequestAbortedCancellationToken()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.RequestAborted)
                .Not.To.Be.Null();
            Expect(result.RequestAborted.IsCancellationRequested)
                .To.Be.False();
        }

        [Test]
        public void ShouldSetTraceIdentifierToGuid()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.TraceIdentifier)
                .Not.To.Be.Null.Or.Empty();
            Expect(result.TraceIdentifier)
                .To.Be.A.Guid();
        }

        [Test]
        public void ShouldSetSession()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Session)
                .Not.To.Be.Null();
        }

        [Test]
        public void ShouldHaveValidRequestUrl()
        {
            // Arrange
            var result = BuildDefault();
            // Act
            Expect(() => new Uri(result.Request.GetDisplayUrl()))
                .Not.To.Throw();
            // Assert
        }

        private static HttpContext BuildDefault()
        {
            return HttpContextBuilder.BuildDefault();
        }
    }

    [Test]
    public void ShouldBeAbleToSpecifyFeatures()
    {
        // Arrange
        var expected = Substitute.For<IFeatureCollection>();
        // Act
        var result = HttpContextBuilder.Create()
            .WithFeatures(expected)
            .Build();
        // Assert
        Expect(result.Features)
            .To.Be(expected);
    }

    [Test]
    public void ShouldBeAbleToSpecifyAFeature()
    {
        // Arrange
        var feature = new SomeFeature();
        // Act
        var result = HttpContextBuilder.Create()
            .WithFeature(feature)
            .Build();
        // Assert
        Expect(result.Features.Get<SomeFeature>())
            .To.Be(feature);
    }

    private class SomeFeature
    {
    }

    [Test]
    public void ShouldBeAbleToSetRequest()
    {
        // Arrange
        var expected = HttpRequestBuilder.BuildDefault();
        // Act
        var result = HttpContextBuilder.Create()
            .WithRequest(expected)
            .Build();
        // Assert
        Expect(result.Request)
            .To.Be(expected);
        Expect(result.Request.HttpContext)
            .To.Be(result);
    }

    [Test]
    public void ShouldBeAbleToSetResponse()
    {
        // Arrange
        var expected = HttpResponseBuilder.BuildDefault();
        // Act
        var result = HttpContextBuilder.Create()
            .WithResponse(expected)
            .Build();

        // Assert
        Expect(result.Response)
            .To.Be(expected);
    }

    [Test]
    public void ShouldBeAbleToSetWebSocketManager()
    {
        // Arrange
        var expected = Substitute.For<WebSocketManager>();
        // Act
        var result = HttpContextBuilder.Create()
            .WithWebSockets(expected)
            .Build();
        // Assert
        Expect(result.WebSockets)
            .To.Be(expected);
    }

    [Test]
    public void ShouldBeAbleToSetUserAsPrincipal()
    {
        // Arrange
        var user = new ClaimsPrincipal();
        // Act
        var result = HttpContextBuilder.Create()
            .WithUser(user)
            .Build();
        // Assert
        Expect(result.User)
            .To.Be(user);
    }

    [Test]
    public void ShouldBeAbleToAddItem()
    {
        // Arrange
        var key = new object();
        var value = new object();
        // Act
        var result = HttpContextBuilder.Create()
            .WithItem(key, value)
            .Build();
        // Assert
        Expect(result.Items)
            .To.Contain.Key(key)
            .With.Value(value);
    }

    [Test]
    public void ShouldBeAbleToSetServices()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var expected = new AService();
        serviceProvider.GetService(Arg.Any<Type>())
            .Returns(
                ci =>
                {
                    var t = ci.Arg<Type>();
                    if (t != typeof(AService))
                    {
                        throw new ArgumentException(
                            $"Unable to make type {t}",
                            nameof(t)
                        );
                    }

                    return expected;
                }
            );
        // Act
        var result = HttpContextBuilder.Create()
            .WithRequestServices(serviceProvider)
            .Build();
        // Assert
        var resolved = result.RequestServices.GetService(typeof(AService));
        Expect(resolved)
            .To.Be(expected);
    }

    [Test]
    public void ShouldRecordBeingAborted()
    {
        // Arrange
        var sut = HttpContextBuilder.BuildDefault() as FakeHttpContext;
        Expect(sut!.Aborted)
            .To.Be.False();
        // Act
        sut.Abort();
        // Assert
        Expect(sut.Aborted)
            .To.Be.True();
    }

    [Test]
    public void ShouldBeAbleToAddAFormFile()
    {
        // Arrange
        var data = GetRandomString();
        var name = GetRandomString();
        // Act
        var result = HttpContextBuilder.Create()
            .WithFormFile(data, name)
            .Build();

        // Assert
        var file = result.Request.Form.Files.GetFile(name);
        Expect(file)
            .Not.To.Be.Null();
        Expect(file.ReadAllText())
            .To.Equal(data);
    }

    [Test]
    public void ShouldBeAbleToSetAFormField()
    {
        // Arrange
        var key = GetRandomString();
        var value = GetRandomString();

        // Act
        var result = HttpContextBuilder.Create()
            .WithFormField(key, value)
            .Build();
        // Assert
        Expect(result.Request.Form)
            .To.Contain.Key(key)
            .With.Value(value);
    }

    [Test]
    public void ShouldBeAbleToBuildRandom()
    {
        // Arrange
        // Act
        var result1 = GetRandom<HttpContext>();
        var result2 = GetRandom<HttpContext>();
        // Assert
        Expect(result1)
            .Not.To.Deep.Equal(result2);
        Expect(result1.Request.Host.Host)
            .Not.To.Equal("localhost");
    }

    [Test]
    public void ShouldBeAbleToSetUrl()
    {
        // Arrange
        var url = "http://some.host.com/path/to/resource?foo=bar&one=1";
        // Act
        var result = HttpContextBuilder.Create()
            .WithUrl(url)
            .Build();
        // Assert
        var req = result.Request;
        Expect(req.Host.Value)
            .To.Equal("some.host.com");
        Expect(req.Host.Port)
            .To.Be.Null();
        Expect(req.Path)
            .To.Equal("/path/to/resource");
        Expect($"{req.Query["foo"]}")
            .To.Equal("bar");
        Expect($"{req.Query["one"]}")
            .To.Equal("1");
    }

    [Test]
    public void ShouldBeAbleToSetUrlWithCustomPort()
    {
        // Arrange
        var url = "https://some.host.com:1234/path/to/resource?foo=bar&one=1";
        // Act
        var result = HttpContextBuilder.Create()
            .WithUrl(url)
            .Build();
        // Assert
        var req = result.Request;
        Expect(req.Host.Host)
            .To.Equal("some.host.com");
        Expect(req.Host.Port)
            .To.Equal(1234);
        Expect(req.Path)
            .To.Equal("/path/to/resource");
        Expect($"{req.Query["foo"]}")
            .To.Equal("bar");
        Expect($"{req.Query["one"]}")
            .To.Equal("1");
    }

    [Test]
    public void SettingAResponseCookieHeaderShouldReflectInResponseCookiesAsFake()
    {
        // Arrange
        var ctx = HttpContextBuilder.BuildDefault();
        var response = ctx.Response;
        Expect(response.Headers["set-cookie"].ToArray())
            .To.Be.Empty();
        var responseCookies = ctx.Response.Cookies as FakeResponseCookies;

        var key1 = GetRandomString();
        var value1 = GetRandomString();
        var domain = GetRandomHostname();
        var path = $"/{GetRandomString()}";
        var maxAge = GetRandomInt(100, 200);
        var key2 = GetRandomString(10);
        var value2 = GetRandomString(10);
        var header1 =
            $"{key1}={value1}; Domain={domain}; Path={path}; Max-Age={maxAge}; Secure; HttpOnly; SameSite=None";
        var header2 = $"{key2}={value2}";
        var expected1 = new FakeCookie(
            key1,
            value1,
            new CookieOptions()
            {
                Path = path,
                Domain = domain,
                MaxAge = TimeSpan.FromSeconds(maxAge),
                Secure = true,
                SameSite = SameSiteMode.None,
                HttpOnly = true,
            }
        );
        var expected2 = new FakeCookie(key2, value2, new CookieOptions());
        // Act
        response.Headers["set-cookie"] = new StringValues(
            new[]
            {
                header1,
                header2
            }
        );
        var result1 = responseCookies![key1];
        var result2 = responseCookies[key2];

        // Assert
        Expect(result1)
            .To.Deep.Equal(expected1);
        Expect(result2)
            .To.Deep.Equal(expected2);
    }

    [Test]
    public void SettingAResponseCookieShouldReflectInTheResponseCookiesHeader()
    {
        // Arrange
        var key = GetRandomString();
        var value = GetRandomString();
        var expected = new FakeCookie(key, value, new CookieOptions());
        var ctx = HttpContextBuilder.BuildDefault();
        // Act
        ctx.Response.Cookies.Append(expected.Name, expected.Value, expected.Options);
        var result = FakeCookie.Parse(ctx.Response.Headers["set-cookie"].FirstOrDefault());
        // Assert
        Expect(result)
            .To.Deep.Equal(expected);
    }

    [Test]
    public void RemovingAResponseCookieShouldReflectInTheResponseCookiesHeader()
    {
        // Arrange
        var key = GetRandomString();
        var value = GetRandomString();
        var expected = new FakeCookie(key, value, new CookieOptions());
        var ctx = HttpContextBuilder.BuildDefault();
        ctx.Response.Cookies.Append(expected.Name, expected.Value, expected.Options);
        // Act
        ctx.Response.Cookies.Delete(key);
        // Assert
        Expect(ctx.Response.Headers["set-cookie"].ToArray())
            .To.Be.Empty();
    }

    [Test]
    public void SettingARequestCookieShouldSetTheCookieHeaderOnTheRequest()
    {
        // Arrange
        var key = GetRandomString();
        var value = GetRandomString();
        var ctx = HttpContextBuilder.BuildDefault();
        var requestCookies = ctx.Request.Cookies as FakeRequestCookieCollection;
        Expect(requestCookies!.HttpRequest)
            .To.Be(ctx.Request);
        // Act
        requestCookies[key] = value;
        var result = ctx.Request.Headers["Cookie"].FirstOrDefault();
        // Assert
        Expect(result)
            .Not.To.Be.Null();
        Expect(result)
            .To.Equal($"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(value)}");
    }

    [Test]
    public void SettingACookieInTheHeaderShouldUpdateTheCollection()
    {
        // Arrange
        var key = GetRandomString();
        var value = GetRandomString();
        var ctx = HttpContextBuilder.BuildDefault();
        // Act
        ctx.Request.Headers["Cookie"] = $"{WebUtility.UrlEncode(key)}={WebUtility.UrlEncode(value)}";
        // Assert
        Expect(ctx.Request.Cookies)
            .To.Contain.Key(key)
            .With.Value(value);
    }

    [TestFixture]
    public class DirectRequestManipulation
    {
        [Test]
        public void ShouldBeAbleToSetPathDirectly()
        {
            // Arrange
            var expected = GetRandomAbsolutePath();
            var ctx = HttpContextBuilder.Create()
                .WithRequestPath(expected)
                .Build();
            // Act
            var result = ctx.Request.Path;
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldBeAbleToSetContentTypeDirectly()
        {
            // Arrange
            var expected = GetRandomString();
            var ctx = HttpContextBuilder.Create()
                .WithRequestContentType(expected)
                .Build();
            // Act
            var result = ctx.Request.ContentType;
            // Assert
            Expect(result)
                .To.Equal(expected);
            Expect($"{ctx.Request.Headers["Content-Type"]}")
                .To.Equal(expected);
        }

        [Test]
        public void ShouldBeAbleToSetContentTypeIndirectlyViaHeader()
        {
            // Arrange
            var expected = GetRandomString();
            var ctx = HttpContextBuilder.Create()
                .WithRequestHeader("Content-Type", expected)
                .Build();
            // Act
            var result = ctx.Request.ContentType;
            // Assert
            Expect(result)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    public class CustomisingAvailableServices
    {
        [Test]
        public void ShouldBeAbleToRegisterTransient()
        {
            // Arrange
            var ctx = HttpContextBuilder.Create()
                .WithTransientService<ISomeService, SomeService>()
                .Build();
            // Act
            var result1 = ctx.RequestServices.GetService(typeof(ISomeService));
            var result2 = ctx.RequestServices.GetService(typeof(ISomeService));
            // Assert
            Expect(result1)
                .To.Be.An.Instance.Of<SomeService>();
            Expect(result2)
                .To.Be.An.Instance.Of<SomeService>();
            Expect(result1)
                .Not.To.Be(result2);
        }

        [Test]
        public void ShouldBeAbleToRegisterSingleton()
        {
            // Arrange
            var ctx = HttpContextBuilder.Create()
                .WithSingletonService<ISomeService, SomeService>()
                .Build();
            // Act
            var result1 = ctx.RequestServices.GetService(typeof(ISomeService));
            var result2 = ctx.RequestServices.GetService(typeof(ISomeService));
            // Assert
            Expect(result1)
                .To.Be.An.Instance.Of<SomeService>();
            Expect(result2)
                .To.Be.An.Instance.Of<SomeService>();
            Expect(result1)
                .To.Be(result2);
        }

        [Test]
        public void ShouldBeAbleToRegisterAnInstance()
        {
            // Arrange
            var expected = new SomeService();
            var ctx = HttpContextBuilder.Create()
                .WithService<ISomeService>(expected)
                .Build();
            // Act
            var result = ctx.RequestServices.GetService(typeof(ISomeService));
            // Assert
            Expect(result)
                .To.Be(expected);
        }

        [Test]
        public void ShouldBeAbleToRegisterAFactory()
        {
            // Arrange
            var expected = new SomeService();
            var ctx = HttpContextBuilder.Create()
                .WithService<ISomeService>(() => expected)
                .Build();
            // Act
            var result = ctx.RequestServices.GetService(typeof(ISomeService));
            // Assert
            Expect(result)
                .To.Be(expected);
        }

        public interface ISomeService
        {
            void NoOp();
        }

        public class SomeService : ISomeService
        {
            public void NoOp()
            {
            }
        }

        [Test]
        public void ShouldBeAbleToSetRequestHeader()
        {
            // Arrange
            var key = GetRandomString();
            var value = GetRandomString();
            // Act
            var result = HttpContextBuilder.Create()
                .WithRequestHeader(key, value)
                .Build();
            // Assert
            Expect(result.Request.Headers)
                .To.Contain.Key(key)
                .With.Value(value);
        }

        [Test]
        public void ShouldBeAbleToEasilySetARequestCookie()
        {
            // Arrange
            var key = GetRandomString();
            var value = GetRandomString();
            // Act
            var result = HttpContextBuilder.Create()
                .WithRequestCookie(key, value)
                .Build();
            // Assert
            Expect(result.Request.Cookies)
                .To.Contain.Key(key)
                .With.Value(value);
        }

        [Test]
        public void ShouldBeAbleToSetResponseHeader()
        {
            // Arrange
            var key = GetRandomString();
            var value = GetRandomString();
            // Act
            var result = HttpContextBuilder.Create()
                .WithResponseHeader(key, value)
                .Build();
            // Assert
            Expect(result.Response.Headers)
                .To.Contain.Key(key)
                .With.Value(value);
        }

        [TestCase(typeof(IHttpResponseFeature))]
        [TestCase(typeof(IHttpRequestFeature))]
        public void ResponseShouldHaveFeature_(Type expected)
        {
            // Arrange
            var ctx = HttpContextBuilder.BuildDefault();
            // Act
            var genericMethod = ctx.Features.GetType().GetMethod("Get");
            var method = genericMethod!.MakeGenericMethod(expected);
            var result = method.Invoke(ctx.Features, Array.Empty<object>());
            // Assert
            Expect(result)
                .Not.To.Be.Null();
            Expect(result)
                .To.Be.An.Instance.Of(expected);
        }

        [TestFixture]
        public class HttpResponseFeature
        {
            [Test]
            public void ShouldDoNothingOnStarting()
            {
                // Arrange
                var ctx = HttpContextBuilder.BuildDefault();
                var feature = ctx.Features.Get<IHttpResponseFeature>();
                // Act
                Expect(() => feature!.OnStarting(_ => Task.CompletedTask, new object()))
                    .Not.To.Throw();
                // Assert
            }

            [Test]
            public void ShouldDoNothingOnCompleted()
            {
                // Arrange
                var ctx = HttpContextBuilder.BuildDefault();
                var feature = ctx.Features.Get<IHttpResponseFeature>();
                // Act
                Expect(() => feature!.OnCompleted(_ => Task.CompletedTask, new object()))
                    .Not.To.Throw();
                // Assert
            }

            [TestFixture]
            public class Props
            {
                [Test]
                public void ShouldHaveOkStatusCode()
                {
                    // Arrange
                    var ctx = HttpContextBuilder.BuildDefault();
                    var feature = ctx.Features.Get<IHttpResponseFeature>();
                    // Act
                    // Assert
                    Expect(feature!.StatusCode)
                        .To.Equal((int)HttpStatusCode.OK);
                }

                [TestCase("OK")]
                public void ShouldHaveReasonPhrase_(string expected)
                {
                    // Arrange
                    var ctx = HttpContextBuilder.BuildDefault();
                    var feature = ctx.Features.Get<IHttpResponseFeature>();
                    // Act
                    // Assert
                    Expect(feature!.ReasonPhrase)
                        .To.Equal(expected);
                }

                [Test]
                public void StatusCodeShouldTrackResponse()
                {
                    // Arrange
                    var ctx = HttpContextBuilder.BuildDefault();
                    var feature = ctx.Features.Get<IHttpResponseFeature>();
                    // Act
                    ctx.Response.StatusCode = (int)GetRandom<HttpStatusCode>();
                    Expect(feature!.StatusCode)
                        .To.Equal(ctx.Response.StatusCode);
                    feature.StatusCode = (int)GetRandom<HttpStatusCode>();
                    Expect(ctx.Response.StatusCode)
                        .To.Equal(feature.StatusCode);
                    // Assert
                }

                [Test]
                public void ShouldHaveEmptyHeaders()
                {
                    // Arrange
                    var ctx = HttpContextBuilder.BuildDefault();
                    var feature = ctx.Features.Get<IHttpResponseFeature>();
                    // Act
                    // Assert
                    Expect(feature!.Headers)
                        .To.Be.Empty();
                    Expect(feature.Headers)
                        .To.Equal(ctx.Response.Headers);
                }

                [Test]
                public void HeadersShouldTrackResponse()
                {
                    // Arrange
                    var ctx = HttpContextBuilder.BuildDefault();
                    var key = GetRandomString();
                    var feature = ctx.Features.Get<IHttpResponseFeature>();
                    // Act
                    feature!.Headers[key] = GetRandomString();
                    Expect(ctx.Response.Headers[key])
                        .To.Equal(feature.Headers[key]);
                    ctx.Response.Headers[key] = GetRandomString();
                    Expect(feature.Headers[key])
                        .To.Equal(ctx.Response.Headers[key]);
                    // Assert
                }

#pragma warning disable CS0618
                [Test]
                public void BodyShouldRedirectToResponse()
                {
                    // Arrange
                    var ctx = HttpContextBuilder.BuildDefault();
                    var feature = ctx.Features.Get<IHttpResponseFeature>();
                    // Act
                    Expect(feature!.Body)
                        .To.Be(ctx.Response.Body);
                    feature.Body = new MemoryStream();
                    Expect(ctx.Response.Body)
                        .To.Be(feature.Body);
                    // Assert
                }

                [Test]
                public void HasStartedShouldRedirectToResponse()
                {
                    // Arrange
                    var ctx = HttpContextBuilder.BuildDefault();
                    var feature = ctx.Features.Get<IHttpResponseFeature>();
                    // Act
                    Expect(feature!.HasStarted)
                        .To.Equal(ctx.Response.HasStarted);
                    var response = ctx.Response as FakeHttpResponse;
                    response!.SetHasStarted(true);
                    Expect(feature.HasStarted)
                        .To.Equal(ctx.Response.HasStarted);
                    // Assert
                }
#pragma warning restore CS0618
            }
        }

        [TestFixture]
        public class HttpContextItems
        {
            [Test]
            public void ShouldBeAbleToSetAndRetrieve()
            {
                // Arrange
                var sut = HttpContextBuilder.BuildDefault();
                var key = GetRandomString();
                var value = GetRandomString();
                // Act
                sut.Items[key] = value;
                // Assert
                Expect(sut.Items[key])
                    .To.Equal(value);
            }

            [TestFixture]
            public class WhenItemNotFound
            {
                [Test]
                public void ShouldReturnNull()
                {
                    // this is what a real HttpContext does
                    // Arrange
                    var sut = HttpContextBuilder.BuildDefault();
                    var key = GetRandomString();
                    // Act
                    var result = sut.Items[key];
                    // Assert
                    Expect(result)
                        .To.Be.Null();
                }

                [Test]
                public void ShouldReportMissingKey()
                {
                    // Arrange
                    var sut = HttpContextBuilder.BuildDefault();
                    var key = GetRandomString();
                    // Act
                    var result = sut.Items.ContainsKey(key);
                    // Assert
                    Expect(result)
                        .To.Be.False();
                }
            }
        }

        [TestFixture]
        public class HttpRequestFeature
        {
            [TestFixture]
            public class Props
            {
                [Test]
                public void ProtocolShouldTieBackToRequest()
                {
                    // Arrange
                    var ctx = HttpContextBuilder.BuildDefault();
                    var feature = ctx.Features.Get<IHttpRequestFeature>();
                    // Act
                    feature!.Protocol = GetRandomString(4, 4);
                    Expect(ctx.Request.Protocol)
                        .To.Equal(feature.Protocol);
                    ctx.Request.Protocol = GetRandomString(4, 4);
                    Expect(feature.Protocol)
                        .To.Equal(ctx.Request.Protocol);
                    // Assert
                }

                [Test]
                public void SchemeShouldTieBackToRequest()
                {
                    // Arrange
                    var ctx = HttpContextBuilder.BuildDefault();
                    var feature = ctx.Features.Get<IHttpRequestFeature>();
                    // Act
                    feature!.Scheme = GetRandomString(4, 4);
                    Expect(ctx.Request.Scheme)
                        .To.Equal(feature.Scheme);
                    ctx.Request.Scheme = GetRandomString(4, 4);
                    Expect(feature.Scheme)
                        .To.Equal(ctx.Request.Scheme);
                    // Assert
                }

                [Test]
                public void MethodShouldTieBackToRequest()
                {
                    // Arrange
                    var ctx = HttpContextBuilder.BuildDefault();
                    var feature = ctx.Features.Get<IHttpRequestFeature>();
                    // Act
                    feature!.Method = GetRandomString(4, 4);
                    Expect(ctx.Request.Method)
                        .To.Equal(feature.Method);
                    ctx.Request.Method = GetRandomString(4, 4);
                    Expect(feature.Method)
                        .To.Equal(ctx.Request.Method);
                    // Assert
                }

                [Test]
                public void PathBaseShouldTieBackToRequest()
                {
                    // Arrange
                    var ctx = HttpContextBuilder.BuildDefault();
                    var feature = ctx.Features.Get<IHttpRequestFeature>();
                    // Act
                    feature!.PathBase = $"/{GetRandomString(4, 4)}";
                    Expect(ctx.Request.PathBase)
                        .To.Equal(feature.PathBase);
                    ctx.Request.PathBase = $"/{GetRandomString(4, 4)}";
                    Expect(feature.PathBase)
                        .To.Equal(ctx.Request.PathBase);
                    // Assert
                }

                [Test]
                public void PathShouldTieBackToRequest()
                {
                    // Arrange
                    var ctx = HttpContextBuilder.BuildDefault();
                    var feature = ctx.Features.Get<IHttpRequestFeature>();
                    // Act
                    feature!.Path = $"/{GetRandomString(4, 4)}";
                    Expect(ctx.Request.Path)
                        .To.Equal(feature.Path);
                    ctx.Request.Path = $"/{GetRandomString(4, 4)}";
                    Expect(feature.Path)
                        .To.Equal(ctx.Request.Path);
                    // Assert
                }

                [Test]
                public void QueryStringShouldTieBackToRequest()
                {
                    // Arrange
                    var ctx = HttpContextBuilder.BuildDefault();
                    var feature = ctx.Features.Get<IHttpRequestFeature>();
                    // Act
                    feature!.QueryString = $"?{GetRandomString(4, 4)}";
                    Expect(ctx.Request.QueryString.ToString())
                        .To.Equal(feature.QueryString);
                    ctx.Request.QueryString = new QueryString($"?{GetRandomString(4, 4)}");
                    Expect(feature.QueryString)
                        .To.Equal(ctx.Request.QueryString.ToString());
                    // Assert
                }

                [Test]
                public void RawTargetShouldTieBackToRequest()
                {
                    // Arrange
                    var ctx = HttpContextBuilder.BuildDefault();
                    var feature = ctx.Features.Get<IHttpRequestFeature>();
                    var req = ctx.Request;
                    var expected = $"{req.Method.ToUpper()} {req.FullUrl()} HTTP/1.1";
                    // Act
                    var result = feature!.RawTarget;
                    // Assert
                    Expect(result)
                        .To.Equal(expected);
                }

                [Test]
                public void HeadersShouldTrackRequest()
                {
                    // Arrange
                    var ctx = HttpContextBuilder.BuildDefault();
                    var key = GetRandomString();
                    var feature = ctx.Features.Get<IHttpRequestFeature>();
                    // Act
                    feature!.Headers[key] = GetRandomString();
                    Expect(ctx.Request.Headers[key])
                        .To.Equal(feature.Headers[key]);
                    ctx.Request.Headers[key] = GetRandomString();
                    Expect(feature.Headers[key])
                        .To.Equal(ctx.Request.Headers[key]);
                    // Assert
                }

                [Test]
                public void BodyShouldTrackRequestBody()
                {
                    // Arrange
                    var ctx = HttpContextBuilder.BuildDefault();
                    var feature = ctx.Features.Get<IHttpRequestFeature>();
                    // Act
                    Expect(feature!.Body)
                        .To.Be(ctx.Request.Body);
                    feature.Body = new MemoryStream();
                    Expect(ctx.Request.Body)
                        .To.Be(feature.Body);
                    ctx.Request.Body = new MemoryStream();
                    Expect(feature.Body)
                        .To.Be(ctx.Request.Body);
                    // Assert
                }
            }
        }
    }

    [Test]
    public void ShouldBeAbleToSetRemoteAddress()
    {
        // Arrange
        var expected = GetRandomIPv4Address();
        var ctx = HttpContextBuilder.Create()
            .WithRemoteAddress(expected)
            .Build();
        // Act
        var result = ctx.Connection.RemoteIpAddress;
        // Assert
        Expect(result)
            .To.Equal(IPAddress.Parse(expected));
    }

    [Test]
    public void ShouldBeAbleToSetQueryStringParameter()
    {
        // Arrange
        var key = GetRandomString(10);
        var value = GetRandomString(10);
        // Act
        var ctx = HttpContextBuilder.Create()
            .WithRequestQueryParameter(key, value)
            .Build();
        // Assert
        var dict = HttpUtility.ParseQueryString(
            ctx.Request.FullUrl().Query
        );
        Expect(dict)
            .To.Contain.Key(key)
            .With.Value(value);
    }

    [Test]
    public void ShouldBeAbleToSetQueryParametersFromDictionary()
    {
        // Arrange
        var expected = GetRandom<Dictionary<string, string>>();
        // Act
        var ctx = HttpContextBuilder.Create()
            .WithRequestQueryParameters(expected)
            .Build();
        // Assert
        var result = HttpUtility.ParseQueryString(
            ctx.Request.FullUrl().Query
        );
        Expect(result)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldBeAbleToSetQueryParametersFromNameValueCollection()
    {
        // Arrange
        var parameters = GetRandom<NameValueCollection>();
        var expected = parameters.ToDictionary();
        // Act
        var ctx = HttpContextBuilder.Create()
            .WithRequestQueryParameters(expected)
            .Build();
        // Assert
        var result = HttpUtility.ParseQueryString(
            ctx.Request.FullUrl().Query
        );
        Expect(result)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldBeAbleToSetSessionItems()
    {
        // Arrange
        var k1 = GetRandomString();
        var v1 = new IdContainer(GetRandomInt(1));
        var k2 = GetAnother(k1);
        var v2 = GetRandomDate();
        var k3 = GetAnother<string>(new[] { k1, k2 });
        var v3 = GetRandomWords();
        var items = new Dictionary<string, object>()
        {
            [k1] = v1,
            [k2] = v2,
            [k3] = v3
        };
        // Act
        var ctx = HttpContextBuilder.Create()
            .WithSessionItems(items)
            .Build();
        // Assert
        var result1 = ctx.Session.Read<IdContainer>(k1);
        var result2 = ctx.Session.Read<DateTime>(k2);
        var result3 = ctx.Session.GetString(k3);
        
        Expect(result1)
            .To.Deep.Equal(v1);
        Expect(result2)
            .To.Equal(v2);
        Expect(result3)
            .To.Equal(v3);
    }

    public class IdContainer
    {
        public int Id { get; set; }

        public IdContainer(int id)
        {
            Id = id;
        }
    }

    public class AService
    {
        public void DoNothing()
        {
        }
    }
}

internal static class SessionExtensions
{
    internal static T Read<T>(
        this ISession session,
        string key
    )
    {
        if (!session.Keys.Contains(key))
        {
            return default;
        }
        var str = session.GetString(key);
        if (str is null)
        {
            return default;
        }

        return System.Text.Json.JsonSerializer.Deserialize<T>(str);
    }
}
