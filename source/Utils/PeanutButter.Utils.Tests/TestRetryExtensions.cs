using System;
using NUnit.Framework;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestRetryExtensions
{
    [Test]
    public void ShouldRetryTheNumberOfPrescribedTimes()
    {
        // Arrange
        var calls = 0;
        var countAndFail = () =>
        {
            calls++;
            throw new Exception("nope");
        };
        var expected = GetRandomInt(4, 6);
        // Act
        Expect(() => countAndFail.RunWithRetries(expected))
            .To.Throw();
        // Assert
        Expect(calls)
            .To.Equal(expected);
    }

    [Test]
    public void ShouldRaiseTheLastErrorOnFail()
    {
        // Arrange
        var calls = 0;
        var expected = GetRandomInt(3, 7);
        var countAndFail = () =>
        {
            calls++;
            throw new CustomException($"calls: {calls}");
        };
        // Act
        Expect(() => countAndFail.RunWithRetries(expected))
            .To.Throw<CustomException>()
            .With.Message.Equal.To($"calls: {expected}");
        // Assert
    }

    [Test]
    public void ShouldReturnOnFirstSuccess()
    {
        // Arrange
        var calls = 0;
        var expected = GetRandomInt(3, 7);
        var countAndFail = () =>
        {
            calls++;
            if (calls == expected)
            {
                return;
            }
            throw new CustomException($"wibbles: {calls}");
        };
        // Act
        Expect(() => countAndFail.RunWithRetries(100))
            .Not.To.Throw();
        // Assert
        Expect(calls)
            .To.Equal(expected);
    }

    public class CustomException : Exception
    {
        public CustomException(string message) : base(message)
        {
        }
    }
}