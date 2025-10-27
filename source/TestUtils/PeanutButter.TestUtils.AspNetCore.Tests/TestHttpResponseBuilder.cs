using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.Utils;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestHttpResponseBuilder
{
    [TestFixture]
    public class DefaultBuild
    {
        [Test]
        public void ShouldHaveHttpContext()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.HttpContext)
                .Not.To.Be.Null();
            Expect(result.HttpContext.Response)
                .To.Be(result);
        }

        [TestCase(200)]
        public void ShouldHaveStatusCode_(int expected)
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.StatusCode)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldHaveEmptyHeaders()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Headers)
                .To.Be.Empty();
        }

        [Test]
        public void ShouldHaveEmptyBody()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Body)
                .Not.To.Be.Null();
            Expect(result.Body.ReadAllBytes())
                .To.Be.Empty();
        }

        [Test]
        public void ShouldHaveZeroContentLength()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.ContentLength)
                .To.Equal(0);
        }

        [Test]
        public void ShouldHaveHtmlContentType()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.ContentType)
                .To.Equal("text/html");
        }

        [Test]
        public void ShouldHaveCookiesObject()
        {
            // Arrange
            var result = BuildDefault();
            // Act
            Expect(result.Cookies)
                .Not.To.Be.Null();
            // Assert
        }

        [Test]
        public void ShouldNotHaveStarted()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.HasStarted)
                .To.Be.False();
        }

        private static HttpResponse BuildDefault()
        {
            return HttpResponseBuilder.BuildDefault();
        }
    }

    [Test]
    public void ShouldBeAbleToAddOnStartedHandler()
    {
        // Arrange
        var called = false;
        Func<object, Task> capturedFunc = null;
        object capturedState = null;
        Func<object, Task> inputFunc = _ => Task.CompletedTask;
        var inputState = new object();
        // Act
        var result = HttpResponseBuilder.Create()
            .WithOnStartingHandler(
                (func, state) =>
                {
                    capturedFunc = func;
                    capturedState = state;
                    called = true;
                }
            )
            .Build();
        // Assert
        result.OnStarting(inputFunc, inputState);
        Expect(called)
            .To.Be.True();
        Expect(capturedFunc)
            .To.Be(inputFunc);
        Expect(capturedState)
            .To.Be(inputState);
    }

    [Test]
    public void ShouldBeAbleToAddOnCompletedHandler()
    {
        // Arrange
        var called = false;
        Func<object, Task> capturedFunc = null;
        object capturedState = null;
        Func<object, Task> inputFunc = _ => Task.CompletedTask;
        var inputState = new object();
        // Act
        var result = HttpResponseBuilder.Create()
            .WithOnCompletedHandler(
                (func, state) =>
                {
                    capturedFunc = func;
                    capturedState = state;
                    called = true;
                }
            )
            .Build();
        // Assert
        result.OnCompleted(inputFunc, inputState);
        Expect(called)
            .To.Be.True();
        Expect(capturedFunc)
            .To.Be(inputFunc);
        Expect(capturedState)
            .To.Be(inputState);
    }

    [Test]
    public void ShouldBeAbleToAddRedirectHandler()
    {
        // Arrange
        var expectedLocation = GetRandomHttpUrl();
        var expectedPermanent = GetRandomBoolean();
        var capturedLocation = null as string;
        var capturedPermanent = null as bool?;
        var called = false;
        // Act
        var result = HttpResponseBuilder.Create()
            .WithRedirectHandler(
                (location, permanent) =>
                {
                    capturedLocation = location;
                    capturedPermanent = permanent;
                    called = true;
                }
            ).Build();
        // Assert
        result.Redirect(expectedLocation, expectedPermanent);
        Expect(called)
            .To.Be.True();
        Expect(capturedLocation)
            .To.Equal(expectedLocation);
        Expect(capturedPermanent)
            .To.Equal(expectedPermanent);
    }

    [Test]
    public void ShouldBeAbleToSetHttpContext()
    {
        // Arrange
        var expected = new FakeHttpContext();
        // Act
        var result = HttpResponseBuilder.Create()
            .WithHttpContext(expected)
            .Build();
        // Assert
        Expect(result.HttpContext)
            .To.Be(expected);
    }

    [Test]
    public void ShouldBeAbleToSetHttpContextDelayed()
    {
        // Arrange
        var expected = new FakeHttpContext();
        // Act
        var result = HttpResponseBuilder.Create()
            .WithHttpContext(() => expected)
            .Build();
        // Assert
        Expect(result.HttpContext)
            .To.Be(expected);
    }

    [Test]
    public void ShouldBeAbleToSetStatusCodeFromEnum()
    {
        // Arrange
        var expected = GetRandom<HttpStatusCode>();
        // Act
        var result = HttpResponseBuilder.Create()
            .WithStatusCode(expected)
            .Build();
        // Assert
        Expect(result.StatusCode)
            .To.Equal((int)expected);
    }

    [Test]
    public void ShouldBeAbleToSetStatusCodeFromInt()
    {
        // Arrange
        var expected = (int)GetRandom<HttpStatusCode>();
        // Act
        var result = HttpResponseBuilder.Create()
            .WithStatusCode(expected)
            .Build();
        // Assert
        Expect(result.StatusCode)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldBeAbleToSetHasStarted()
    {
        // Arrange
        var expected = GetRandomBoolean();
        // Act
        var result = HttpResponseBuilder.Create()
            .WithHasStarted(expected)
            .Build();
        // Assert
        Expect(result.HasStarted)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldBeAbleToBuildRandom()
    {
        // Arrange
        // Act
        var result1 = GetRandom<HttpResponse>();
        var result2 = GetRandom<HttpResponse>();
        // // Assert
        Expect(result1)
            .Not.To.Deep.Equal(result2);
    }

    [Test]
    public async Task ShouldBeAbleTo_WriteAsync()
    {
        // Arrange
        var sut = HttpResponseBuilder.BuildDefault();
        var expected = GetRandomWords();
        // Act
        await sut.WriteAsync(expected);
        var result = await sut.Body.ReadAllTextAsync();
        // Assert
        Expect(result)
            .To.Equal(expected);
    }

    [Test]
    public async Task ShouldBeAbleTo_WriteJsonAsync()
    {
        // Arrange
        var sut = HttpResponseBuilder.BuildDefault();
        var obj = new Data
        {
            Id = 1,
            Name = "bob"
        };
        // Act
        await sut.WriteAsJsonAsync(obj);
        var jsonResult = await sut.Body.ReadAllTextAsync();
        var objResult = JsonConvert.DeserializeObject<Data>(jsonResult);
        // Assert
        Expect(objResult)
            .To.Deep.Equal(obj);
    }

    [Test]
    public async Task ShouldBeAbleToSetStringBody()
    {
        // Arrange
        var expected = GetRandomWords();
        var sut = HttpResponseBuilder.Create()
            .WithBody(expected)
            .Build();

        // Act
        var result = await sut.Body.ReadAllTextAsync();
        // Assert
        Expect(result)
            .To.Equal(expected);
    }

    [Test]
    public async Task ShouldBeAbleToSetBytesBody()
    {
        // Arrange
        var expected = GetRandomBytes();
        var sut = HttpResponseBuilder.Create()
            .WithBody(expected)
            .Build();

        // Act
        var result = await sut.Body.ReadAllBytesAsync();
        // Assert
        Expect(result)
            .To.Deep.Equal(expected);
    }

    [Test]
    public async Task ShouldBeAbleToSetStreamBody()
    {
        // Arrange
        var expected = GetRandomBytes();
        var stream = new MemoryStream(expected);
        var sut = HttpResponseBuilder.Create()
            .WithBody(stream)
            .Build();

        // Act
        var result = await sut.Body.ReadAllBytesAsync();
        // Assert
        Expect(result)
            .To.Deep.Equal(expected);
    }

    [Test]
    public async Task ShouldBeAbleToClear()
    {
        // Arrange
        var sut = HttpResponseBuilder.BuildDefault();

        // Act
        await sut.Body.WriteAsync(GetRandomBytes());
        sut.StatusCode = (int)HttpStatusCode.Found;
        sut.Clear();
        // Assert
        Expect(sut.StatusCode)
            .To.Equal((int)HttpStatusCode.OK);
        Expect(await sut.Body.ReadAllBytesAsync())
            .To.Be.Empty();
    }

    [Test]
    public void ShouldHaveCaseInsensitiveHeaderAccess()
    {
        // Arrange
        var res = HttpResponseBuilder.BuildDefault();
        // Act
        res.Headers["set-cookie"] = "foo=bar";
        // Assert
        Expect($"{res.Headers["Set-Cookie"]}")
            .To.Equal("foo=bar");
    }

    [Test]
    public void ShouldStoreAppendedCookies()
    {
        // Arrange
        var sut = HttpResponseBuilder.BuildDefault();
        var cookieName = GetRandomString();
        var cookieValue = GetRandomString();
        // Act
        sut.Cookies.Append(cookieName, cookieValue);
        // Assert
        Expect(sut)
            .To.Have.Cookie(cookieName)
            .With.Value(cookieValue);
    }

    [Test]
    public void ShouldBeAbleToSetCookieJar()
    {
        // Arrange
        var cookies = new FakeResponseCookies();
        var key = GetRandomString();
        var value = GetRandom<FakeCookie>();
        cookies[key] = value;
        Expect(cookies.ContainsKey(key))
            .To.Be.True();
        // Act
        var sut = HttpResponseBuilder.Create()
            .WithCookies(cookies)
            .Build();
        // Assert
        Expect(sut.Cookies)
            .To.Be(cookies);
        Expect(cookies.ContainsKey(key))
            .To.Be.True();
        Expect(sut.Headers)
            .To.Contain.Key("Set-Cookie")
            .With.Value.Matched.By(o => $"{o}".Contains(value.Value));
    }

    [Test]
    public void ShouldBeAbleToSetCookieValues()
    {
        // Arrange
        var key1 = GetRandomString();
        var value1 = GetRandomString();
        var key2 = GetRandomString();
        var value2 = GetRandomString();
        // Act
        var sut = HttpResponseBuilder.Create()
            .WithCookie(key1, value1)
            .WithCookie(key2, value2)
            .Build();
        Expect(sut.Headers["Set-Cookie"].Count)
            .To.Equal(2);
        // Assert
        Expect(sut.Cookies)
            .To.Be.An.Instance.Of<FakeResponseCookies>();
        var cookies = (FakeResponseCookies)sut.Cookies;
        Expect(cookies.ContainsKey(key1))
            .To.Be.True();
        Expect(cookies[key1].Value)
            .To.Equal(value1);
        Expect(cookies.ContainsKey(key2))
            .To.Be.True();
        Expect(cookies[key2].Value)
            .To.Equal(value2);
    }

    [TestFixture]
    public class SettingHeaders
    {
        [Test]
        public void ShouldBeAbleToSetIndividually()
        {
            // Arrange
            var key1 = GetRandomString();
            var key2 = GetAnother(key1);
            var value1 = GetRandomString();
            var value2 = GetAnother(value1);
            // Act
            var sut = HttpResponseBuilder.Create()
                .WithHeader(key1, value1)
                .WithHeader(key2, value2)
                .Build();
            // Assert
            Expect(sut.Headers)
                .To.Contain.Key(key1)
                .With.Value(value1);
            Expect(sut.Headers)
                .To.Contain.Key(key2)
                .With.Value(value2);
        }

        [Test]
        public void ShouldBeAbleToSetFromDictionary()
        {
            // Arrange
            var key1 = GetRandomString();
            var key2 = GetAnother(key1);
            var value1 = GetRandomString();
            var value2 = GetAnother(value1);
            var dict = new Dictionary<string, string>()
            {
                [key1] = value1,
                [key2] = value2
            };
            // Act
            var sut = HttpResponseBuilder.Create()
                .WithHeaders(dict)
                .Build();
            // Assert
            Expect(sut.Headers)
                .To.Contain.Key(key1)
                .With.Value(value1);
            Expect(sut.Headers)
                .To.Contain.Key(key2)
                .With.Value(value2);
        }

        [Test]
        public void ShouldBeAbleToSetFromNameValueCollection()
        {
            // Arrange
            var key1 = GetRandomString();
            var key2 = GetAnother(key1);
            var value1 = GetRandomString();
            var value2 = GetAnother(value1);
            var collection = new NameValueCollection()
            {
                [key1] = value1,
                [key2] = value2
            };
            // Act
            var sut = HttpResponseBuilder.Create()
                .WithHeaders(collection)
                .Build();
            // Assert
            Expect(sut.Headers)
                .To.Contain.Key(key1)
                .With.Value(value1);
            Expect(sut.Headers)
                .To.Contain.Key(key2)
                .With.Value(value2);
        }
    }

    public class Data
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}