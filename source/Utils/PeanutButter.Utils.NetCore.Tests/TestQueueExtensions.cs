using System.Collections.Generic;
using System.Linq;
using NExpect;
using NUnit.Framework;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static NExpect.Expectations;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestQueueExtensions
    {
        [TestFixture]
        public class TryDequeue
        {
            [Test]
            public void ShouldReturnFalseForEmptyQueue()
            {
                // Arrange
                var q = new Queue<int>();
                // Act
                var result = q.TryDequeue(out var i);
                // Assert
                Expect(result)
                    .To.Be.False();
                Expect(i)
                    .To.Equal(default);
            }

            [Test]
            public void ShouldReturnTrueAndFirstItemInQueue()
            {
                // Arrange
                var values = GetRandomArray<int>(3, 5);
                var expected = values.First();
                var q = new Queue<int>(values);
                // Act
                var result = q.TryDequeue(out var i);
                // Assert
                Expect(result)
                    .To.Be.True();
                Expect(i)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldDequeueUntilEmptyThenAlwaysReturnFalse()
            {
                // Arrange
                var values = GetRandomArray<int>(3, 5);
                var collectedValues = new List<int>();
                var collectedResults = new List<bool>();
                var expectedResults = new List<bool>();
                for (var i = 0; i < values.Length; i++)
                {
                    expectedResults.Add(true);
                }

                while (expectedResults.Count < 10)
                {
                    expectedResults.Add(false);
                }
                

                var q = new Queue<int>(values);
                
                // Act
                for (var i = 0; i < 10; i++)
                {
                    collectedResults.Add(q.TryDequeue(out var v));
                    collectedValues.Add(v);
                }
                // Assert
                Expect(collectedResults)
                    .To.Equal(expectedResults);
                Expect(collectedValues.Take(values.Length))
                    .To.Equal(values);
                Expect(collectedValues.Skip(values.Length))
                    .To.Contain.All.Equal.To(0);

            }
        }

        [TestFixture]
        public class DequeueOrDefault
        {
            [Test]
            public void ShouldReturnDefaultForEmptyQueue()
            {
                // Arrange
                var q = new Queue<int>();
                var expected = GetRandomInt(1);
                // Act
                var result1 = q.DequeueOrDefault();
                var result2 = q.DequeueOrDefault(expected);
                // Assert
                Expect(result1)
                    .To.Equal(0);
                Expect(result2)
                    .To.Equal(expected);
            }

            [Test]
            public void ShouldDequeueThenContinuallyReturnTheProvidedDefault()
            {
                // Arrange
                var values = GetRandomArray<int>(3, 5);
                var q = new Queue<int>(values);
                var collected = new List<int>();
                var fallback = GetRandomInt(20);
                // Act
                for (var i = 0; i < 10; i++)
                {
                    collected.Add(q.DequeueOrDefault(fallback));
                }
                // Assert
                Expect(collected.Take(values.Length))
                    .To.Equal(values);
                Expect(collected.Skip(values.Length))
                    .To.Contain.All.Equal.To(fallback);
            }
        }
    }
}