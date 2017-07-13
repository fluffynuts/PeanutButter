using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using static PeanutButter.Utils.PyRange;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestRange : AssertionHelper
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
    }
}