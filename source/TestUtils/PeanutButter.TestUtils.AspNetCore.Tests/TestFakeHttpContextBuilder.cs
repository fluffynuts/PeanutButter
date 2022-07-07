using Microsoft.AspNetCore.Http;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using NUnit.Framework;
using PeanutButter.TestUtils.AspNetCore.Builders;
using static NExpect.Expectations;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestFakeHttpContextBuilder
{
    [TestFixture]
    public class DefaultBuild
    {
        [Test]
        public void ShouldBuildAnHttpContext()
        {
            // Arrange
            // Act
            var result = HttpContextBuilder.BuildDefault();
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
            var result = HttpContextBuilder.BuildDefault();
            // Assert
            Expect(result.Features)
                .Not.To.Be.Null();
        }

        [Test]
        public void ShouldSetRequest()
        {
            // Arrange
            var result = HttpContextBuilder.BuildDefault();
            // Act
            Expect(result.Request)
                .Not.To.Be.Null();
            // Assert
        }

        [Test]
        public void ShouldSetResponse()
        {
            // Arrange
            // Act
            var result = HttpContextBuilder.BuildDefault();
            // Assert
            Expect(result.Response)
                .Not.To.Be.Null();
        }

        [Test]
        public void ShouldSetConnection()
        {
            // Arrange
            // Act
            var result = HttpContextBuilder.BuildDefault();
            // Assert
            Expect(result.Connection)
                .Not.To.Be.Null();
        }

        [Test]
        public void AuthenticationShouldThrow()
        {
            // Arrange
            // Act
            var result = HttpContextBuilder.BuildDefault();
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
            var result = HttpContextBuilder.BuildDefault();
            // Assert
            Expect(result.User)
                .Not.To.Be.Null();
        }
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
}