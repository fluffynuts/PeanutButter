using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NExpect;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestFakeHttpRequestBuilder
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
                .To.Equal("http");
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
    }

    [Test]
    public void ShouldDetermineIsHttpsFromScheme()
    {
        // Arrange
        // Act
        var result1 = FakeHttpRequestBuilder.Create()
            .WithScheme("http")
            .Build();
        var result2 = FakeHttpRequestBuilder.Create()
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
        var result = FakeHttpRequestBuilder.BuildDefault();
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
    public void ShouldUseProvidedHttpContext()
    {
        // Arrange
        var expected = new FakeHttpContext();
        // Act
        var result = FakeHttpRequestBuilder.Create()
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
        var result = FakeHttpRequestBuilder.Create()
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
        var result = FakeHttpRequestBuilder.Create()
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
        var result = FakeHttpRequestBuilder.Create()
            .WithBasePath(expected)
            .Build();
        // Assert
        var pb = result.PathBase;
        Expect(result.PathBase)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldUseProvidedQueryString()
    {
        // Arrange
        var expected = GetRandom<QueryString>();
        // Act
        var result = FakeHttpRequestBuilder.Create()
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
        var result = FakeHttpRequestBuilder.Create()
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
        var result = FakeHttpRequestBuilder.Create()
            .WithQueryString(queryString)
            .Build();
        // Assert
        var query = result.Query.ToDictionary(o => o.Key, o => o.Value);
        Expect(query)
            .To.Contain.Key("foo")
            .With.Value("bar");
        Expect(query)
            .To.Contain.Key("quux")
            .With.Value("1");
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
        var result = FakeHttpRequestBuilder.Create()
            .WithQuery(collection)
            .Build();
        // Assert
        Expect(result.QueryString.ToString())
            .To.Equal(queryString);
    }

    [Test]
    public void ShouldBeAbleToSetHeaders()
    {
        // Arrange
        // Act
        var result = FakeHttpRequestBuilder.Create()
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
    public void ShouldBeAbleToSetIndividualCookies()
    {
        // Arrange
        // Act
        var result = FakeHttpRequestBuilder.Create()
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
            var sut = FakeHttpRequestBuilder.Create()
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
            // Act
            // Assert
        }
    }

    private static FakeHttpRequestBuilder Create()
    {
        return FakeHttpRequestBuilder.Create();
    }

    private static HttpRequest BuildDefault()
    {
        return FakeHttpRequestBuilder.BuildDefault();
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        // force static constructor to be called
        FakeHttpRequestBuilder.Create();
    }
}