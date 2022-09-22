using Microsoft.Extensions.Primitives;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using PeanutButter.TestUtils.AspNetCore.Builders;
using static NExpect.Expectations;

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