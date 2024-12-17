using System.Collections.Generic;
using PeanutButter.RandomGenerators.Core.Tests.Domain;

namespace PeanutButter.RandomGenerators.Core.Tests
{
    [TestFixture]
    public class TestGetRandom
    {
        // ReSharper disable once UnusedMember.Global
        public class PocoBuilder : GenericBuilder<PocoBuilder, Poco>
        {
            public override PocoBuilder WithRandomProps()
            {
                return base.WithRandomProps()
                           .WithProp(o => o.Id = GetRandomInt(100, 200));
            }
        }

        [Test]
        public void ShouldBeAbleToBuildRemoteClassWithLocalBuilder()
        {
            // Arrange
            // Act
            var result = GetRandom<Poco>();
            // Assert
            Expect(result).Not.To.Be.Null();
            Expect(result.Id).To.Be.Greater.Than.Or.Equal.To(100)
                             .And.Less.Than.Or.Equal.To(200);
        }

        [Test]
        public void ShouldBeAbleToCreateRandomObjectsFromConstructorParams()
        {
            // Arrange
            // Act
            var kvp = GetRandom<KeyValuePair<string, int>>();
            // Assert
            Expect(kvp.Key)
                .Not.To.Be.Null();
        }
    }
}