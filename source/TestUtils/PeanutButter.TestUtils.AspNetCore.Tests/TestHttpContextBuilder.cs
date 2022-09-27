using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.TestUtils.AspNetCore.Utils;
using static NExpect.Expectations;
using static NExpect.AspNetCoreExpectations;

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
        public void AuthenticationShouldThrow()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
#pragma warning disable CS0618
            Expect(() => result.Authentication)
#pragma warning restore CS0618
                .To.Throw<FeatureIsObsoleteException>();
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
            var foo = Substitute.For<HttpContext>();
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
            .Returns(ci =>
            {
                var t = ci.Arg<Type>();
                if (t != typeof(AService))
                {
                    throw new ArgumentException("Unable to make type {t}");
                }

                return expected;
            });
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
        Expect(sut.Aborted)
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
        Expect(req.Query["foo"])
            .To.Equal("bar");
        Expect(req.Query["one"])
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
        Expect(req.Query["foo"])
            .To.Equal("bar");
        Expect(req.Query["one"])
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
        var expected1 = new FakeCookie(key1, value1, new CookieOptions()
        {
            Path = path,
            Domain = domain,
            MaxAge = TimeSpan.FromSeconds(maxAge),
            Secure = true,
            SameSite = SameSiteMode.None,
            HttpOnly = true,
        });
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
        var key  = GetRandomString();
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
    }

    public class AService
    {
        public void DoNothing()
        {
        }
    }
}