using NUnit.Framework;
using NExpect;
using PeanutButter.DuckTyping.Extensions;
using static NExpect.Expectations;

namespace PeanutButter.DuckTyping.NetCore.Tests
{
    [TestFixture]
    public class TestDuckTyper
    {
        [Test]
        public void ShouldBeAbleToDuckInNetCore()
        {
            // Arrange
            var data = new HasAnId() { Id = 42 };
            // Act
            var ducked = data.DuckAs<IHasAnId>();
            // Assert
            Expect(ducked.Id)
                .To.Equal(42);
        }
    }

    public interface IHasAnId
    {
        public int Id { get; set; }
    }

    public class HasAnId
    {
        public int Id { get; set; }
    }
}