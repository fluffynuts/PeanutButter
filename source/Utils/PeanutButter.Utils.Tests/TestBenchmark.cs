using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestBenchmark
    {
        [TestFixture]
        [Parallelizable]
        public class SynchronousInterface
        {
            [Test]
            public void ShouldTestActionOnce()
            {
                // Arrange
                var expected = GetRandomInt(200, 500);
                var counter = 0;
                // Act
                var result = Benchmark.Time(() =>
                {
                    counter++;
                    Thread.Sleep(expected);
                });
                // Assert
                Expect(counter)
                    .To.Equal(1);
                Expect(Math.Abs(result.TotalMilliseconds - expected))
                    .To.Be.Less.Than(25);
            }

            [Test]
            public void ShouldTestActionProvidedNumberOfTimes()
            {
                // Arrange
                var delay = GetRandomInt(200, 500);
                var runs = GetRandomInt(3, 6);
                var expected = delay * runs;
                var counter = 0;
                // Act
                var result = Benchmark.Time(() =>
                {
                    counter++;
                    Thread.Sleep(delay);
                }, runs);
                // Assert
                Expect(counter)
                    .To.Equal(runs);
                Expect(Math.Abs(result.TotalMilliseconds - expected))
                    .To.Be.Less.Than(25 * runs);
            }
        }

        [TestFixture]
        [Parallelizable]
        public class AsyncInterface
        {
            [Test]
            public async Task ShouldTestActionOnce()
            {
                // Arrange
                var expected = GetRandomInt(200, 500);
                var counter = 0;
                // Act
                var result = await Benchmark.TimeAsync(async () =>
                {
                    counter++;
                    await Task.Delay(expected);
                });
                // Assert
                Expect(counter)
                    .To.Equal(1);
                Expect(Math.Abs(result.TotalMilliseconds - expected))
                    .To.Be.Less.Than(25);
            }

            [Test]
            public async Task ShouldTestActionProvidedNumberOfTimes()
            {
                // Arrange
                var delay = GetRandomInt(200, 500);
                var runs = GetRandomInt(3, 6);
                var expected = delay * runs;
                var counter = 0;
                // Act
                var result = await Benchmark.TimeAsync(async () =>
                {
                    counter++;
                    await Task.Delay(delay);
                }, runs);
                // Assert
                Expect(counter)
                    .To.Equal(runs);
                Expect(Math.Abs(result.TotalMilliseconds - expected))
                    .To.Be.Less.Than(25 * runs);
            }
        }
    }
}