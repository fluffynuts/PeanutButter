using NExpect;
using PeanutButter.TestUtils.AspNetCore.Builders;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestFakeFormBuilder
{
    [TestFixture]
    public class DefaultBuild
    {
        [Test]
        public void ShouldProduceForm()
        {
            // Arrange
            // Act
            var result = FakeFormBuilder.BuildDefault();
            // Assert
            Expectations.Expect(result)
                .Not.To.Be.Null();
            Expectations.Expect(result.Keys)
                .To.Be.Empty();
        }
    }
}