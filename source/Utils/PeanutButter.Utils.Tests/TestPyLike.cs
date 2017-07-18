using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static PeanutButter.Utils.PyLike;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestPyLike : AssertionHelper
    {
        [Test]
        public void Range_GivenCeilingOf0_ShouldYieldNoResults()
        {
            // Arrange
            // Pre-Assert
            // Act
            var result = Range(0).ToArray();
            // Assert
            Expect(result, Is.Empty);
        }

        [Test]
        public void Range_GivenCeilingOf1_ShouldReturnCollectionWithOnlyZero()
        {
            // Arrange
            // Pre-Assert
            // Act
            var result = Range(1).ToArray();
            // Assert
            Expect(result, Is.EquivalentTo(new[] {0}));
        }

        [Test]
        public void Range_GivenCeilingOfSomeNumber_ShouldReturnCollectionWithNumbersFromZeroToJustBelowCeiling()
        {
            // Arrange
            var ceiling = GetRandomInt(6, 12);
            var expected = new List<int>();
            for (var i = 0; i < ceiling; i++)
            {
                expected.Add(i);
            }
            // Pre-Assert
            // Act
            var result = Range(ceiling).ToArray();
            // Assert
            Expect(result, Is.EquivalentTo(expected));
        }

        [Test]
        public void
            Range_GivenStartAndCeiling_WhenCeilingGreaterThanStart_ShouldResultInRangeFromStartToJustUnderCeiling()
        {
            // Arrange
            var start = GetRandomInt(5, 8);
            var ceiling = GetRandomInt(12, 24);
            var expected = new List<int>();
            for (var i = start; i < ceiling; i++)
            {
                expected.Add(i);
            }
            // Pre-Assert
            // Act
            var result = Range(start, ceiling).ToArray();
            // Assert
            Expect(result, Is.EquivalentTo(expected));
        }

        [Test]
        public void
            Range_GivenStartAndCeilingAndStep_ShouldProduceSteppedResult()
        {
            // Arrange
            var start = GetRandomInt(5, 18);
            var ceiling = GetRandomInt(22, 34);
            var step = GetRandomInt(2, 3);
            var expected = new List<int>();
            for (var i = start; i < ceiling; i += step)
            {
                expected.Add(i);
            }
            // Pre-Assert
            // Act
            var result = Range(start, ceiling, step).ToArray();
            // Assert
            Expect(result, Is.EquivalentTo(expected));
        }

        [Test]
        public void Range_GivenNegativeProgression_ShouldReturnEmptyCollection()
        {
            // Arrange
            var start = GetRandomInt(5, 10);
            var ceiling = GetRandomInt(-5, 2);
            // Pre-Assert
            // Act
            var result = Range(start, ceiling).ToArray();
            // Assert
            Expect(result, Is.Empty);
        }

        [Test]
        public void Zip_WhenCollectionsHaveTheSameNumberOfItems_ShouldGetItemPairs()
        {
            // Arrange
            var left = new[] { 1, 2, 3 };
            var right = new[] { "a", "b", "c" };
            // Pre-Assert
            // Act
            var result = Zip(left, right).ToArray();
            // Assert
            Expect(result, Has.Length.EqualTo(3));
            Expect(result[0].Item1, Is.EqualTo(left[0]));
            Expect(result[0].Item2, Is.EqualTo(right[0]));
            Expect(result[1].Item1, Is.EqualTo(left[1]));
            Expect(result[1].Item2, Is.EqualTo(right[1]));
            Expect(result[2].Item1, Is.EqualTo(left[2]));
            Expect(result[2].Item2, Is.EqualTo(right[2]));
        }

        [Test]
        public void Zip_WhenCollectionsHaveDifferntNumberOfItems_ShouldOnlyReturnFullPairs()
        {
            // Arrange
            var left = new[] { 1, 2 };
            var right = new[] { "a", "b", "c" };
            // Pre-Assert
            // Act
            var result = Zip(left, right).ToArray();
            // Assert
            Expect(result, Has.Length.EqualTo(2));
            Expect(result[0].Item1, Is.EqualTo(left[0]));
            Expect(result[0].Item2, Is.EqualTo(right[0]));
            Expect(result[1].Item1, Is.EqualTo(left[1]));
            Expect(result[1].Item2, Is.EqualTo(right[1]));
        }
    }
}