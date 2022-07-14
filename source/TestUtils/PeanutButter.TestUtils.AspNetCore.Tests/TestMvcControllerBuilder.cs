using Microsoft.AspNetCore.Mvc;
using NExpect;
using NUnit.Framework;
using PeanutButter.TestUtils.AspNetCore.Builders;
using static NExpect.Expectations;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

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
                .Build();
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