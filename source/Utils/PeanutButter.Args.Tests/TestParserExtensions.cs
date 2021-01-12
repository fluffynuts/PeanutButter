using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Args.Tests
{
    [TestFixture]
    public class TestArgsParser
    {
        [Test]
        public void ShouldParseArgumentBasedOnShortName()
        {
            // Arrange
            var expected = GetRandomInt(1, 32768);
            var args = new[] { "-p", expected.ToString() };
            // Act
            var result = args.Parse<IArgs>();
            // Assert
            Expect(result.Port)
                .To.Equal(expected);
        }

        public interface IArgs
        {
            [ShortName('p')]
            public int Port { get; set; }
        }
    }
}