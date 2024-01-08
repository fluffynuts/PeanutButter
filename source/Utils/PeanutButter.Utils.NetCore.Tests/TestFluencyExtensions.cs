using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestFluencyExtensions
    {
        [TestFixture]
        public class With
        {
            [Test]
            public void ShouldApplyTransformAndReturnMutatedObject()
            {
                // Arrange
                var expected = GetRandomInt();
                var poco = new Poco();
                // Act
                var result = poco
                    .With(o => o.Id = expected);
                // Assert
                Expect(result)
                    .To.Be(poco);
                Expect(result.Id)
                    .To.Equal(expected);
            }
        }

        public class Poco
        {
            public int Id { get; set; }
        }
    }
}