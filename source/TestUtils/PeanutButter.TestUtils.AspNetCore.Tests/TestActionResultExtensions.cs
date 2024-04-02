using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestActionResultExtensions
{
    [TestFixture]
    public class ResolveResponseAsync
    {
        [TestFixture]
        public class ActingOnRedirectResult
        {
            [Test]
            public async Task ShouldResolveTemporaryRedirect()
            {
                // Arrange
                var redirectResult = new RedirectResult(
                    "/Home/Index?foo=bar",
                    false,
                    true
                );
                // Act
                var result = await redirectResult.ResolveResponseAsync();
                // Assert
                Expect(result.StatusCode)
                    .To.Equal((int) HttpStatusCode.TemporaryRedirect);
                Expect(result.Headers.Location)
                    .To.Contain.Only(1)
                    .Equal.To("/Home/Index?foo=bar");
            }

            [Test]
            public async Task ShouldResolvePermanentRedirect()
            {
                // Arrange
                var redirectResult = new RedirectResult(
                    "/Home/Index?foo=bar",
                    true,
                    true
                );
                // Act
                var result = await redirectResult.ResolveResponseAsync();
                // Assert
                Expect(result.StatusCode)
                    .To.Equal((int) HttpStatusCode.PermanentRedirect);
                Expect(result.Headers.Location)
                    .To.Contain.Only(1)
                    .Equal.To("/Home/Index?foo=bar");
            }

            [Test]
            public async Task ShouldResolveRedirectToActionResult()
            {
                // Arrange
                var action = GetRandomString();
                var controller = GetRandomString();
                var routeValues = new
                {
                    id = 123,
                    foo = "bar"
                };
                var redirectResult = new RedirectToActionResult(
                    action,
                    controller,
                    routeValues
                );
                // Act
                var result = await redirectResult.ResolveResponseAsync();
                // Assert
                Expect(result.StatusCode)
                    .To.Equal((int) HttpStatusCode.Redirect);
                Expect(result.Headers.Location)
                    .To.Contain.Only(1)
                    .Equal.To($"/{controller}/{action}?id={routeValues.id}&foo=bar");
            }
        }

        [TestFixture]
        public class ActingOnPartialViewResult
        {
            [Test]
            public async Task ShouldResolveStatusCodeWithViewNameInBodyAndDataInHttpContextItems()
            {
                // Arrange
                var model = GetRandom<Person>();
                var key = GetRandomString();
                var value = GetRandomString();
                var statusCode = (int) GetRandom<HttpStatusCode>();
                var viewName = GetRandomString();
                var viewResult = new PartialViewResult()
                {
                    ViewName = viewName,
                    ViewData = ViewDataDictionaryBuilder.Create()
                        .WithModel(model)
                        .With(key, value)
                        .Build(),
                    StatusCode = statusCode
                };

                // Act
                var result = await viewResult.ResolveResponseAsync();
                // Assert
                Expect(result.StatusCode)
                    .To.Equal(statusCode);
                Expect(await result.Body.ReadAllTextAsync())
                    .To.Equal($"[{viewName}]");
                Expect(result.HttpContext.Items)
                    .To.Contain.Key("ViewData")
                    .With.Value(viewResult.ViewData);
                Expect(result.HttpContext.Items)
                    .To.Contain.Key("Model")
                    .With.Value(viewResult.Model);
                Expect(result.HttpContext.Response.ContentType)
                    .To.Equal("text/html");
            }
        }

        [TestFixture]
        public class ActingOnViewResult
        {
            [Test]
            public async Task ShouldResolveStatusCodeWithViewNameInBodyAndDataInHttpContextItems()
            {
                // Arrange
                var model = GetRandom<Person>();
                var key = GetRandomString();
                var value = GetRandomString();
                var statusCode = (int) GetRandom<HttpStatusCode>();
                var viewName = GetRandomString();
                var viewResult = new ViewResult()
                {
                    ViewName = viewName,
                    ViewData = ViewDataDictionaryBuilder.Create()
                        .WithModel(model)
                        .With(key, value)
                        .Build(),
                    StatusCode = statusCode
                };

                // Act
                var result = await viewResult.ResolveResponseAsync();
                // Assert
                Expect(result.StatusCode)
                    .To.Equal(statusCode);
                Expect(await result.Body.ReadAllTextAsync())
                    .To.Equal($"[{viewName}]");
                Expect(result.HttpContext.Items)
                    .To.Contain.Key("ViewData")
                    .With.Value(viewResult.ViewData);
                Expect(result.HttpContext.Items)
                    .To.Contain.Key("Model")
                    .With.Value(viewResult.Model);
                Expect(result.HttpContext.Response.ContentType)
                    .To.Equal("text/html");
            }
        }

        [TestFixture]
        public class ActingOnContentResult
        {
            [Test]
            public async Task ShouldResolveContentAndStatusCode()
            {
                // Arrange
                var contentResult = new ContentResult()
                {
                    StatusCode = (int) GetRandom<HttpStatusCode>(),
                    ContentType = "application/json",
                    Content = "<html><head></head><body>Hello, world!</body></html>"
                };
                // Act
                var result = await contentResult.ResolveResponseAsync();
                // Assert
                Expect(result.StatusCode)
                    .To.Equal(contentResult.StatusCode);
                Expect(result.ContentType)
                    .To.Equal(contentResult.ContentType);
                var content = await result.Body.ReadAllTextAsync();
                Expect(content)
                    .To.Equal(contentResult.Content);
            }
        }

        [TestFixture]
        public class ActingOnObjectResult
        {
            [Test]
            public async Task ShouldResolveContentAndStatusCode()
            {
                // Arrange
                var data = GetRandom<Person>();
                var objectResult = new ObjectResult(data)
                {
                    StatusCode = (int) GetRandom<HttpStatusCode>(),
                    ContentTypes = new MediaTypeCollection()
                        { "application/json" }
                };
                // Act
                var result = await objectResult.ResolveResponseAsync();
                // Assert
                var body = await result.Body.ReadAllTextAsync();
                Expect(body)
                    .To.Equal(JsonSerializer.Serialize(data));
            }
        }

        [TestFixture]
        public class ActingOnStatusCodeResult
        {
            [Test]
            public async Task ShouldResolveStatusCode()
            {
                // Arrange
                var notFound = new NotFoundResult();
                // Act
                var result = await notFound.ResolveResponseAsync();
                // Assert
                Expect(result.StatusCode)
                    .To.Equal((int) HttpStatusCode.NotFound);
            }
        }
    }

    [TestFixture]
    public class ResolveResponse
    {
        [TestFixture]
        public class ActingOnRedirectResult
        {
            [Test]
            public void ShouldResolveTemporaryRedirect()
            {
                // Arrange
                var redirectResult = new RedirectResult(
                    "/Home/Index?foo=bar",
                    false,
                    true
                );
                // Act
                var result = redirectResult.ResolveResponse();
                // Assert
                Expect(result.StatusCode)
                    .To.Equal((int) HttpStatusCode.TemporaryRedirect);
                Expect(result.Headers.Location)
                    .To.Contain.Only(1)
                    .Equal.To("/Home/Index?foo=bar");
            }

            [Test]
            public void ShouldResolvePermanentRedirect()
            {
                // Arrange
                var redirectResult = new RedirectResult(
                    "/Home/Index?foo=bar",
                    true,
                    true
                );
                // Act
                var result = redirectResult.ResolveResponse();
                // Assert
                Expect(result.StatusCode)
                    .To.Equal((int) HttpStatusCode.PermanentRedirect);
                Expect(result.Headers.Location)
                    .To.Contain.Only(1)
                    .Equal.To("/Home/Index?foo=bar");
            }

            [Test]
            public void ShouldResolveRedirectToActionResult()
            {
                // Arrange
                var action = GetRandomString();
                var controller = GetRandomString();
                var routeValues = new
                {
                    id = 123,
                    foo = "bar"
                };
                var redirectResult = new RedirectToActionResult(
                    action,
                    controller,
                    routeValues
                );
                // Act
                var result = redirectResult.ResolveResponse();
                // Assert
                Expect(result.StatusCode)
                    .To.Equal((int) HttpStatusCode.Redirect);
                Expect(result.Headers.Location)
                    .To.Contain.Only(1)
                    .Equal.To($"/{controller}/{action}?id={routeValues.id}&foo=bar");
            }
        }

        [TestFixture]
        public class ActingOnPartialViewResult
        {
            [Test]
            public void ShouldResolveStatusCodeWithViewNameInBodyAndDataInHttpContextItems()
            {
                // Arrange
                var model = GetRandom<Person>();
                var key = GetRandomString();
                var value = GetRandomString();
                var statusCode = (int) GetRandom<HttpStatusCode>();
                var viewName = GetRandomString();
                var viewResult = new PartialViewResult()
                {
                    ViewName = viewName,
                    ViewData = ViewDataDictionaryBuilder.Create()
                        .WithModel(model)
                        .With(key, value)
                        .Build(),
                    StatusCode = statusCode
                };

                // Act
                var result = viewResult.ResolveResponse();
                // Assert
                Expect(result.StatusCode)
                    .To.Equal(statusCode);
                Expect(result.Body.ReadAllText())
                    .To.Equal($"[{viewName}]");
                Expect(result.HttpContext.Items)
                    .To.Contain.Key("ViewData")
                    .With.Value(viewResult.ViewData);
                Expect(result.HttpContext.Items)
                    .To.Contain.Key("Model")
                    .With.Value(viewResult.Model);
                Expect(result.HttpContext.Response.ContentType)
                    .To.Equal("text/html");
            }
        }

        [TestFixture]
        public class ActingOnViewResult
        {
            [Test]
            public void ShouldResolveStatusCodeWithViewNameInBodyAndDataInHttpContextItems()
            {
                // Arrange
                var model = GetRandom<Person>();
                var key = GetRandomString();
                var value = GetRandomString();
                var statusCode = (int) GetRandom<HttpStatusCode>();
                var viewName = GetRandomString();
                var viewResult = new ViewResult()
                {
                    ViewName = viewName,
                    ViewData = ViewDataDictionaryBuilder.Create()
                        .WithModel(model)
                        .With(key, value)
                        .Build(),
                    StatusCode = statusCode
                };

                // Act
                var result = viewResult.ResolveResponse();
                // Assert
                Expect(result.StatusCode)
                    .To.Equal(statusCode);
                Expect(result.Body.ReadAllText())
                    .To.Equal($"[{viewName}]");
                Expect(result.HttpContext.Items)
                    .To.Contain.Key("ViewData")
                    .With.Value(viewResult.ViewData);
                Expect(result.HttpContext.Items)
                    .To.Contain.Key("Model")
                    .With.Value(viewResult.Model);
                Expect(result.HttpContext.Response.ContentType)
                    .To.Equal("text/html");
            }
        }

        [TestFixture]
        public class ActingOnContentResult
        {
            [Test]
            public void ShouldResolveContentAndStatusCode()
            {
                // Arrange
                var contentResult = new ContentResult()
                {
                    StatusCode = (int) GetRandom<HttpStatusCode>(),
                    ContentType = "application/json",
                    Content = "<html><head></head><body>Hello, world!</body></html>"
                };
                // Act
                var result = contentResult.ResolveResponse();
                // Assert
                Expect(result.StatusCode)
                    .To.Equal(contentResult.StatusCode);
                Expect(result.ContentType)
                    .To.Equal(contentResult.ContentType);
                var content = result.Body.ReadAllText();
                Expect(content)
                    .To.Equal(contentResult.Content);
            }
        }

        [TestFixture]
        public class ActingOnObjectResult
        {
            [Test]
            public void ShouldResolveContentAndStatusCode()
            {
                // Arrange
                var data = GetRandom<Person>();
                var objectResult = new ObjectResult(data)
                {
                    StatusCode = (int) GetRandom<HttpStatusCode>(),
                    ContentTypes = new MediaTypeCollection()
                        { "application/json" }
                };
                // Act
                var result = objectResult.ResolveResponse();
                // Assert
                var body = result.Body.ReadAllText();
                Expect(body)
                    .To.Equal(JsonSerializer.Serialize(data));
            }
        }

        [TestFixture]
        public class ActingOnStatusCodeResult
        {
            [Test]
            public void ShouldResolveStatusCode()
            {
                // Arrange
                var notFound = new NotFoundResult();
                // Act
                var result = notFound.ResolveResponse();
                // Assert
                Expect(result.StatusCode)
                    .To.Equal((int) HttpStatusCode.NotFound);
            }
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}