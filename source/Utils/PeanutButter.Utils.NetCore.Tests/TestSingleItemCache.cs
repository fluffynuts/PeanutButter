using System.Collections.Concurrent;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.NetCore.Tests
{
    [TestFixture]
    public class TestSingleItemCache
    {
        [TestFixture]
        public class WhenCacheCriteriaIsTimeToLive
        {
            [Test]
            public void ShouldExposeSetupForTesting()
            {
                // Arrange
                var expected = GetRandomInt();
                var callCount = 0;
                var ttl = TimeSpan.FromSeconds(GetRandomInt(10, 20));
                var sut = Create(
                    () =>
                    {
                        callCount++;
                        return expected;
                    },
                    ttl
                );
                // Act
                var result = sut.Generator();
                // Assert
                Expect(result)
                    .To.Equal(expected);
                Expect(callCount)
                    .To.Equal(1);
                Expect(sut.TimeToLive)
                    .To.Equal(ttl);
            }

            [TestFixture]
            public class WhenNoPriorCalls
            {
                [Test]
                public void ShouldGenerateValue()
                {
                    // Arrange
                    var expected = GetRandomInt();
                    var callCount = 0;
                    var sut = Create(
                        () =>
                        {
                            callCount++;
                            return expected;
                        }
                    );
                    // Act
                    var result = sut.Value;
                    // Assert
                    Expect(result)
                        .To.Equal(expected);
                    Expect(callCount)
                        .To.Equal(1);
                }
            }

            [TestFixture]
            public class WhenSecondCallWithinTimeToLive
            {
                [Test]
                public void ShouldReturnCachedValue()
                {
                    // Arrange
                    var start = GetRandomInt();
                    var expected = start;
                    var callCount = 0;
                    var sut = Create(
                        () =>
                        {
                            callCount++;
                            return start++;
                        }
                    );

                    // Act
                    var result1 = sut.Value;
                    var result2 = sut.Value;
                    // Assert
                    Expect(result1)
                        .To.Equal(expected);
                    Expect(result2)
                        .To.Equal(expected);
                    Expect(callCount)
                        .To.Equal(1);
                }
            }

            [TestFixture]
            public class WhenSecondCallAfterTimeToLive
            {
                [Test]
                public void ShouldRegenerateValue()
                {
                    // Arrange
                    var start = GetRandomInt();
                    var expected1 = start;
                    var expected2 = start + 1;
                    var callCount = 0;
                    var sut = Create(
                        () =>
                        {
                            callCount++;
                            return start++;
                        },
                        TimeSpan.FromMilliseconds(90)
                    );

                    // Act
                    var result1 = sut.Value;
                    Thread.Sleep(100);
                    var result2 = sut.Value;

                    // Assert
                    Expect(result1)
                        .To.Equal(expected1);
                    Expect(result2)
                        .To.Equal(expected2);
                    Expect(callCount)
                        .To.Equal(2);
                }
            }

            [Test]
            public void ShouldBeAbleToForceRegeneration()
            {
                // Arrange
                var start = GetRandomInt();
                var expected1 = start;
                var expected2 = start + 1;
                var callCount = 0;
                var sut = Create(
                    () =>
                    {
                        callCount++;
                        return start++;
                    }
                );

                // Act
                var result1 = sut.Value;
                sut.Invalidate();
                var result2 = sut.Value;
                // Assert
                Expect(result1)
                    .To.Equal(expected1);
                Expect(result2)
                    .To.Equal(expected2);
                Expect(callCount)
                    .To.Equal(2);
            }

            [Test]
            public void ShouldBeThreadSafe()
            {
                // Arrange
                var semaphore = new SemaphoreSlim(1, 1);
                var results = new ConcurrentBag<bool>();
                var threadCount = 64;
                var threadWaitTime = TimeSpan.FromMilliseconds(10);
                var semaphoreWaitTime = threadWaitTime.Add(TimeSpan.FromMilliseconds(10));
                var cacheTime = threadWaitTime.Add(threadWaitTime.Add(TimeSpan.FromMilliseconds(-5)));
                var sut = Create(
                    () =>
                    {
                        if (semaphore.Wait(semaphoreWaitTime))
                        {
                            Thread.Sleep(threadWaitTime);
                            semaphore.Release();
                            results.Add(true);
                        }
                        else
                        {
                            results.Add(false);
                        }

                        return results.Count;
                    },
                    cacheTime
                );
                // Act
                var threads = new List<Thread>();
                var collected = new ConcurrentBag<int>();
                var barrier = new Barrier(threadCount + 1);
                for (var i = 0; i < threadCount; i++)
                {
                    var t = new Thread(
                        () =>
                        {
                            collected.Add(sut.Value);
                            Thread.Sleep(threadWaitTime);
                            barrier.SignalAndWait();
                        }
                    );
                    t.Start();
                    threads.Add(t);
                }

                // Assert
                barrier.SignalAndWait();
                threads.JoinAll();
                Expect(results.ToArray())
                    .To.Contain.All.Matched.By(o => o);
            }

            private static SingleItemCache<T> Create<T>(
                Func<T> generator,
                TimeSpan? timeToLive = null
            )
            {
                return new(generator, timeToLive ?? TimeSpan.FromSeconds(5));
            }
        }

        [TestFixture]
        public class WhenCacheCriteriaIsFunction
        {
            [TestFixture]
            public class WhenNoPriorCalls
            {
                [Test]
                public void ShouldGenerateValue()
                {
                    // Arrange
                    var expected = GetRandomInt();
                    var callCount = 0;
                    var sut = Create(
                        () =>
                        {
                            callCount++;
                            return expected;
                        },
                        () => false
                    );
                    // Act
                    var result = sut.Value;
                    // Assert
                    Expect(result)
                        .To.Equal(expected);
                    Expect(callCount)
                        .To.Equal(1);
                }
            }

            [TestFixture]
            public class WhenPriorCalls
            {
                [TestFixture]
                public class AndCacheInvalidatorReturnsFalse
                {
                    [Test]
                    public void ShouldNotRegenerateValue()
                    {
                        // Arrange
                        var expected = GetRandomInt();
                        var callCount = 0;
                        var sut = Create(
                            () =>
                            {
                                callCount++;
                                return expected;
                            },
                            () => false
                        );
                        // Act
                        var result1 = sut.Value;
                        var result2 = sut.Value;
                        // Assert
                        Expect(result1)
                            .To.Equal(expected);
                        Expect(result2)
                            .To.Equal(expected);
                        Expect(callCount)
                            .To.Equal(1);
                    }
                }

                [TestFixture]
                public class AndCacheInvalidateReturnsTrue
                {
                    [Test]
                    public void ShouldRegenerateValue()
                    {
                        // Arrange
                        var expected1 = GetRandomInt();
                        var expected2 = GetRandomInt();
                        var queue = new Queue<int>(new[] { expected1, expected2 });
                        var expected = GetRandomInt();
                        var initial = expected;
                        var callCount = 0;
                        var sut = Create(
                            () =>
                            {
                                callCount++;
                                return queue.Dequeue();
                            },
                            () => true
                        );
                        // Act
                        var result1 = sut.Value;
                        var result2 = sut.Value;
                        // Assert
                        Expect(result1)
                            .To.Equal(expected1);
                        Expect(result2)
                            .To.Equal(expected2);
                        Expect(callCount)
                            .To.Equal(2);
                    }
                }
            }
            
            private static SingleItemCache<T> Create<T>(
                Func<T> generator,
                Func<bool> cacheInvalidator
            )
            {
                return new(
                    generator,
                    cacheInvalidator
                );
            }
        }
    }
}