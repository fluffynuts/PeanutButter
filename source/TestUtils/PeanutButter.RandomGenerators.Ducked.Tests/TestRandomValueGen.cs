using NExpect;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.RandomGenerators.Ducked.Tests
{
    [TestFixture]
    public class TestRandomValueGen
    {
        public interface IPerson
        {
            string Name { get; set; }
            int Age { get; set; }
        }
        [Test]
        public void WhenHaveDuckTyperAvailable_ShouldBeAbleToCreateRandomInstanceOfInterface()
        {
            // Arrange
            // Pre-Assert
            // Act
            var result = GetRandom<IPerson>();
            // Assert
            Expect(result).Not.To.Be.Null();
        }
    }
}
