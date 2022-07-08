using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using NSubstitute;
using NUnit.Framework;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.TestUtils.AspNetCore.Fakes;
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
            var result = BuildDefault();
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
            var result = BuildDefault();
            // Assert
            Expect(result.Features)
                .Not.To.Be.Null();
        }

        [Test]
        public void ShouldSetRequest()
        {
            // Arrange
            var result = BuildDefault();
            // Act
            Expect(result.Request)
                .Not.To.Be.Null();
            // Assert
        }

        [Test]
        [Ignore("WIP")]
        public void ShouldSetResponse()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Response)
                .Not.To.Be.Null();
            Expect(result.Response.HttpContext)
                .To.Be(result);
        }

        [Test]
        public void ShouldSetConnection()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Connection)
                .Not.To.Be.Null();
        }

        [Test]
        public void AuthenticationShouldThrow()
        {
            // Arrange
            // Act
            var result = BuildDefault();
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
            var result = BuildDefault();
            // Assert
            Expect(result.User)
                .Not.To.Be.Null();
        }

        [Test]
        public void ShouldSetItems()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Items)
                .Not.To.Be.Null();
        }

        [Test]
        public void ShouldSetNonFunctionalServiceProvider()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.RequestServices)
                .Not.To.Be.Null();
            Expect(() => result.RequestServices.GetService(typeof(SomeService)))
                .To.Throw<ServiceProviderImplementationRequiredException>();
        }

        [Test]
        public void ShouldSetRequestAbortedCancellationToken()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.RequestAborted)
                .Not.To.Be.Null();
            Expect(result.RequestAborted.IsCancellationRequested)
                .To.Be.False();
        }

        [Test]
        public void ShouldSetTraceIdentifierToGuid()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.TraceIdentifier)
                .Not.To.Be.Null.Or.Empty();
            Expect(result.TraceIdentifier)
                .To.Be.A.Guid();
        }

        [Test]
        public void ShouldSetSession()
        {
            // Arrange
            // Act
            var result = BuildDefault();
            // Assert
            Expect(result.Session)
                .Not.To.Be.Null();
        }

        public class SomeService
        {
            public void DoNothing()
            {
            }
        }

        private static HttpContext BuildDefault()
        {
            return HttpContextBuilder.BuildDefault();
        }
    }

    [Test]
    public void ShouldBeAbleToSpecifyFeatures()
    {
        // Arrange
        var expected = Substitute.For<IFeatureCollection>();
        // Act
        var result = HttpContextBuilder.Create()
            .WithFeatures(expected)
            .Build();
        // Assert
        Expect(result.Features)
            .To.Be(expected);
    }

    [Test]
    public void ShouldBeAbleToSpecifyAFeature()
    {
        // Arrange
        var feature = new SomeFeature();
        // Act
        var result = HttpContextBuilder.Create()
            .WithFeature(feature)
            .Build();
        // Assert
        Expect(result.Features.Get<SomeFeature>())
            .To.Be(feature);
    }

    private class SomeFeature
    {
    }

    [Test]
    public void ShouldBeAbleToSetRequest()
    {
        // Arrange
        var expected = HttpRequestBuilder.BuildDefault();
        // Act
        var result = HttpContextBuilder.Create()
            .WithRequest(expected)
            .Build();
        // Assert
        Expect(result.Request)
            .To.Be(expected);
        Expect(result.Request.HttpContext)
            .To.Be(result);
    }

    [Test]
    [Explicit("WIP")]
    public void ShouldBeAbleToSetResponse()
    {
        // Arrange
        var expected = HttpResponseBuilder.BuildDefault();
        // Act
        
        // Assert
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