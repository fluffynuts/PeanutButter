using NExpect;
using static NExpect.Expectations;
using NUnit.Framework;
using PeanutButter.DuckTyping.AutoConversion.Converters;

namespace PeanutButter.DuckTyping.Tests.AutoConversion.Converters
{
    [TestFixture]
    public class TestEnumConverter
    {
        public enum Priorities
        {
            Low,
            Medium,
            High
        }

        [Test]
        public void TryConvert_GivenStringToEnumWithExactValidValue_ShouldReturnTrue()
        {
            // Arrange
            var value = "Medium";
            var fromType = typeof(string);
            var toType = typeof(Priorities);
            // Pre-Assert
            // Act
            var result = EnumConverter.TryConvert(fromType, toType, value, out var parsed);
            // Assert
            Expect(result).To.Be.True();
            Expect(parsed).To.Equal(Priorities.Medium);
        }

        [Test]
        public void TryConvert_GivenEnumToString_ShouldReturnTrue()
        {
            // Arrange
            var value = Priorities.High;
            var fromType = typeof(Priorities);
            var toType = typeof(string);
            // Pre-Assert
            // Act
            var result = EnumConverter.TryConvert(fromType, toType, value, out var parsed);
            // Assert
            Expect(result).To.Be.True();
            Expect(parsed).To.Equal("High");
        }
    }
}