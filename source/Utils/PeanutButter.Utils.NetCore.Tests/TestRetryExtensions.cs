using System;
using System.Threading.Tasks;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestRetryExtensions
    {
        [TestFixture]
        [Parallelizable]
        public class SynchronousCode
        {
            [TestFixture]
            [Parallelizable]
            public class RetryingAnAction
            {
                [TestFixture]
                [Parallelizable]
                public class GivenZeroRetries
                {
                    [Test]
                    [Parallelizable]
                    public void ShouldRunActionOnce()
                    {
                        // Arrange
                        var calls = 0;
                        var func = new Action(() => calls++);
                        // Act
                        func.RunWithRetries(0);
                        // Assert
                        Expect(calls)
                            .To.Equal(1);
                    }
                }

                [Test]
                [Parallelizable]
                public void ShouldRetryTheSpecifiedNumberOfTime()
                {
                    // Arrange
                    var count = 0;
                    var runTo = 1000;
                    var action = new Action(
                        () =>
                        {
                            if (++count < runTo)
                            {
                                throw new Exception("moo");
                            }
                        }
                    );
                    // Act
                    Expect(
                        () =>
                        {
                            action.RunWithRetries(runTo);
                        }
                    ).Not.To.Throw();
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                }

                [Test]
                [Parallelizable]
                public void ShouldExitTheFirstTimeTheActionRunsProperly()
                {
                    // Arrange
                    var count = 0;
                    var action = new Action(() => count++);

                    // Act
                    action.RunWithRetries(5);
                    // Assert
                    Expect(count)
                        .To.Equal(1);
                }

                [Test]
                [Parallelizable]
                public void ShouldBackOffByTheSingleProvidedTimeSpan()
                {
                    // Arrange
                    var count = 0;
                    var runTo = GetRandomInt(3, 5);
                    var action = new Action(
                        () =>
                        {
                            if (++count < runTo)
                            {
                                throw new Exception("moo");
                            }
                        }
                    );
                    // Act
                    var runTime = Benchmark.Time(
                        () =>
                            Expect(
                                () =>
                                {
                                    action.RunWithRetries(runTo, TimeSpan.FromMilliseconds(100));
                                }
                            ).Not.To.Throw()
                    );
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(runTime.TotalMilliseconds)
                        .To.Be.Greater.Than(100 * (runTo - 1));
                }

                [Test]
                [Parallelizable]
                public void ShouldBackOffByTheProvidedTimeSpans()
                {
                    // Arrange
                    var count = 0;
                    var runTo = 4;
                    var action = new Action(
                        () =>
                        {
                            if (++count < runTo)
                            {
                                throw new Exception("moo");
                            }
                        }
                    );
                    // Act
                    var runTime = Benchmark.Time(
                        () =>
                            Expect(
                                () =>
                                {
                                    action.RunWithRetries(
                                        runTo,
                                        TimeSpan.FromMilliseconds(100),
                                        TimeSpan.FromMilliseconds(200),
                                        TimeSpan.FromMilliseconds(300),
                                        TimeSpan.FromMilliseconds(400)
                                    );
                                }
                            ).Not.To.Throw()
                    );
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(runTime.TotalMilliseconds)
                        .To.Be.Greater.Than(
                            600,
                            () => "should have exhausted the first three delays"
                        ).And.To.Be.Less.Than(
                            1000,
                            () => "should not have hit the final delay"
                        );
                }
            }

            [TestFixture]
            [Parallelizable]
            public class RetryingAFunc
            {
                [Test]
                [Parallelizable]
                public void ShouldRetryTheSpecifiedNumberOfTime()
                {
                    // Arrange
                    var count = 0;
                    var runTo = 1000;
                    var expected = GetRandomInt(1);
                    var func = new Func<int>(
                        () =>
                        {
                            if (++count < runTo)
                            {
                                throw new Exception("moo");
                            }

                            return expected;
                        }
                    );
                    // Act
                    var result = -1;
                    var runTime = Benchmark.Time(
                        () =>
                            Expect(
                                () =>
                                {
                                    result = func.RunWithRetries(runTo);
                                }
                            ).Not.To.Throw()
                    );
                    Console.WriteLine($"Runtime: {runTime}");
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(result)
                        .To.Equal(expected);
                }

                [Test]
                [Parallelizable]
                public void ShouldExitTheFirstTimeTheFuncRunsProperly()
                {
                    // Arrange
                    var count = 0;
                    var action = new Func<int>(() => count++);

                    // Act
                    action.RunWithRetries(5);
                    // Assert
                    Expect(count)
                        .To.Equal(1);
                }

                [Test]
                [Parallelizable]
                public void ShouldBackOffByTheSingleProvidedTimeSpan()
                {
                    // Arrange
                    var count = 0;
                    var runTo = GetRandomInt(3, 5);
                    var action = new Func<int>(
                        () =>
                        {
                            if (++count < runTo)
                            {
                                throw new Exception("moo");
                            }

                            return 0;
                        }
                    );
                    // Act
                    var runTime = Benchmark.Time(
                        () =>
                            Expect(
                                () =>
                                {
                                    action.RunWithRetries(runTo, TimeSpan.FromMilliseconds(100));
                                }
                            ).Not.To.Throw()
                    );
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(runTime.TotalMilliseconds)
                        .To.Be.Greater.Than(100 * (runTo - 1));
                }

                [Test]
                [Parallelizable]
                public void ShouldBackOffByTheProvidedTimeSpans()
                {
                    // Arrange
                    var count = 0;
                    var runTo = 4;
                    var action = new Func<int>(
                        () =>
                        {
                            if (++count < runTo)
                            {
                                throw new Exception("moo");
                            }

                            return 0;
                        }
                    );
                    // Act
                    var runTime = Benchmark.Time(
                        () =>
                            Expect(
                                () =>
                                {
                                    action.RunWithRetries(
                                        runTo,
                                        TimeSpan.FromMilliseconds(100),
                                        TimeSpan.FromMilliseconds(200),
                                        TimeSpan.FromMilliseconds(300),
                                        TimeSpan.FromMilliseconds(400)
                                    );
                                }
                            ).Not.To.Throw()
                    );
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(runTime.TotalMilliseconds)
                        .To.Be.Greater.Than(
                            600,
                            () => "should have exhausted the first three delays"
                        ).And.To.Be.Less.Than(
                            1000,
                            () => "should not have hit the final delay"
                        );
                }
            }
        }

        [TestFixture]
        [Parallelizable]
        public class AsynchronousCode
        {
            [TestFixture]
            [Parallelizable]
            public class RetryingAnAsyncAction
            {
                [TestFixture]
                [Parallelizable]
                public class WhenRetriesIsZero
                {
                    [Test]
                    [Parallelizable]
                    public async Task ShouldRunTheCodeOnce()
                    {
                        // Arrange
                        var calls = 0;
                        var func = new Func<Task>(
                            () =>
                            {
                                calls++;
                                return Task.CompletedTask;
                            }
                        );
                        // Act
                        await func.RunWithRetries(0);
                        // Assert
                        Expect(calls)
                            .To.Equal(1);
                    }
                }

                [Test]
                [Parallelizable]
                public async Task ShouldRetryTheSpecifiedNumberOfTime()
                {
                    // Arrange
                    var count = 0;
                    var runTo = 1000;
                    var action = new Func<Task>(
                        () =>
                        {
                            if (++count < runTo)
                            {
                                throw new Exception("moo");
                            }

                            return Task.CompletedTask;
                        }
                    );
                    // Act
                    await action.RunWithRetries(runTo);
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                }

                [Test]
                [Parallelizable]
                public async Task ShouldExitTheFirstTimeTheActionRunsProperly()
                {
                    // Arrange
                    var count = 0;
                    var action = new Func<Task>(
                        () =>
                        {
                            count++;
                            return Task.CompletedTask;
                        }
                    );

                    // Act
                    await action.RunWithRetries(5);
                    // Assert
                    Expect(count)
                        .To.Equal(1);
                }

                [Test]
                [Parallelizable]
                public void ShouldBackOffByTheSingleProvidedTimeSpan()
                {
                    // Arrange
                    var count = 0;
                    var runTo = GetRandomInt(3, 5);
                    var action = new Func<Task>(
                        () =>
                        {
                            if (++count < runTo)
                            {
                                throw new Exception("moo");
                            }

                            return Task.CompletedTask;
                        }
                    );
                    // Act
                    var runTime = Benchmark.Time(
                        () =>
                            Expect(
                                async () =>
                                {
                                    await action.RunWithRetries(runTo, TimeSpan.FromMilliseconds(100));
                                }
                            ).Not.To.Throw()
                    );
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(runTime.TotalMilliseconds)
                        .To.Be.Greater.Than(100 * (runTo - 1));
                }

                [Test]
                [Parallelizable]
                public void ShouldBackOffByTheProvidedTimeSpans()
                {
                    // Arrange
                    var count = 0;
                    var runTo = 4;
                    var action = new Func<Task>(
                        () =>
                        {
                            if (++count < runTo)
                            {
                                throw new Exception("moo");
                            }

                            return Task.CompletedTask;
                        }
                    );
                    // Act
                    var runTime = Benchmark.Time(
                        () =>
                            Expect(
                                async () =>
                                {
                                    await action.RunWithRetries(
                                        runTo,
                                        TimeSpan.FromMilliseconds(100),
                                        TimeSpan.FromMilliseconds(200),
                                        TimeSpan.FromMilliseconds(300),
                                        TimeSpan.FromMilliseconds(400)
                                    );
                                }
                            ).Not.To.Throw()
                    );
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(runTime.TotalMilliseconds)
                        .To.Be.Greater.Than(
                            600,
                            () => "should have exhausted the first three delays"
                        ).And.To.Be.Less.Than(
                            1000,
                            () => "should not have hit the final delay"
                        );
                }
            }

            [TestFixture]
            [Parallelizable]
            public class RetryingAnAsyncFunc
            {
                [Test]
                [Parallelizable]
                public void ShouldRetryTheSpecifiedNumberOfTime()
                {
                    // Arrange
                    var count = 0;
                    var runTo = 1000;
                    var expected = GetRandomInt(1);
                    var func = new Func<Task<int>>(
                        () =>
                        {
                            if (++count < runTo)
                            {
                                throw new Exception("moo");
                            }

                            return Task.FromResult(expected);
                        }
                    );
                    // Act
                    var result = -1;
                    var runTime = Benchmark.Time(
                        () =>
                            Expect(
                                async () =>
                                {
                                    result = await func.RunWithRetries(runTo);
                                }
                            ).Not.To.Throw()
                    );
                    Console.WriteLine($"Runtime: {runTime}");
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(result)
                        .To.Equal(expected);
                }

                [Test]
                [Parallelizable]
                public void ShouldExitTheFirstTimeTheFuncRunsProperly()
                {
                    // Arrange
                    var count = 0;
                    var action = new Func<int>(() => count++);

                    // Act
                    action.RunWithRetries(5);
                    // Assert
                    Expect(count)
                        .To.Equal(1);
                }

                [Test]
                [Parallelizable]
                public void ShouldBackOffByTheSingleProvidedTimeSpan()
                {
                    // Arrange
                    var count = 0;
                    var runTo = GetRandomInt(3, 5);
                    var action = new Func<Task<int>>(
                        () =>
                        {
                            if (++count < runTo)
                            {
                                throw new Exception("moo");
                            }

                            return Task.FromResult(0);
                        }
                    );
                    // Act
                    var runTime = Benchmark.Time(
                        () =>
                            Expect(
                                async () =>
                                {
                                    await action.RunWithRetries(runTo, TimeSpan.FromMilliseconds(100));
                                }
                            ).Not.To.Throw()
                    );
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(runTime.TotalMilliseconds)
                        .To.Be.Greater.Than(100 * (runTo - 1));
                }

                [Test]
                [Parallelizable]
                public void ShouldBackOffByTheProvidedTimeSpans()
                {
                    // Arrange
                    var count = 0;
                    var runTo = 4;
                    var action = new Func<Task<int>>(
                        () =>
                        {
                            if (++count < runTo)
                            {
                                throw new Exception("moo");
                            }

                            return Task.FromResult(0);
                        }
                    );
                    // Act
                    var runTime = Benchmark.Time(
                        () =>
                            Expect(
                                async () =>
                                {
                                    await action.RunWithRetries(
                                        runTo,
                                        TimeSpan.FromMilliseconds(100),
                                        TimeSpan.FromMilliseconds(200),
                                        TimeSpan.FromMilliseconds(300),
                                        TimeSpan.FromMilliseconds(400)
                                    );
                                }
                            ).Not.To.Throw()
                    );
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(runTime.TotalMilliseconds)
                        .To.Be.Greater.Than(
                            600,
                            () => "should have exhausted the first three delays"
                        ).And.To.Be.Less.Than(
                            1000,
                            () => "should not have hit the final delay"
                        );
                }
            }
        }
    }
}