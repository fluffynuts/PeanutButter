using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using NExpect;
using PeanutButter.RandomGenerators.Core.Tests.Domain;
using static NExpect.Expectations;

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
    }
}