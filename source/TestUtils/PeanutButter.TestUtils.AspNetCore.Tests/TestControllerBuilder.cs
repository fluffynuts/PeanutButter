using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NUnit.Framework;
using NExpect;
using NSubstitute;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.Utils;
using static NExpect.Expectations;
using static NExpect.AspNetCoreExpectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestControllerBuilder
{
    [TestFixture]
    public class WhenControllerIsControllerBase
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

            [TestCase("http://localhost")]
            public void ShouldSetUrl_(string expected)
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.Request.FullUrl())
                    .To.Equal(new Uri(expected));
            }

            [TestCase("GET")]
            public void ShouldSetMethod_(string expected)
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.Request.Method)
                    .To.Equal(expected);
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
                    .To.Equal(new[] { "controller" });
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

            [Test]
            public void ShortHandBuilder()
            {
                // Arrange
                // Act
                var result = ControllerBuilder.BuildDefault<NoDependenciesController>();
                // Assert
                Expect(result)
                    .To.Be.An.Instance.Of<NoDependenciesController>();
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
                .WithDefaultFactoryForApiController()
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
                .WithDefaultFactoryForApiController()
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
                .WithDefaultFactoryForApiController()
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
                .WithDefaultFactoryForApiController()
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
                .WithDefaultFactoryForApiController()
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
                .WithDefaultFactoryForApiController()
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
                .WithDefaultFactoryForApiController()
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
                .WithDefaultFactoryForApiController()
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
        public void ShouldBeAbleToSetMultipleRequestCookies()
        {
            // Arrange
            var key1 = GetRandomString(10);
            var value1 = GetRandomString(10);
            var key2 = GetRandomString(10);
            var value2 = GetRandomString(10);
            var setCookies = new Dictionary<string, string>()
            {
                [key1] = value1,
                [key2] = value2
            };
            // Act
            var result = ControllerBuilder.For<ApiController>()
                .WithDefaultFactoryForApiController()
                .WithRequestCookies(setCookies)
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
                .WithDefaultFactoryForApiController()
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
                .WithDefaultFactoryForApiController()
                .WithAction(nameof(ApiController.Add))
                .Build();
            // Assert
            var actionDescriptor = result.ControllerContext.ActionDescriptor;
            Expect(actionDescriptor.ActionName)
                .To.Equal(nameof(ApiController.Add));
            Expect(actionDescriptor.MethodInfo)
                .To.Be(typeof(ApiController).GetMethod(nameof(ApiController.Add)));
        }

        [Test]
        public void ShouldBeAbleToSetRequestUrl()
        {
            // Arrange
            var url = GetRandomHttpUrlWithPathAndParameters();
            // Act
            var result = ControllerBuilder.For<ApiController>()
                .WithDefaultFactoryForApiController()
                .WithAction(nameof(ApiController.Add))
                .WithRequestUrl(url)
                .Build();
            // Assert
            Expect(result.Request.FullUrl())
                .To.Equal(new Uri(url));
        }

        [Test]
        public void ShouldBeAbleToMutateTheRequestAsAFakeHttpRequest()
        {
            // Arrange
            var url = GetRandomHttpUrlWithPathAndParameters();
            // Act
            var result = ControllerBuilder.For<ApiController>()
                .WithDefaultFactoryForApiController()
                .WithAction(nameof(ApiController.Add))
                .WithRequestMutator(req => req.SetUrl(url))
                .Build();
            // Assert
            Expect(result.Request.FullUrl())
                .To.Equal(new Uri(url));
        }

        [Test]
        public void ShouldBeAbleToSetABunchOfRequestHeaders()
        {
            // Arrange
            var headers = GetRandom<Dictionary<string, string>>();
            // Act
            var result = ControllerBuilder.For<ApiController>()
                .WithDefaultFactoryForApiController()
                .WithRequestHeaders(headers)
                .Build();
            // Assert
            foreach (var kvp in headers)
            {
                Expect(result.Request.Headers)
                    .To.Contain.Key(kvp.Key)
                    .With.Value(kvp.Value);
            }
        }

        [Test]
        public void ShouldBeAbleToSetMethod()
        {
            // Arrange
            var expected = OneOf(
                HttpMethods.Connect,
                HttpMethods.Delete,
                HttpMethods.Head,
                HttpMethods.Options,
                HttpMethods.Patch,
                HttpMethods.Post,
                HttpMethods.Put,
                HttpMethods.Trace
            );
            // Act
            var result = ControllerBuilder.For<ApiController>()
                .WithDefaultFactoryForApiController()
                .WithRequestMethod(expected)
                .Build();
            // Assert
            Expect(result.Request.Method)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    public class WhenControllerIsController
    {
        [TestFixture]
        public class DefaultBuild : TestControllerBuilder.WhenControllerIsControllerBase.DefaultBuild
        {
            [Test]
            public void ShouldSetTempData()
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.TempData)
                    .Not.To.Be.Null();
            }

            [Test]
            public void ShouldSetViewBag()
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.ViewBag as object)
                    .Not.To.Be.Null();
            }

            [Test]
            public void ShouldSetViewData()
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.ViewData)
                    .Not.To.Be.Null();
            }

            private static MvcController BuildDefault()
            {
                return ControllerBuilder.For<MvcController>()
                    .Build();
            }
        }

        [TestFixture]
        public class Issues
        {
            [Test]
            public void ShouldResolveMvcLikePathForAction()
            {
                // Arrange
                var controller = ControllerBuilder
                    .For<MvcController>()
                    .WithAction(nameof(MvcController.Add))
                    .Build();
                // Act
                // ReSharper disable once Mvc.ActionNotResolved
                var result = controller.Url.Action("foo");
                // Assert
                Expect(result)
                    .To.Equal("/Mvc/foo");
            }

            [Test]
            public void ResolvingLinks()
            {
                // Arrange
                var controller = ControllerBuilder
                    .For<MvcController>()
                    .WithAction(nameof(MvcController.Add))
                    .Build();
                // Act
                var result = controller.Url.Link("foo", new { id = 1 });
                // Assert
                Expect(result)
                    .To.Equal("http://localhost:80/Mvc/Add?id=1");
            }

            [Test]
            public void ResolvingContent()
            {
                // Arrange
                var controller = ControllerBuilder
                    .For<MvcController>()
                    .WithAction(nameof(MvcController.Add))
                    .Build();
                // Act
                var result = controller.Url.Content("~/resource/123?a=bc");
                // Assert
                Expect(result)
                    .To.Equal("/resource/123?a=bc");
            }

            [Test]
            public void ShouldRegisterOwnImplementationOfTempDataDictionaryFactory()
            {
                // Arrange
                var controller = ControllerBuilder.For<MvcController>()
                    .Build();
                // Act
                var result = controller.HttpContext.RequestServices.GetService(
                    typeof(ITempDataDictionaryFactory)
                );
                // Assert
                Expect(result)
                    .To.Be.An.Instance.Of<FakeTempDataDictionaryFactory>();
            }

            [Test]
            public void ShouldRegisterOwnImplementationOfTempDataProvider()
            {
                // Arrange
                var controller = ControllerBuilder.For<MvcController>()
                    .Build();
                // Act
                var result = controller.HttpContext.RequestServices.GetService(
                    typeof(ITempDataProvider)
                );
                // Assert
                Expect(result)
                    .To.Be.An.Instance.Of<FakeTempDataProvider>();
            }
        }

        public class MvcController : Controller
        {
            public int Add(int a, int b)
            {
                return a + b;
            }
        }
    }
}

// TODO: if there's ever more asp.net utils,
// move this into that project to publish
public static class HttpRequestExtensions
{
    public static Uri FullUrl(
        this HttpRequest request
    )
    {
        return new Uri($@"{
            request.Scheme
        }://{
            request.Host
        }{
            request.Path
        }{request.QueryString}"
        );
    }
}

public static class ApiControllerBuilderExtensions
{
    public static ControllerBuilder<ApiController> WithDefaultFactoryForApiController(
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

public class NoDependenciesController : ControllerBase
{
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