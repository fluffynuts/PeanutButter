using System;
using NUnit.Framework;
using NExpect;
using PeanutButter.TestUtils.AspNetCore.Builders;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

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
            Console.WriteLine(result);
        }
    }
}