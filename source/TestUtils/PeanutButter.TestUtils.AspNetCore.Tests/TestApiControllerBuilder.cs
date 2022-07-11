using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using NExpect;
using PeanutButter.TestUtils.AspNetCore;
using PeanutButter.TestUtils.AspNetCore.Tests;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Tests
{
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
            

            private static ApiController BuildDefault()
            {
                return ControllerBuilder.For<ApiController>()
                    .BuildDefault();
            }
        }
    }

    public class ApiController : ControllerBase
    {
        public int Add(int a, int b)
        {
            return a + b;
        }
    }
}

[TestFixture]
public class TestMvcControllerBuilder
{
    [TestFixture]
    public class DefaultBuild: TestApiControllerBuilder.DefaultBuild
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
                .BuildDefault();
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