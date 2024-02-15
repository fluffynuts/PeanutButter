namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestBooleanExtensions
{
    [TestFixture]
    public class Negate
    {
        [Test]
        public void ShouldNegateTheValueInPlaceAndReturnIt()
        {
            // Arrange
            var value = GetRandomBoolean();
            var expected = !value;
            // Act
            var result = value.Negate();
            // Assert
            Expect(result)
                .To.Equal(expected);
            Expect(value)
                .To.Equal(expected);
        }
    }
}