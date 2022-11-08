using System;
using System.Threading.Tasks;
using NExpect;
using NUnit.Framework;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestRetryExtensions
    {
        [TestFixture]
        public class SynchronousCode
        {
            [TestFixture]
            public class RetryingAnAction
            {
                [Test]
                public void ShouldRetryTheSpecifiedNumberOfTime()
                {
                    // Arrange
                    var count = 0;
                    var runTo = 1000;
                    var action = new Action(() =>
                    {
                        if (++count < runTo)
                        {
                            throw new Exception("moo");
                        }
                    });
                    // Act
                    Expect(() =>
                    {
                        action.RunWithRetries(runTo);
                    }).Not.To.Throw();
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                }

                [Test]
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
                public void ShouldBackOffByTheSingleProvidedTimeSpan()
                {
                    // Arrange
                    var count = 0;
                    var runTo = GetRandomInt(3, 5);
                    var action = new Action(() =>
                    {
                        if (++count < runTo)
                        {
                            throw new Exception("moo");
                        }
                    });
                    // Act
                    var runTime = Benchmark.Time(() =>
                        Expect(() =>
                        {
                            action.RunWithRetries(runTo, TimeSpan.FromMilliseconds(100));
                        }).Not.To.Throw()
                    );
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(runTime.TotalMilliseconds)
                        .To.Be.Greater.Than(100 * (runTo - 1));
                }

                [Test]
                public void ShouldBackOffByTheProvidedTimeSpans()
                {
                    // Arrange
                    var count = 0;
                    var runTo = 4;
                    var action = new Action(() =>
                    {
                        if (++count < runTo)
                        {
                            throw new Exception("moo");
                        }
                    });
                    // Act
                    var runTime = Benchmark.Time(
                        () =>
                            Expect(() =>
                            {
                                action.RunWithRetries(
                                    runTo,
                                    TimeSpan.FromMilliseconds(100),
                                    TimeSpan.FromMilliseconds(200),
                                    TimeSpan.FromMilliseconds(300),
                                    TimeSpan.FromMilliseconds(400)
                                );
                            }).Not.To.Throw()
                    );
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(runTime.TotalMilliseconds)
                        .To.Be.Greater.Than(
                            600, () => "should have exhausted the first three delays"
                        ).And.To.Be.Less.Than(
                            1000, () => "should not have hit the final delay"
                        );
                }
            }

            [TestFixture]
            public class RetryingAFunc
            {
                [Test]
                public void ShouldRetryTheSpecifiedNumberOfTime()
                {
                    // Arrange
                    var count = 0;
                    var runTo = 1000;
                    var expected = GetRandomInt(1);
                    var func = new Func<int>(() =>
                    {
                        if (++count < runTo)
                        {
                            throw new Exception("moo");
                        }

                        return expected;
                    });
                    // Act
                    var result = -1;
                    var runTime = Benchmark.Time(
                        () =>
                            Expect(() =>
                            {
                                result = func.RunWithRetries(runTo);
                            }).Not.To.Throw()
                    );
                    Console.WriteLine($"Runtime: {runTime}");
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(result)
                        .To.Equal(expected);
                }

                [Test]
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
                public void ShouldBackOffByTheSingleProvidedTimeSpan()
                {
                    // Arrange
                    var count = 0;
                    var runTo = GetRandomInt(3, 5);
                    var action = new Func<int>(() =>
                    {
                        if (++count < runTo)
                        {
                            throw new Exception("moo");
                        }

                        return 0;
                    });
                    // Act
                    var runTime = Benchmark.Time(() =>
                        Expect(() =>
                        {
                            action.RunWithRetries(runTo, TimeSpan.FromMilliseconds(100));
                        }).Not.To.Throw()
                    );
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(runTime.TotalMilliseconds)
                        .To.Be.Greater.Than(100 * (runTo - 1));
                }

                [Test]
                public void ShouldBackOffByTheProvidedTimeSpans()
                {
                    // Arrange
                    var count = 0;
                    var runTo = 4;
                    var action = new Func<int>(() =>
                    {
                        if (++count < runTo)
                        {
                            throw new Exception("moo");
                        }

                        return 0;
                    });
                    // Act
                    var runTime = Benchmark.Time(
                        () =>
                            Expect(() =>
                            {
                                action.RunWithRetries(
                                    runTo,
                                    TimeSpan.FromMilliseconds(100),
                                    TimeSpan.FromMilliseconds(200),
                                    TimeSpan.FromMilliseconds(300),
                                    TimeSpan.FromMilliseconds(400)
                                );
                            }).Not.To.Throw()
                    );
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(runTime.TotalMilliseconds)
                        .To.Be.Greater.Than(
                            600, () => "should have exhausted the first three delays"
                        ).And.To.Be.Less.Than(
                            1000, () => "should not have hit the final delay"
                        );
                }
            }
        }

        [TestFixture]
        public class AsynchronousCode
        {
            [TestFixture]
            public class RetryingAnAsyncAction
            {
                [Test]
                public async Task ShouldRetryTheSpecifiedNumberOfTime()
                {
                    // Arrange
                    var count = 0;
                    var runTo = 1000;
                    var action = new Func<Task>(() =>
                    {
                        if (++count < runTo)
                        {
                            throw new Exception("moo");
                        }

                        return Task.CompletedTask;
                    });
                    // Act
                    await action.RunWithRetries(runTo);
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                }

                [Test]
                public async Task ShouldExitTheFirstTimeTheActionRunsProperly()
                {
                    // Arrange
                    var count = 0;
                    var action = new Func<Task>(() =>
                    {
                        count++;
                        return Task.CompletedTask;
                    });

                    // Act
                    await action.RunWithRetries(5);
                    // Assert
                    Expect(count)
                        .To.Equal(1);
                }

                [Test]
                public void ShouldBackOffByTheSingleProvidedTimeSpan()
                {
                    // Arrange
                    var count = 0;
                    var runTo = GetRandomInt(3, 5);
                    var action = new Func<Task>(() =>
                    {
                        if (++count < runTo)
                        {
                            throw new Exception("moo");
                        }
                        return Task.CompletedTask;
                    });
                    // Act
                    var runTime = Benchmark.Time(() =>
                        Expect(async () =>
                        {
                            await action.RunWithRetries(runTo, TimeSpan.FromMilliseconds(100));
                        }).Not.To.Throw()
                    );
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(runTime.TotalMilliseconds)
                        .To.Be.Greater.Than(100 * (runTo - 1));
                }

                [Test]
                public void ShouldBackOffByTheProvidedTimeSpans()
                {
                    // Arrange
                    var count = 0;
                    var runTo = 4;
                    var action = new Func<Task>(() =>
                    {
                        if (++count < runTo)
                        {
                            throw new Exception("moo");
                        }
                        return Task.CompletedTask;
                    });
                    // Act
                    var runTime = Benchmark.Time(
                        () =>
                            Expect(async () =>
                            {
                                await action.RunWithRetries(
                                    runTo,
                                    TimeSpan.FromMilliseconds(100),
                                    TimeSpan.FromMilliseconds(200),
                                    TimeSpan.FromMilliseconds(300),
                                    TimeSpan.FromMilliseconds(400)
                                );
                            }).Not.To.Throw()
                    );
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(runTime.TotalMilliseconds)
                        .To.Be.Greater.Than(
                            600, () => "should have exhausted the first three delays"
                        ).And.To.Be.Less.Than(
                            1000, () => "should not have hit the final delay"
                        );
                }
            }
            
            [TestFixture]
            public class RetryingAnAsyncFunc
            {
                [Test]
                public void ShouldRetryTheSpecifiedNumberOfTime()
                {
                    // Arrange
                    var count = 0;
                    var runTo = 1000;
                    var expected = GetRandomInt(1);
                    var func = new Func<Task<int>>(() =>
                    {
                        if (++count < runTo)
                        {
                            throw new Exception("moo");
                        }

                        return Task.FromResult(expected);
                    });
                    // Act
                    var result = -1;
                    var runTime = Benchmark.Time(
                        () =>
                            Expect(async() =>
                            {
                                result = await func.RunWithRetries(runTo);
                            }).Not.To.Throw()
                    );
                    Console.WriteLine($"Runtime: {runTime}");
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(result)
                        .To.Equal(expected);
                }

                [Test]
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
                public void ShouldBackOffByTheSingleProvidedTimeSpan()
                {
                    // Arrange
                    var count = 0;
                    var runTo = GetRandomInt(3, 5);
                    var action = new Func<Task<int>>(() =>
                    {
                        if (++count < runTo)
                        {
                            throw new Exception("moo");
                        }

                        return Task.FromResult(0);
                    });
                    // Act
                    var runTime = Benchmark.Time(() =>
                        Expect(async () =>
                        {
                            await action.RunWithRetries(runTo, TimeSpan.FromMilliseconds(100));
                        }).Not.To.Throw()
                    );
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(runTime.TotalMilliseconds)
                        .To.Be.Greater.Than(100 * (runTo - 1));
                }

                [Test]
                public void ShouldBackOffByTheProvidedTimeSpans()
                {
                    // Arrange
                    var count = 0;
                    var runTo = 4;
                    var action = new Func<Task<int>>(() =>
                    {
                        if (++count < runTo)
                        {
                            throw new Exception("moo");
                        }

                        return Task.FromResult(0);
                    });
                    // Act
                    var runTime = Benchmark.Time(
                        () =>
                            Expect(async () =>
                            {
                                await action.RunWithRetries(
                                    runTo,
                                    TimeSpan.FromMilliseconds(100),
                                    TimeSpan.FromMilliseconds(200),
                                    TimeSpan.FromMilliseconds(300),
                                    TimeSpan.FromMilliseconds(400)
                                );
                            }).Not.To.Throw()
                    );
                    // Assert

                    Expect(count)
                        .To.Equal(runTo);
                    Expect(runTime.TotalMilliseconds)
                        .To.Be.Greater.Than(
                            600, () => "should have exhausted the first three delays"
                        ).And.To.Be.Less.Than(
                            1000, () => "should not have hit the final delay"
                        );
                }
            }
        }
    }
}