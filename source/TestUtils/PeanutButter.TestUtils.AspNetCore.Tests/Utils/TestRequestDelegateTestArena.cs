using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.TestUtils.AspNetCore.Utils;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Tests.Utils;

[TestFixture]
public class TestRequestDelegateTestArena
{
    [Test]
    [Parallelizable]
    public void ShouldDeconstruct()
    {
        // Arrange
        // Act
        var (ctx, next) = RequestDelegateTestArenaBuilder.BuildDefault();
        // Assert
        Expect(ctx)
            .To.Be.An.Instance.Of<HttpContext>();
        Expect(next)
            .To.Be.An.Instance.Of<RequestDelegate>();
    }

    [TestFixture]
    [Parallelizable]
    public class Fluently
    {
        [Test]
        [Parallelizable]
        public void ShouldBeAbleToSetCustomLogic()
        {
            // Arrange
            HttpContext captured = null;
            var otherContext = HttpContextBuilder.BuildRandom();
            var (ctx, next) = RequestDelegateTestArenaBuilder.Create()
                .WithDelegateLogic(dctx => captured = dctx)
                .Build();
            // Act
            next.Invoke(otherContext);
            // Assert
            Expect(captured)
                .To.Be(otherContext);
        }

        [Test]
        [Parallelizable]
        public void ShouldBeAbleToSetNullLogic()
        {
            // Arrange
            var (ctx, next) = RequestDelegateTestArenaBuilder.Create()
                .WithDelegateLogic(null)
                .Build();
            // Act
            Expect(async () => await next.Invoke(ctx))
                .Not.To.Throw();
            // Assert
        }

        [Test]
        [Parallelizable]
        public void ShouldIgnoreNullHttpContextMutator()
        {
            // Arrange
            Expect(
                    async () =>
                    {
                        var (ctx, next) = RequestDelegateTestArenaBuilder.Create()
                            .WithContextMutator(null)
                            .Build();
                        // Act
                        await next.Invoke(ctx);
                    }
                )
                .Not.To.Throw();
            // Assert
        }

        [Test]
        [Parallelizable]
        public void ShouldRecordTheCalls()
        {
            // Arrange
            var arena = RequestDelegateTestArenaBuilder.BuildDefault();
            var otherContext = HttpContextBuilder.BuildRandom();
            var (ctx, next) = arena;
            // Act
            next.Invoke(ctx);
            next.Invoke(otherContext);
            // Assert
            var recorded = next.GetMetadata<List<HttpContext>>(
                RequestDelegateTestArena.METADATA_KEY_CALL_ARGS
            );
            Expect(recorded)
                .To.Equal(new[] { ctx, otherContext });
        }

        [Test]
        [Parallelizable]
        public void ShouldBeAbleToMutateTheContext()
        {
            // Arrange
            var (key, value) = (GetRandomString(), GetRandomString());
            // Act
            var (ctx, _) = RequestDelegateTestArenaBuilder.Create()
                .WithContextMutator(
                    builder => builder.WithItem(key, value)
                ).Build();
            // Assert
            Expect(ctx.Items[key])
                .To.Equal(value);
        }

        [Test]
        [Parallelizable]
        public void ShouldHaveEasierAccessToModifyTheRequest()
        {
            // Arrange
            var (ctx, _) = RequestDelegateTestArenaBuilder.Create()
                .WithRequestMutator(
                    req => req.Method = "OPTIONS"
                ).Build();
            // Act
            Expect(ctx.Request.Method)
                .To.Equal("OPTIONS");
            // Assert
        }

        [Test]
        public void ShouldBeAbleToStartWithOptionsRequest()
        {
            // Arrange
            // Act
            var (ctx, _) = RequestDelegateTestArenaBuilder.Create()
                .ForOptionsRequest()
                .Build();
            // Assert
            Expect(ctx.Request.Method)
                .To.Equal("OPTIONS");
        }

        [Test]
        [Parallelizable]
        public void ShouldHaveEasierAccessToSetTheRequest()
        {
            // Arrange
            var expected = HttpRequestBuilder.BuildDefault();
            var (ctx, _) = RequestDelegateTestArenaBuilder.Create()
                .WithRequest(expected)
                .Build();
            // Act
            Expect(ctx.Request)
                .To.Be(expected);
            // Assert
        }

        [Test]
        [Parallelizable]
        public void ShouldHaveEasyMethodToAddOriginHeader()
        {
            // Arrange
            var expected = GetRandomHttpsUrl();
            // Act
            var (ctx, _) = RequestDelegateTestArenaBuilder.Create()
                .WithOriginHeader(expected)
                .Build();
            // Assert
            Expect(ctx.Request.Headers)
                .To.Contain.Key("Origin")
                .With.Value(expected);
        }

        [Test]
        [Parallelizable]
        public void ShouldHaveEasyMethodToAddSelfOrigin()
        {
            // Arrange
            var requestUrl = GetRandomHttpsUrlWithPathAndParameters();
            var expected = new Uri(requestUrl).ToString().UriRoot();

            // Act
            var (ctx, _) = RequestDelegateTestArenaBuilder.Create()
                .WithRequestMutator(req => req.SetUrl(requestUrl))
                .WithOriginHeader()
                .Build();
            // Assert
            Expect(ctx.Request.Headers)
                .To.Contain.Key("Origin")
                .With.Value(expected);
        }

        [Test]
        public void ShouldHaveEasyAccessToMutateTheResponse()
        {
            // Arrange
            var expected = GetRandomMimeType();
            var (ctx, _) = RequestDelegateTestArenaBuilder.Create()
                .WithResponseMutator(res => res.ContentType = expected)
                .Build();
            
            // Act
            var result = ctx.Response.ContentType;
            // Assert
            Expect(result)
                .To.Equal(expected);
        }

        [Test]
        public void ShouldHaveEasyAccessToReplaceTheResponse()
        {
            // Arrange
            var expected = HttpResponseBuilder.BuildRandom();
            var (ctx, _) = RequestDelegateTestArenaBuilder.Create()
                .WithResponse(expected)
                .Build();
            
            // Act
            var result = ctx.Response;
            // Assert
            Expect(result)
                .To.Be(expected);
        }

        [Test]
        [Parallelizable]
        public void ShouldBeAbleToOutrightSetTheContext()
        {
            // Arrange
            var expected = HttpContextBuilder.BuildRandom();
            // Act
            var (ctx, _) = RequestDelegateTestArenaBuilder.Create()
                .WithContext(expected)
                .Build();
            // Assert
            Expect(ctx)
                .To.Be(expected);
        }
    }
}