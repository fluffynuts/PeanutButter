using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using NExpect;
using NUnit.Framework;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.TestUtils.AspNetCore.Utils;
using PeanutButter.Utils;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.TestUtils.AspNetCore.Tests.Utils;

[TestFixture]
public class TestRequestDelegateTestArena
{
    [Test]
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
    public class Fluently
    {
        [Test]
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