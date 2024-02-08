using Microsoft.AspNetCore.Http;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.TestUtils.AspNetCore.Fakes;

namespace PeanutButter.TestUtils.AspNetCore.Tests;

[TestFixture]
public class TestQueryCollectionBuilder
{
    [TestFixture]
    public class DefaultBuild
    {
        [Test]
        public void ShouldProduceEmptyCollection()
        {
            // Arrange
            // Act
            var result = QueryCollectionBuilder.BuildDefault();
            
            // Assert
            Expect(result)
                .Not.To.Be.Null();
            Expect(result)
                .To.Be.An.Instance.Of<IQueryCollection>();
            Expect(result)
                .To.Be.An.Instance.Of<FakeQueryCollection>();
        }
    }

    [TestFixture]
    public class RandomBuild
    {
        [Test]
        public void ShouldProduceCollectionWithSomeItems()
        {
            // Arrange
            // Act
            var result = QueryCollectionBuilder.BuildRandom();
            // Assert
            Expect(result)
                .Not.To.Be.Empty();
        }
    }

    [TestFixture]
    public class AddingParameters
    {
        [Test]
        public void ShouldBeAbleToAddParameters()
        {
            // Arrange
            var k1 = GetRandomString();
            var k2 = GetRandomString();
            var v1 = GetRandomString();
            var v2 = GetRandomInt();
            
            // Act
            var result = QueryCollectionBuilder.Create()
                .WithParameter(k1, v1)
                .WithParameter(k2, v2)
                .Build();
            // Assert
            Expect(result)
                .To.Contain.Key(k1)
                .With.Value(v1);
            Expect(result)
                .To.Contain.Key(k2)
                .With.Value($"{v2}");
        }
    }
}