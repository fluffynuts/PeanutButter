using System;
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

        [Test]
        public void CanPerhapsConvertBetween_WhenType1IsEnumTypeAndType2IsStringType_ShouldReturnTrue()
        {
            // Arrange
            var t1 = typeof(Priorities);
            var t2 = typeof(string);
            // Pre-Assert
            // Act
            var result = EnumConverter.CanPerhapsConvertBetween(t1, t2);
            // Assert
            Expect(result).To.Be.True();
        }

        [Test]
        public void CanPerhapsConvertBetween_WhenType1IsStringTypeAndType2IsEnumType_ShouldReturnTrue()
        {
            // Arrange
            var t1 = typeof(string);
            var t2 = typeof(Priorities);
            // Pre-Assert
            // Act
            var result = EnumConverter.CanPerhapsConvertBetween(t1, t2);
            // Assert
            Expect(result).To.Be.True();
        }

        [TestCase(typeof(int), typeof(bool))]
        [TestCase(typeof(string), typeof(bool))]
        [TestCase(typeof(string), typeof(Guid))]
        public void CanPerhapsConvertBetween_ShouldReturnFalseFor_(Type t1, Type t2)
        {
            // Arrange
            // Pre-Assert
            // Act
            var result1 = EnumConverter.CanPerhapsConvertBetween(t1, t2);
            var result2 = EnumConverter.CanPerhapsConvertBetween(t2, t1);
            // Assert
            Expect(result1).To.Be.False();
            Expect(result2).To.Be.False();
        }
    }
}