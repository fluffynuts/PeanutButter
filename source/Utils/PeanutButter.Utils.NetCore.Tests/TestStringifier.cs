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
        public void ShouldStringifyHttpContext()
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
    }
}