using System;
using NUnit.Framework;
using PeanutButter.TestUtils.AspNetCore.Builders;
using static NExpect.Expectations;
using NExpect;

namespace PeanutButter.Utils.NetCore.Tests
{
    [TestFixture]
    public class TestStringifier
    {
        [Test]
        public void ShouldStringifyPlainHttpContext()
        {
            // Arrange
            var ctx = HttpContextBuilder.BuildRandom();
            // Act
            var result = ctx.Stringify();
            // Assert
            Expect(result)
                .Not.To.Contain(Stringifier.SEEN_OBJECT_PLACEHOLDER);
            Console.WriteLine(result);
        }

        [Test]
        public void ShouldStringifyHttpContextReferencingItself()
        {
            // Arrange
            var ctx = HttpContextBuilder.BuildRandom();
            ctx.Items["context"] = ctx;
            // Act
            var result = ctx.Stringify();
            // Assert
            var parts = result.Split(Stringifier.SEEN_OBJECT_PLACEHOLDER);
            Expect(parts)
                .To.Contain.Only(2).Items();
            Console.WriteLine(result);
        }
    }
}