using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using NUnit.Framework;
using NExpect;
using NSubstitute;
using PeanutButter.TestUtils.AspNetCore.Builders;
using static NExpect.Expectations;
using static NExpect.AspNetCoreExpectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestApiControllerBuilder
{
    [TestFixture]
    public class DefaultBuild
    {
        [Test]
        public void ShouldSetUpHttpContext()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.HttpContext)
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
        }

        [Test]
        public void ShouldRegisterActionContext()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.HttpContext.RequestServices.GetService(typeof(ActionContext)))
                .Not.To.Be.Null();
        }

        [Test]
        public void ShouldSetUrl()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Url)
                .Not.To.Be.Null();
        }

        [Test]
        public void ShouldSetMetadataProvider()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.MetadataProvider)
                .Not.To.Be.Null();
        }

        [Test]
        public void ShouldSetModelStateDictionary()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.ModelState)
                .Not.To.Be.Null();
        }

        [Test]
        public void ShouldSetObjectValidator()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.ObjectValidator)
                .Not.To.Be.Null();
        }

        [Test]
        public void ShouldSetEmptyRouteData()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.RouteData)
                .Not.To.Be.Null();
            Expect(result.RouteData.Values.Keys)
                .To.Be.Empty();
            Expect(result.RouteData.DataTokens.Keys)
                .To.Be.Empty();
        }

        [Test]
        public void ShouldSetModelBinderFactory()
        {
            // Arrange
            var result = BuildDefault();
            // Act
            Expect(result.ModelBinderFactory)
                .Not.To.Be.Null();
            // Assert
        }

        [Test]
        public void ShouldControllerActionDescriptor()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.ControllerContext.ActionDescriptor)
                .Not.To.Be.Null();
            Expect(result.ControllerContext.ActionDescriptor.ControllerTypeInfo)
                .To.Equal(typeof(ApiController).GetTypeInfo());
            Expect(result.ControllerContext.ActionDescriptor.ControllerName)
                .To.Equal("Api");
        }

        private static ApiController BuildDefault(
            ILogger logger = null
        )
        {
            return ControllerBuilder.For<ApiController>()
                .WithFactory(Factory(logger))
                .Build();
        }
    }

    private static Func<ApiController> DefaultFactory => Factory();

    private static Func<ApiController> Factory(
        ILogger logger = null
    )
    {
        return () => new ApiController(logger ?? Substitute.For<ILogger>());
    }

    [Test]
    public void ShouldBeAbleToSetRequestScheme()
    {
        // Arrange
        // Act
        var expected = GetRandomString(4, 4);
        var result = ControllerBuilder.For<ApiController>()
            .WithDefaultFactory()
            .WithRequestScheme(expected)
            .Build();
        // Assert
        Expect(result.Request.Scheme)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldBeAbleToSetRequestHost()
    {
        // Arrange
        var expected = GetRandomHostname();
        // Act
        var result = ControllerBuilder.For<ApiController>()
            .WithDefaultFactory()
            .WithRequestHost(expected)
            .Build();
        // Assert
        Expect(result.Request.Host.Host)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldBeAbleToSetRequestHostString()
    {
        // Arrange
        var expected = GetRandom<HostString>();
        // Act
        var result = ControllerBuilder.For<ApiController>()
            .WithDefaultFactory()
            .WithRequestHost(expected)
            .Build();
        // Assert
        Expect(result.Request.Host)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldBeAbleToSetPort()
    {
        // Arrange
        var expected = GetRandomInt(100, 200);
        // Act
        var result = ControllerBuilder.For<ApiController>()
            .WithDefaultFactory()
            .WithRequestPort(expected)
            .Build();
        // Assert
        Expect(result.Request.Host.Port)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldBeAbleToSetHostAndPort()
    {
        // Arrange
        var host = GetRandomHostname();
        var port = GetRandomInt(100, 200);
        // Act
        var result = ControllerBuilder.For<ApiController>()
            .WithDefaultFactory()
            .WithRequestHost(host)
            .WithRequestPort(port)
            .Build();
        // Assert
        Expect(result.Request.Host.Host)
            .To.Equal(host);
        Expect(result.Request.Host.Port)
            .To.Equal(port);
    }

    [Test]
    public void ShouldBeAbleToSetPortAndHost()
    {
        // Arrange
        var host = GetRandomHostname();
        var port = GetRandomInt(100, 200);
        // Act
        var result = ControllerBuilder.For<ApiController>()
            .WithDefaultFactory()
            .WithRequestPort(port)
            .WithRequestHost(host)
            .Build();
        // Assert
        Expect(result.Request.Host.Host)
            .To.Equal(host);
        Expect(result.Request.Host.Port)
            .To.Equal(port);
    }

    [Test]
    public void ShouldBeAbleToSetQueryParameter()
    {
        // Arrange
        // Act
        var result = ControllerBuilder.For<ApiController>()
            .WithFactory(DefaultFactory)
            .WithRequestQueryParameter("a", "1")
            .WithRequestQueryParameter("b", "2")
            .Build();
        // Assert
        var query = result.Request.Query;
        Expect(query)
            .To.Contain.Only(2).Items();
        Expect(query)
            .To.Contain.Key("a")
            .With.Value("1");
        Expect(query)
            .To.Contain.Key("b")
            .With.Value("2");
        Expect(result.Request.QueryString.Value)
            .To.Contain("a=1", "fake querystring should automatically update value when parameters change")
            .And
            .To.Contain("b=2");
    }

    [Test]
    public void ShouldBeAbleToSetRequestHeader()
    {
        // Arrange
        var header1 = GetRandomString(10);
        var value1 = GetRandomString(10);
        var header2 = GetRandomString(10);
        var value2 = GetRandomString(10);
        // Act
        var result = ControllerBuilder.For<ApiController>()
            .WithDefaultFactory()
            .WithRequestHeader(header1, value1)
            .WithRequestHeader(header2, value2)
            .Build();
        // Assert
        var headers = result.Request.Headers;
        Expect(headers)
            .To.Contain.Key(header1)
            .With.Value(value1);
        Expect(headers)
            .To.Contain.Key(header2)
            .With.Value(value2);
    }

    [Test]
    public void ShouldBeAbleToSetRequestCookie()
    {
        // Arrange
        var key1 = GetRandomString(10);
        var value1 = GetRandomString(10);
        var key2 = GetRandomString(10);
        var value2 = GetRandomString(10);
        // Act
        var result = ControllerBuilder.For<ApiController>()
            .WithDefaultFactory()
            .WithRequestCookie(key1, value1)
            .WithRequestCookie(key2, value2)
            .Build();
        // Assert
        var cookies = result.Request.Cookies;
        Expect(cookies)
            .To.Contain.Key(key1)
            .With.Value(value1);
        Expect(cookies)
            .To.Contain.Key(key2)
            .With.Value(value2);
    }

    [Test]
    public void ShouldBeAbleToSetHttpContextItem()
    {
        // Arrange
        var key1 = GetRandomString(10);
        var value1 = GetRandomString(10);
        var key2 = GetRandomString(10);
        var value2 = GetRandomString(10);
        // Act
        var result = ControllerBuilder.For<ApiController>()
            .WithDefaultFactory()
            .WithHttpContextItem(key1, value1)
            .WithHttpContextItem(key2, value2)
            .Build();
        // Assert
        var items = result.HttpContext.Items;
        Expect(items)
            .To.Contain.Key(key1)
            .With.Value(value1);
        Expect(items)
            .To.Contain.Key(key2)
            .With.Value(value2);
    }

    [Test]
    public void ShouldBeAbleToSetAction()
    {
        // Arrange
        // Act
        var result = ControllerBuilder.For<ApiController>()
            .WithDefaultFactory()
            .WithAction(nameof(ApiController.Add))
            .Build();
        // Assert
        var actionDescriptor = result.ControllerContext.ActionDescriptor;
        Expect(actionDescriptor.ActionName)
            .To.Equal(nameof(ApiController.Add));
        Expect(actionDescriptor.MethodInfo)
            .To.Be(typeof(ApiController).GetMethod(nameof(ApiController.Add)));
    }
}

public static class ApiControllerBuilderExtensions
{
    public static ControllerBuilder<ApiController> WithDefaultFactory(
        this ControllerBuilder<ApiController> builder
    )
    {
        return builder.WithFactory(
            () => new ApiController(Substitute.For<ILogger>())
        );
    }
}

public interface ILogger
{
    void Log(string str);
}

public class ApiController : ControllerBase
{
    private readonly ILogger _logger;

    public ApiController(ILogger logger)
    {
        _logger = logger;
    }

    public int Add(int a, int b)
    {
        var result = a + b;
        _logger.Log($"{a} + {b} = {result}");
        return result;
    }
}