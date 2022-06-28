using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using static NExpect.Expectations;
namespace PeanutButter.RandomGenerators.Tests
{
    [TestFixture]
    public class TestUniqueRandomGenerator
    {
        [Test]
        public void ShouldBeAbleToGenerateALotOfRandomNumbers()
        {
            // Arrange
            var sut = Create<int>();
            // Act
            Expect(() =>
            {
                for (var i = 0; i < 100000; i++)
                {
                    sut.Next();
                }
            }).Not.To.Throw();
            // Assert
        }

        private static UniqueRandomValueGenerator<T> Create<T>()
        {
            return new UniqueRandomValueGenerator<T>();
        }
    }
}