using PeanutButter.TestUtils.AspNetCore.Builders;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestModelMetadataBuilder
{
    [TestFixture]
    public class DefaultBuild
    {
        [Test]
        public void ShouldHaveEmptyAdditionalValues()
        {
            // Arrange
            // Act
            var result = ModelMetadataBuilder.BuildDefault();
            // Assert
            Expect(result.AdditionalValues)
                .Not.To.Be.Null();
            Expect(result.AdditionalValues)
                .To.Be.Empty();
        }
    }

    [TestFixture]
    public class FakingProps
    {
        [Test]
        public void ShouldBeAbleToFakeName()
        {
            // Arrange
            var expected = GetRandomString();
            // Act
            var result = ModelMetadataBuilder.For<object>()
                .WithFaked(o => o._DisplayName = expected)
                .Build();
            // Assert
            Expect(result.DisplayName)
                .To.Equal(expected);
        }
    }
}