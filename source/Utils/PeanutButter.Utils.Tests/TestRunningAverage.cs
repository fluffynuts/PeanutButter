using System.Linq;
using NUnit.Framework;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestRunningAverage
{
    [TestFixture]
    public class WithNoValues
    {
        [Test]
        public void ShouldReturnZero()
        {
            // Arrange
            var sut = Create();
            // Act
            var result = sut.Average;
            // Assert
            Expect(result)
                .To.Equal(0);
        }
    }

    [TestFixture]
    public class WithOneValue
    {
        [Test]
        public void ShouldReturnThatValue()
        {
            // Arrange
            var expected = GetRandomInt(1);
            var sut = Create();
            // Act
            sut.Add(expected);
            var result = sut.Average;
            // Assert
            Expect(result)
                .To.Equal(expected);
        }
    }

    [TestFixture]
    public class WithNValues
    {
        [Test]
        public void ShouldReturnTheAverage()
        {
            // Arrange
            var values = GetRandomArray<int>(100, 200);
            var expected = values.Select(o => (decimal)o)
                .Average();
            var sut = Create();
            // Act
            sut.Add(values.Select(i => (decimal)i).ToArray());
            var result = sut.Average;
            // Assert
            Expect(result)
                .To.Equal(expected);
        }
    }

    private static RunningAverage Create()
    {
        return new();
    }
}