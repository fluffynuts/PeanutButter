using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.Utils;
using JsonResult = Microsoft.AspNetCore.Mvc.JsonResult;
using TempDataDictionary = Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary;
using ViewResult = Microsoft.AspNetCore.Mvc.ViewResult;

namespace PeanutButter.TestUtils.AspNetCore.Tests.Fakes;

[TestFixture]
public class TestJsonResultExecutor
{
    [Test]
    public async Task ShouldRenderJsonToTheContext()
    {
        // Arrange
        var actionContext = ActionContextBuilder.BuildDefault();
        var data = new
        {
            id = 1,
            name = GetRandomString()
        };
        var expected = JsonSerializer.Serialize(data);
        var actionResult = new JsonResult(data);
        var sut = Create();
        // Act
        await sut.ExecuteAsync(
            actionContext,
            actionResult
        );
        // Assert
        var result = await actionContext.HttpContext.Response.Body.ReadAllTextAsync();
        Expect(result)
            .To.Equal(expected);
    }


    private static IActionResultExecutor<JsonResult> Create()
    {
        return new FakeJsonResultExecutor();
    }
}

[TestFixture]
public class TestFakeObjectResultExecutor
{
    [Test]
    public async Task ShouldRenderJsonToTheContext()
    {
        // Arrange
        var actionContext = ActionContextBuilder.BuildDefault();
        var data = new
        {
            id = 1,
            name = GetRandomString()
        };
        var expected = JsonSerializer.Serialize(data);
        var actionResult = new ObjectResult(data);
        var sut = Create();
        // Act
        await sut.ExecuteAsync(
            actionContext,
            actionResult
        );
        // Assert
        var result = await actionContext.HttpContext.Response.Body.ReadAllTextAsync();
        Expect(result)
            .To.Equal(expected);
    }


    private static IActionResultExecutor<ObjectResult> Create()
    {
        return new FakeObjectResultExecutor();
    }
}

[TestFixture]
public class TestFakeViewResultExecutor
{
    [TestFixture]
    public class ActingOnViewResult
    {
        [Test]
        public async Task ShouldRenderSomethingToTheContext()
        {
            // Arrange
            var actionContext = ActionContextBuilder.BuildDefault();
            var data = new
            {
                id = 1,
                value = false
            };
            var viewName = GetRandomString();
            var viewDataDictionary = ViewDataDictionaryBuilder.Create()
                .WithModel(data)
                .Build();
            var httpStatusCode = GetRandom<HttpStatusCode>();
            var contentType = GetRandomFrom(["text/html", "application/json", "text/plain", "text/xml"]);
            var actionResult = new ViewResult()
            {
                ContentType = contentType,
                StatusCode = (int)httpStatusCode,
                TempData = new TempDataDictionary(actionContext.HttpContext, new FakeTempDataProvider()),
                ViewData = viewDataDictionary,
                ViewEngine = Substitute.For<IViewEngine>(),
                ViewName = viewName
            };
            var sut = Create();
            // Act
            await sut.ExecuteAsync(actionContext, actionResult);
            // Assert
            var result = await actionContext.HttpContext.Response.Body.ReadAllTextAsync();
            var opts = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            Expect(result)
                .To.Contain($"Status: {actionResult.StatusCode}")
                .Then($"Content Type: {contentType}")
                .Then($"View: {viewName}")
                .Then("Model:")
                .Then(JsonSerializer.Serialize(data, opts))
                .Then("ViewData:")
                .Then(JsonSerializer.Serialize(viewDataDictionary, opts));
            var httpContext = actionContext.HttpContext;
            Expect(httpContext.Items["Model"])
                .To.Be(data);
            Expect(httpContext.Items["ViewData"])
                .To.Be(viewDataDictionary);
            Expect(httpContext.Response.StatusCode)
                .To.Equal(httpStatusCode);
            Expect(httpContext.Response.ContentType)
                .To.Equal(contentType);
        }

        private static IActionResultExecutor<ViewResult> Create()
        {
            return new FakeViewResultExecutor();
        }
    }

    [TestFixture]
    public class ActingOnPartialViewResult
    {
        [Test]
        public async Task ShouldRenderSomethingToTheContext()
        {
            // Arrange
            var actionContext = ActionContextBuilder.BuildDefault();
            var data = new
            {
                id = 1,
                value = false
            };
            var viewName = GetRandomString();
            var viewDataDictionary = ViewDataDictionaryBuilder.Create()
                .WithModel(data)
                .Build();
            var httpStatusCode = GetRandom<HttpStatusCode>();
            var contentType = GetRandomFrom(["text/html", "application/json", "text/plain", "text/xml"]);
            var actionResult = new PartialViewResult()
            {
                ContentType = contentType,
                StatusCode = (int)httpStatusCode,
                TempData = new TempDataDictionary(actionContext.HttpContext, new FakeTempDataProvider()),
                ViewData = viewDataDictionary,
                ViewEngine = Substitute.For<IViewEngine>(),
                ViewName = viewName
            };
            var sut = Create();
            // Act
            await sut.ExecuteAsync(actionContext, actionResult);
            // Assert
            var result = await actionContext.HttpContext.Response.Body.ReadAllTextAsync();
            var opts = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            Expect(result)
                .To.Contain($"Status: {actionResult.StatusCode}")
                .Then($"Content Type: {contentType}")
                .Then($"View: {viewName}")
                .Then("Model:")
                .Then(JsonSerializer.Serialize(data, opts))
                .Then("ViewData:")
                .Then(JsonSerializer.Serialize(viewDataDictionary, opts));
            var httpContext = actionContext.HttpContext;
            Expect(httpContext.Items["Model"])
                .To.Be(data);
            Expect(httpContext.Items["ViewData"])
                .To.Be(viewDataDictionary);
            Expect(httpContext.Response.StatusCode)
                .To.Equal(httpStatusCode);
            Expect(httpContext.Response.ContentType)
                .To.Equal(contentType);
        }

        private static IActionResultExecutor<PartialViewResult> Create()
        {
            return new FakeViewResultExecutor();
        }
    }
}