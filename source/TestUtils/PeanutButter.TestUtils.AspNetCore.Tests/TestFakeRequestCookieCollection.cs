using Microsoft.Extensions.Primitives;
using PeanutButter.TestUtils.AspNetCore.Builders;

namespace PeanutButter.TestUtils.AspNetCore.Tests
{
    [TestFixture]
    public class TestFakeRequestCookieCollection
    {
        [Test]
        public void ShouldReturnEmptyStringValueWhenKeyNotFound()
        {
            // Arrange
            var key = GetRandomString();
            var cookies = RequestCookieCollectionBuilder.BuildDefault();
            // Act
            var result = cookies[key];
            // Assert
            Expect(result)
                .To.Equal(new StringValues());
        }
    }
}