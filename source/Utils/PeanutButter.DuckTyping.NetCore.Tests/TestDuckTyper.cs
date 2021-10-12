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

        [Test]
        public void TryConvertStringToDecimal()
        {
            // Arrange
            var s = "1.23";
            // Act
            var converted = s.TryConvertTo<decimal>(out var result);
            // Assert
            Expect(converted)
                .To.Be.True();
            Expect(result)
                .To.Equal(1.23M);
        }

        [Test]
        public void TryConvertStringToDecimal2()
        {
            // Arrange
            var s = "1.23";
            // Act
            var converted = s.TryConvertTo(typeof(decimal), out var result);
            // Assert
            Expect(converted)
                .To.Be.True();
            Expect(result)
                .To.Equal(1.23M);
        }

        [Test]
        public void TryConvertStringToString()
        {
            // Arrange
            var s = "abc123";
            // Act
            var converted = s.TryConvertTo<string>(out var result);
            // Assert
            Expect(converted)
                .To.Be.True();
            Expect(result)
                .To.Equal(s);
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