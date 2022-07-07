using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NExpect;
using NUnit.Framework;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.Utils;
using static NExpect.Expectations;
using static NExpect.AspNetCoreExpectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Tests
{
    [TestFixture]
    public class TestHttpResponseBuilder
    {
        [TestFixture]
        public class DefaultBuild
        {
            [Test]
            public void ShouldHaveHttpContext()
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.HttpContext)
                    .Not.To.Be.Null();
                Expect(result.HttpContext.Response)
                    .To.Be(result);
            }

            [TestCase(200)]
            public void ShouldHaveStatusCode_(int expected)
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.StatusCode)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldHaveEmptyHeaders()
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.Headers)
                    .To.Be.Empty();
            }

            [Test]
            public void ShouldHaveEmptyBody()
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.Body)
                    .Not.To.Be.Null();
                Expect(result.Body.ReadAllBytes())
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

            [Test]
            public void ShouldHaveHtmlContentType()
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.ContentType)
                    .To.Equal("text/html");
            }

            [Test]
            public void ShouldHaveCookiesObject()
            {
                // Arrange
                var result = BuildDefault();
                // Act
                Expect(result.Cookies)
                    .Not.To.Be.Null();
                // Assert
            }

            [Test]
            public void ShouldNotHaveStarted()
            {
                // Arrange
                // Act
                var result = BuildDefault();
                // Assert
                Expect(result.HasStarted)
                    .To.Be.False();
            }

            private static HttpResponse BuildDefault()
            {
                return HttpResponseBuilder.BuildDefault();
            }
        }

        [Test]
        public void ShouldBeAbleToAddOnStartedHandler()
        {
            // Arrange
            var called = false;
            Func<object, Task> capturedFunc = null;
            object capturedState = null;
            Func<object, Task> inputFunc = o => Task.CompletedTask;
            var inputState = new object();
            // Act
            var result = HttpResponseBuilder.Create()
                .WithOnStartingHandler((func, state) =>
                {
                    capturedFunc = func;
                    capturedState = state;
                    called = true;
                })
                .Build();
            // Assert
            result.OnStarting(inputFunc, inputState);
            Expect(called)
                .To.Be.True();
            Expect(capturedFunc)
                .To.Be(inputFunc);
            Expect(capturedState)
                .To.Be(inputState);
        }

        [Test]
        public void ShouldBeAbleToAddOnCompletedHandler()
        {
            // Arrange
            var called = false;
            Func<object, Task> capturedFunc = null;
            object capturedState = null;
            Func<object, Task> inputFunc = o => Task.CompletedTask;
            var inputState = new object();
            // Act
            var result = HttpResponseBuilder.Create()
                .WithOnCompletedHandler((func, state) =>
                {
                    capturedFunc = func;
                    capturedState = state;
                    called = true;
                })
                .Build();
            // Assert
            result.OnCompleted(inputFunc, inputState);
            Expect(called)
                .To.Be.True();
            Expect(capturedFunc)
                .To.Be(inputFunc);
            Expect(capturedState)
                .To.Be(inputState);
        }

        [Test]
        public void ShouldBeAbleToAddRedirectHandler()
        {
            // Arrange
            var expectedLocation = GetRandomHttpUrl();
            var expectedPermanent = GetRandomBoolean();
            var capturedLocation = null as string;
            var capturedPermanent = null as bool?;
            var called = false;
            // Act
            var result = HttpResponseBuilder.Create()
                .WithRedirectHandler((location, permanent) =>
                {
                    capturedLocation = location;
                    capturedPermanent = permanent;
                    called = true;
                }).Build();
            // Assert
            result.Redirect(expectedLocation, expectedPermanent);
            Expect(called)
                .To.Be.True();
            Expect(capturedLocation)
                .To.Equal(expectedLocation);
            Expect(capturedPermanent)
                .To.Equal(expectedPermanent);
        }

        [Test]
        public void ShouldBeAbleToSetHttpContext()
        {
            // Arrange
            var expected = new FakeHttpContext();
            // Act
            var result = HttpResponseBuilder.Create()
                .WithHttpContext(expected)
                .Build();
            // Assert
            Expect(result.HttpContext)
                .To.Be(expected);
        }

        [Test]
        public void ShouldBeAbleToSetHttpContextDelayed()
        {
            // Arrange
            var expected = new FakeHttpContext();
            // Act
            var result = HttpResponseBuilder.Create()
                .WithHttpContext(() => expected)
                .Build();
            // Assert
            Expect(result.HttpContext)
                .To.Be(expected);
        }

        [Test]
        public void ShouldBeAbleToSetStatusCodeFromEnum()
        {
            // Arrange
            var expected = GetRandom<HttpStatusCode>();
            // Act
            var result = HttpResponseBuilder.Create()
                .WithStatusCode(expected)
                .Build();
            // Assert
            Expect(result.StatusCode)
                .To.Equal((int) expected);
        }

        [Test]
        public void ShouldBeAbleToSetStatusCodeFromInt()
        {
            // Arrange
            var expected = (int) GetRandom<HttpStatusCode>();
            // Act
            var result = HttpResponseBuilder.Create()
                .WithStatusCode(expected)
                .Build();
            // Assert
            Expect(result.StatusCode)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldBeAbleToSetHasStarted()
        {
            // Arrange
            var expected = GetRandomBoolean();
            // Act
            var result = HttpResponseBuilder.Create()
                .WithHasStarted(expected)
                .Build();
            // Assert
            Expect(result.HasStarted)
                .To.Equal(expected);
        }
    }
}