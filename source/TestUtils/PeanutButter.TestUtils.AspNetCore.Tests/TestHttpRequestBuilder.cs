using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NExpect;
using NUnit.Framework;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.TestUtils.AspNetCore.Utils;
using PeanutButter.Utils;
using static NExpect.Expectations;
using static NExpect.AspNetCoreExpectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestHttpRequestBuilder
{
    [TestFixture]
    public class DefaultBuild
    {
        [Test]
        public void ShouldSetForm()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Form)
                .Not.To.Be.Null();
            Expect(result.Form.Keys)
                .To.Be.Empty();
        }

        [Test]
        public void ShouldSetHttpContext()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.HttpContext)
                .Not.To.Be.Null();
            Expect(result.HttpContext.Request)
                .To.Be(result);
        }

        [Test]
        public void ShouldSetMethodGet()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Method)
                .To.Equal("GET");
        }

        [Test]
        public void ShouldSetScheme()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Scheme)
                .To.Equal("http");
            Expect(result.Protocol)
                .To.Equal("HTTP/1.1");
        }

        [Test]
        public void ShouldSetHost()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Host)
                .Not.To.Be.Null();
            Expect(result.Host.Host)
                .Not.To.Be.Null.Or.Empty();
            Expect(result.Host.Port)
                .To.Be.Greater.Than
                .Or.Equal.To(80);
        }

        [Test]
        public void ShouldSetPathBase()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.PathBase)
                .Not.To.Be.Null();
            Expect(result.PathBase.ToString())
                .To.Be.Empty();
        }

        [Test]
        public void ShouldSetPath()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Path)
                .Not.To.Be.Null();
            Expect(result.Path.ToString())
                .Not.To.Be.Empty();
        }

        [Test]
        public void ShouldHaveEmptyQueryString()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.QueryString.ToString())
                .To.Be.Empty();
        }

        [Test]
        public void ShouldHaveEmptyHeaders()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Headers as IDictionary<string, StringValues>)
                .To.Be.Empty();
            Expect(result.Headers.Count)
                .To.Equal(0);
        }

        [Test]
        public void ShouldHaveEmptyCookies()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Cookies as IEnumerable<KeyValuePair<string, string>>)
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

        public class Poco
        {
            public int Id { get; set; }
        }
    }

    [Test]
    public void ShouldDetermineIsHttpsFromScheme()
    {
        // Arrange
        // Act
        var result1 = HttpRequestBuilder.Create()
            .WithScheme("http")
            .Build();
        var result2 = HttpRequestBuilder.Create()
            .WithScheme("https")
            .Build();
        // Assert
        Expect(result1.IsHttps)
            .To.Be.False("should be false for http scheme");
        Expect(result2.IsHttps)
            .To.Be.True("should be true for https scheme");
    }

    [Test]
    public void ShouldSwitchSchemeWhenFlippingIsHttps()
    {
        // Arrange
        // Act
        var result = HttpRequestBuilder.BuildDefault();
        result.IsHttps = true;
        var captured1 = result.Scheme;
        result.IsHttps = false;
        var captured2 = result.Scheme;
        // Assert

        Expect(captured1)
            .To.Equal("https");
        Expect(captured2)
            .To.Equal("http");
    }

    [Test]
    public void ShouldSwitchProtocolWhenFlippingIsHttps()
    {
        // Arrange
        // Act
        var result = HttpRequestBuilder.BuildDefault();
        result.IsHttps = true;
        var captured1 = result.Protocol;
        result.IsHttps = false;
        var captured2 = result.Protocol;
        // Assert

        Expect(captured1)
            .To.Equal("HTTP/2");
        Expect(captured2)
            .To.Equal("HTTP/1.1");
    }

    [Test]
    public void ShouldUseProvidedHttpContext()
    {
        // Arrange
        var expected = new FakeHttpContext();
        // Act
        var result = HttpRequestBuilder.Create()
            .WithHttpContext(expected)
            .Build();
        // Assert
        Expect(result.HttpContext)
            .To.Be(expected);
    }

    [Test]
    public void ShouldUseProvidedMethod()
    {
        // Arrange
        var expected = GetRandomHttpMethod();
        // Act
        var result = HttpRequestBuilder.Create()
            .WithMethod(expected)
            .Build();
        // Assert
        Expect(result.Method)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldUseProvidedPath()
    {
        // Arrange
        var expected = GetRandomAbsolutePath();
        // Act
        var result = HttpRequestBuilder.Create()
            .WithPath(expected)
            .Build();
        // Assert
        Expect(result.Path)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldUseProvidedBasePath()
    {
        // Arrange
        var expected = GetRandomAbsolutePath();
        // Act
        var result = HttpRequestBuilder.Create()
            .WithBasePath(expected)
            .Build();
        // Assert
        Expect(result.PathBase)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldUseProvidedQueryString()
    {
        // Arrange
        var expected = GetRandom<QueryString>();
        // Act
        var result = HttpRequestBuilder.Create()
            .WithQueryString(expected)
            .Build();
        // Assert
        Expect(result.QueryString)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldUseProvidedQueryStringString()
    {
        // Arrange
        var expected = GetRandomUrlQuery();
        // Act
        var result = HttpRequestBuilder.Create()
            .WithQueryString(expected)
            .Build();
        // Assert
        Expect(result.QueryString.ToString())
            .To.Equal(expected);
    }

    [Test]
    public void QueryCollectionShouldReflectQueryString()
    {
        // Arrange
        var queryString = "?foo=bar&quux=1";
        // Act
        var result = HttpRequestBuilder.Create()
            .WithQueryString(queryString)
            .Build();
        // Assert
        var query = result.Query;
        Expect(query)
            .To.Contain.Key("foo")
            .With.Value("bar");
        Expect(query)
            .To.Contain.Key("quux")
            .With.Value("1");
    }

    [Test]
    public void ShouldReturnEmptyStringValuesForUnknownQueryKey()
    {
        // Arrange
        var sut = HttpRequestBuilder.BuildDefault();
        var key = GetRandomString(10);
        Expect(sut.Query.Keys)
            .Not.To.Contain(key);
        // Act
        var result = sut.Query[key];
        // Assert
        Expect(result)
            .To.Equal(new StringValues());
    }

    [Test]
    public void ShouldBeCaseInsensitive()
    {
        // Arrange
        var key = GetRandomString(10);
        var value = GetRandomString(10);
        var sut = HttpRequestBuilder.Create()
            .WithQueryParameter(key, value)
            .Build();
        var search = key.ToRandomCase();
        Expect(search)
            .Not.To.Equal(key);
        // Act
        var result = sut.Query[search];
        // Assert
        Expect(result)
            .To.Equal(value);
    }

    [Test]
    public void QueryStringShouldReflectQueryCollection()
    {
        // Arrange
        var queryString = "?foo=bar1&bar=123";
        var collection = new FakeQueryCollection(
            queryString
        );
        // Act
        var result = HttpRequestBuilder.Create()
            .WithQuery(collection)
            .Build();
        // Assert
        Expect(result.QueryString.ToString())
            .To.Equal(queryString);
    }

    [Test]
    public void ShouldBeAbleToSetQueryParameter()
    {
        // Arrange
        var key = GetRandomString();
        var value = GetRandomString();
        // Act
        var result = HttpRequestBuilder.Create()
            .WithQueryParameter(key, value)
            .Build();
        // Assert
        Expect(result.Query[key])
            .To.Equal(value);
        Expect(result.QueryString.Value)
            .To.Contain(key)
            .Then(value);
    }

    [Test]
    public void ShouldBeAbleToSetHeaders()
    {
        // Arrange
        // Act
        var result = HttpRequestBuilder.Create()
            .WithHeader("X-HeaderA", "foo")
            .WithHeader("X-HeaderB", "bar")
            .Build();
        // Assert
        var headers = result.Headers as IDictionary<string, StringValues>;
        Expect(headers)
            .To.Contain.Exactly(1)
            .Matched.By(o =>
                o.Key == "X-HeaderA" &&
                o.Value == "foo"
            );
        Expect(headers)
            .To.Contain.Exactly(1)
            .Matched.By(o =>
                o.Key == "X-HeaderB" &&
                o.Value == "bar"
            );
    }

    [Test]
    public void ShouldBeAbleToClearHeaders()
    {
        // Arrange
        // Act
        var result = HttpRequestBuilder.Create()
            .WithHeader("X-HeaderA", "foo")
            .WithHeader("X-HeaderB", "bar")
            .WithNoHeaders()
            .Build();
        // Assert
        Expect(result.Headers)
            .To.Be.Empty();
    }

    [Test]
    public void ShouldBeAbleToSpecifyADictionaryOfHeaders()
    {
        // Arrange
        // Act
        var result = HttpRequestBuilder.Create()
            .WithHeaders(
                new Dictionary<string, string>()
                {
                    ["X-HeaderA"] = "foo",
                    ["X-HeaderB"] = "bar"
                }
            )
            .Build();
        // Assert
        var headers = result.Headers;
        Expect(headers)
            .To.Contain.Only(2).Items();
        Expect(headers)
            .To.Contain.Exactly(1)
            .Matched.By(o =>
                o.Key == "X-HeaderA" &&
                o.Value == "foo"
            );
        Expect(headers)
            .To.Contain.Exactly(1)
            .Matched.By(o =>
                o.Key == "X-HeaderB" &&
                o.Value == "bar"
            );
    }

    [Test]
    public void ShouldBeAbleToSetIndividualCookies()
    {
        // Arrange
        // Act
        var result = HttpRequestBuilder.Create()
            .WithCookie("foo", "bar")
            .WithCookie("one", "1")
            .Build();
        // Assert
        Expect(result.Cookies as IEnumerable<KeyValuePair<string, string>>)
            .To.Contain.Exactly(1)
            .Matched.By(o =>
                o.Key == "foo" &&
                o.Value == "bar"
            )
            .And
            .To.Contain.Exactly(1)
            .Matched.By(o =>
                o.Key == "one" &&
                o.Value == "1"
            );
    }

    [Test]
    public void ShouldBeAbleToSetCookiesFromDictionary()
    {
        // Arrange
        // Act
        var result = HttpRequestBuilder.Create()
            .WithCookies(
                new Dictionary<string, string>()
                {
                    ["foo"] = "bar"
                }
            ).Build();
        // Assert
        Expect(result.Cookies)
            .To.Contain.Key("foo")
            .With.Value("bar");
    }

    [Test]
    public void ShouldBeAbleToClearCookies()
    {
        // Arrange
        // Act
        var result = HttpRequestBuilder.Create()
            .WithCookies(
                new Dictionary<string, string>()
                {
                    ["foo"] = "bar"
                }
            )
            .WithNoCookies()
            .Build();
        // Assert
        Expect(result.Cookies)
            .To.Be.Empty();
    }

    [TestFixture]
    public class ContentLength
    {
        [Test]
        public void ShouldReflectTheLengthOfTheBodyStream()
        {
            // Arrange
            var stream = new MemoryStream(
                GetRandomBytes()
            );
            var sut = HttpRequestBuilder.Create()
                .WithBody(stream)
                .Build();
            // Act
            var result = sut.ContentLength;
            // Assert
            Expect(result)
                .To.Equal(stream.Length);
        }

        [Test]
        public void ShouldSetBodyLengthWhenSet()
        {
            // Arrange
            var stream = new MemoryStream(
                GetRandomBytes()
            );
            var originalLength = stream.Length;

            var sut = Create()
                .WithBody(stream)
                .Build();
            // Act
            sut.ContentLength = stream.Length - 1;
            // Assert
            Expect(sut.Body.Length)
                .To.Equal(stream.Length)
                .And
                .To.Equal(originalLength - 1);
        }
    }

    [TestFixture]
    public class Body
    {
        [Test]
        public void SettingFormShouldSetBody()
        {
            // Arrange
            var form = GetRandom<IFormCollection>();
            Expect(form)
                .Not.To.Be.Empty();
            Expect(form.Files)
                .To.Be.Empty();
            // Act
            var result = HttpRequestBuilder.Create()
                .WithForm(form)
                .Build();
            // Assert
            Expect(result.Body)
                .Not.To.Be.Null();
            var decoder = new FormDecoder();
            var resultForm = decoder.Decode(result.Body);
            Expect(resultForm)
                .To.Deep.Equal(form);
        }

        [Test]
        public void SettingFormShouldSetBodyWithFiles()
        {
            // Arrange
            var form = FormBuilder.Create()
                .Randomize()
                .WithFile(
                    FormFileBuilder.BuildRandom()
                ).Build();
            Expect(form)
                .Not.To.Be.Empty();
            Expect(form.Files)
                .Not.To.Be.Empty();
            // Act
            var result = HttpRequestBuilder.Create()
                .WithForm(form)
                .Build();
            // Assert
            Expect(result.Body)
                .Not.To.Be.Null();
            var decoder = new FormDecoder();
            var resultForm = decoder.Decode(result.Body);
            Expect(resultForm)
                .To.Deep.Equal(form);
        }

        [Test]
        public void SettingBodyShouldSetForm()
        {
            // Arrange
            var expected = FormBuilder.Create()
                .Randomize()
                .Build();
            Expect(expected)
                .Not.To.Be.Empty();
            Expect(expected.Files)
                .To.Be.Empty();
            var encoded = new UrlEncodedBodyEncoder()
                .Encode(expected);
            // Act
            var result = HttpRequestBuilder.Create()
                .WithBody(encoded)
                .Build();

            // Assert
            Expect(result.Form)
                .To.Deep.Equal(expected);
        }

        [Test]
        public void SettingBodyShouldSetFormWithFiles()
        {
            // Arrange
            var expected = FormBuilder.Create()
                .Randomize()
                .WithRandomFile()
                .Build();
            Expect(expected)
                .Not.To.Be.Empty();
            Expect(expected.Files)
                .Not.To.Be.Empty();
            var encoded = new UrlEncodedBodyEncoder()
                .Encode(expected);
            // Act
            var result = HttpRequestBuilder.Create()
                .WithBody(encoded)
                .Build();

            // Assert
            Expect(result.Form)
                .To.Deep.Equal(expected);
        }

        [Test]
        public void ShouldBeAbleToGenerateRandom()
        {
            // Arrange
            // Act
            var result1 = GetRandom<HttpRequest>();
            var result2 = GetRandom<HttpRequest>();
            // Assert
            Expect(result1)
                .Not.To.Deep.Equal(result2);
            Expect(result1.Method)
                .Not.To.Be.Null.Or.Empty();
            Expect(result1.Scheme)
                .Not.To.Be.Null.Or.Empty();
            Expect(result1.Path.Value)
                .Not.To.Be.Null.Or.Empty();
            Expect(result1.Protocol)
                .Not.To.Be.Null.Or.Empty();
            Expect(result1.Host)
                .Not.To.Be.Null();
            Expect(result1.Host.Host)
                .Not.To.Be.Empty()
                .And
                .Not.To.Equal("localhost");
            Expect(result1.Host.Port)
                .Not.To.Be.Null()
                .And
                .To.Be.Greater.Than(0);
            Expect(result1.Form)
                .Not.To.Be.Null();
            Expect(result1.Cookies)
                .Not.To.Be.Null();
            Expect(result1.Headers)
                .Not.To.Be.Null();
        }

        [Test]
        public void ShouldBeAbleToSetFormField()
        {
            // Arrange
            var key = GetRandomString(10);
            var value = GetRandomString(10);
            var randomized = key.ToRandomCase();
            var sut = HttpRequestBuilder.Create()
                .WithFormField(key, value)
                .Build();
            // Act
            var result = sut.Form[randomized];
            // Assert
            Expect(result)
                .To.Equal(value, "form field access should be case-insensitive");
        }

        [Test]
        public void ShouldBeAbleToAddFormFile()
        {
            // Arrange
            var file = GetRandom<FakeFormFile>();
            var sut = HttpRequestBuilder.Create()
                .WithFormFile(file)
                .Build();
            // Act
            var result = sut.Form.Files[file.Name];
            // Assert
            Expect(result)
                .To.Be(file);
        }

        [Test]
        public void ShouldBeAbleToAddAFormFileByNameFileNameAndContent()
        {
            // Arrange
            var name = GetRandomString(5);
            var fileName = GetRandomFileName();
            var content = GetRandomWords();
            var expected = new FakeFormFile(
                content.ToMemoryStream(),
                name,
                fileName
            );
            Expect(expected.Content)
                .To.Equal(Encoding.UTF8.GetBytes(content));
            
            var sut = HttpRequestBuilder.Create()
                .WithFormFile(content, name, fileName)
                .Build();
            // Act
            var result = sut.Form.Files[name];
            // Assert
            Expect(result)
                .To.Deep.Equal(expected);
        }

        [TestFixture]
        public class WithUrl
        {
            [Test]
            public void ShouldSetHost()
            {
                // Arrange
                var url = GetRandomHttpUrlWithPathAndParameters();
                var uri = new Uri(url);
                // Act
                var result = HttpRequestBuilder.Create()
                    .WithUrl(url)
                    .Build();
                // Assert
                Expect(result.Host.Host)
                    .To.Equal(uri.Host);
            }

            [Test]
            public void ShouldSetPort()
            {
                // Arrange
                var expected = GetRandomInt(80, 1024);
                var url = $"http://{GetRandomHostname()}:{expected}/{GetRandomHttpPathAndParameters()}";
                // Act
                var result = HttpRequestBuilder.Create()
                    .WithUrl(url)
                    .Build();
                // Assert
                Expect(result.Host.Port)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldSetScheme()
            {
                // Arrange
                var expected = GetRandomBoolean() ? "http": "https";
                var url = $"{expected}://{GetRandomHostname()}/{GetRandomHttpPathAndParameters()}";
                // Act
                var result = HttpRequestBuilder.Create()
                    .WithUrl(url)
                    .Build();
                // Assert
                Expect(result.Scheme)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldSetPath()
            {
                // Arrange
                var url = GetRandomHttpUrlWithPathAndParameters();
                var uri = new Uri(url);
                var expected = uri.AbsolutePath;
                // Act
                var result = HttpRequestBuilder.Create()
                    .WithUrl(url)
                    .Build();
                // Assert
                Expect(result.Path)
                    .To.Equal(expected);
                Expect(result.PathBase)
                    .To.Equal("");
            }

            [Test]
            public void ShouldSetQuery()
            {
                // Arrange
                var parameters = GetRandom<Dictionary<string, string>>();
                var url = $"{GetRandomHttpUrlWithPath()}?{parameters.AsQueryStringParameters()}";

                // Act
                var result = HttpRequestBuilder.Create()
                    .WithUrl(url)
                    .Build();
                // Assert
                Expect(result.QueryString)
                    .To.Equal(parameters);
            }

            [Test]
            public void ShouldSetQueryCollection()
            {
                // Arrange
                var parameters = GetRandom<Dictionary<string, StringValues>>();
                var url = $"{GetRandomHttpUrlWithPath()}?{parameters.AsQueryStringParameters()}";

                // Act
                var result = HttpRequestBuilder.Create()
                    .WithUrl(url)
                    .Build();
                // Assert
                Expect(result.Query)
                    .To.Be.Equivalent.To(parameters);
            }

        }
    }

    private static HttpRequestBuilder Create()
    {
        return HttpRequestBuilder.Create();
    }

    private static HttpRequest BuildDefault()
    {
        return HttpRequestBuilder.BuildDefault();
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        // force static constructor to be called
        HttpRequestBuilder.Create();
    }
}