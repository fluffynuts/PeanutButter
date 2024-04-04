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
            [Retry(3)] // works with relatively tight timings - may disrupted by loaded host
            public void ShouldTestActionOnce()
            {
                Assert.That(() =>
                {
                    // Arrange
                    var expected = GetRandomInt(200, 500);
                    var counter = 0;
                    // Act
                    var result = Benchmark.Time(
                        () =>
                        {
                            counter++;
                            Thread.Sleep(expected);
                        }
                    );
                    // Assert
                    Expect(counter)
                        .To.Equal(1);
                    Expect(Math.Abs(result.TotalMilliseconds - expected))
                        .To.Be.Less.Than(25, "benchmarking shouldn't add significant cost");
                }, Throws.Nothing);
            }

            [Test]
            [Retry(3)] // works with relatively tight timings - may disrupted by loaded host
            public void ShouldTestActionProvidedNumberOfTimes()
            {
                Assert.That(() =>
                {
                    // Arrange
                    var delay = GetRandomInt(200, 500);
                    var runs = GetRandomInt(3, 6);
                    var expected = delay * runs;
                    var counter = 0;
                    // Act
                    var result = Benchmark.Time(
                        () =>
                        {
                            counter++;
                            Thread.Sleep(delay);
                        },
                        runs
                    );
                    // Assert
                    Expect(counter)
                        .To.Equal(runs);
                    Expect(Math.Abs(result.TotalMilliseconds - expected))
                        .To.Be.Less.Than(25 * runs);
                }, Throws.Nothing);
            }
        }

        [TestFixture]
        [Parallelizable]
        public class AsyncInterface
        {
            [Test]
            [Retry(5)] // test is time-sensitive, give it a few chances to succeed
            public void ShouldTestActionOnce()
            {
                Assert.That(async () =>
                {
                    // Arrange
                    var expected = GetRandomInt(200, 500);
                    var allowedDrift = 25;
                    var counter = 0;
                    // Act
                    var result = await Benchmark.TimeAsync(
                        async () =>
                        {
                            counter++;
                            await Task.Delay(expected);
                        }
                    );
                    // Assert
                    Expect(counter)
                        .To.Equal(1);
                    Expect(Math.Abs(result.TotalMilliseconds - expected))
                        .To.Be.Less.Than(allowedDrift);
                }, Throws.Nothing);
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