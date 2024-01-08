using System;
using System.Linq;
using System.Threading;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static PeanutButter.Utils.PyLike;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestParallelWorker
    {
        [TestFixture]
        public class StaticConvenienceMethods
        {
            [TestFixture]
            public class RunAll
            {
                [TestFixture]
                public class RunningActions
                {
                    [Test]
                    public void ShouldRunTwoActionsInParallel()
                    {
                        // Arrange
                        var lockObject = new object();
                        var count = 0;
                        var thread1Saw = 0;
                        var thread2Saw = 0;
                        var expected = 2;
                        var barrier = new Barrier(expected);
                        // Act
                        Run.InParallel(
                            () =>
                            {
                                lock (lockObject)
                                {
                                    count++;
                                }

                                Thread.Sleep(50);
                                barrier.SignalAndWait();
                                thread1Saw = count;
                            },
                            () =>
                            {
                                lock (lockObject)
                                {
                                    count++;
                                }

                                Thread.Sleep(100);
                                barrier.SignalAndWait();
                                thread2Saw = count;
                            }
                        );
                        // Assert
                        Expect(count)
                            .To.Equal(expected);
                        Expect(thread1Saw)
                            .To.Equal(expected);
                        Expect(thread2Saw)
                            .To.Equal(expected);
                    }

                    [Test]
                    public void ShouldRunThreeActionsInParallel()
                    {
                        // Arrange
                        var lockObject = new object();
                        var count = 0;
                        var thread1Saw = 0;
                        var thread2Saw = 0;
                        var thread3Saw = 0;
                        var expected = 3;
                        var barrier = new Barrier(expected);
                        // Act
                        Run.InParallel(
                            () =>
                            {
                                lock (lockObject)
                                {
                                    count++;
                                }

                                Thread.Sleep(50);
                                barrier.SignalAndWait();
                                thread1Saw = count;
                            },
                            () =>
                            {
                                lock (lockObject)
                                {
                                    count++;
                                }

                                Thread.Sleep(100);
                                barrier.SignalAndWait();
                                thread2Saw = count;
                            },
                            () =>
                            {
                                lock (lockObject)
                                {
                                    count++;
                                }

                                Thread.Sleep(100);
                                barrier.SignalAndWait();
                                thread3Saw = count;
                            }
                        );
                        // Assert
                        Expect(count)
                            .To.Equal(expected);
                        Expect(thread1Saw)
                            .To.Equal(expected);
                        Expect(thread2Saw)
                            .To.Equal(expected);
                        Expect(thread3Saw)
                            .To.Equal(expected);
                    }

                    [Test]
                    public void ShouldRespectMaxDegreeOfParallelismWhenProvided()
                    {
                        // Arrange
                        var lockObject = new object();
                        var concurrent = 0;
                        var maxConcurrent = 0;
                        // Act
                        Run.InParallel(
                            1,
                            Once,
                            Once,
                            Once,
                            Once,
                            Once,
                            Once,
                            Once,
                            Once,
                            Once,
                            Once,
                            Once
                        );
                        // Assert
                        Expect(maxConcurrent)
                            .To.Equal(1);

                        void Once()
                        {
                            lock (lockObject)
                            {
                                concurrent++;
                                maxConcurrent = Math.Max(concurrent, maxConcurrent);
                            }

                            Thread.Sleep(GetRandomInt(25, 100));
                            lock (lockObject)
                            {
                                concurrent--;
                                maxConcurrent = Math.Max(concurrent, maxConcurrent);
                            }
                        }
                    }
                }

                [TestFixture]
                public class RunningFuncs
                {
                    [Test]
                    public void ShouldRunTwoActionsInParallel()
                    {
                        // Arrange
                        var lockObject = new object();
                        var count = 0;
                        var thread1Saw = 0;
                        var thread2Saw = 0;
                        var expected = 2;
                        var barrier = new Barrier(expected);
                        // Act
                        var result = Run.InParallel(
                            () =>
                            {
                                lock (lockObject)
                                {
                                    count++;
                                }

                                Thread.Sleep(50);
                                barrier.SignalAndWait();
                                thread1Saw = count;
                                return 1;
                            },
                            () =>
                            {
                                lock (lockObject)
                                {
                                    count++;
                                }

                                Thread.Sleep(100);
                                barrier.SignalAndWait();
                                thread2Saw = count;
                                return 2;
                            }
                        );
                        // Assert
                        Expect(count)
                            .To.Equal(expected);
                        Expect(thread1Saw)
                            .To.Equal(expected);
                        Expect(thread2Saw)
                            .To.Equal(expected);
                        var results = result.Select(o => o.Result).ToArray();
                        Expect(results)
                            .To.Be.Equivalent.To(new[] { 1, 2 });
                    }

                    [Test]
                    public void ShouldRunThreeActionsInParallel()
                    {
                        // Arrange
                        var lockObject = new object();
                        var count = 0;
                        var thread1Saw = 0;
                        var thread2Saw = 0;
                        var thread3Saw = 0;
                        var expected = 3;
                        var barrier = new Barrier(expected);
                        // Act
                        var result = Run.InParallel(
                            () =>
                            {
                                lock (lockObject)
                                {
                                    count++;
                                }

                                Thread.Sleep(50);
                                barrier.SignalAndWait();
                                thread1Saw = count;
                                return 1;
                            },
                            () =>
                            {
                                lock (lockObject)
                                {
                                    count++;
                                }

                                Thread.Sleep(100);
                                barrier.SignalAndWait();
                                thread2Saw = count;
                                return 2;
                            },
                            () =>
                            {
                                lock (lockObject)
                                {
                                    count++;
                                }

                                Thread.Sleep(100);
                                barrier.SignalAndWait();
                                thread3Saw = count;
                                return 3;
                            }
                        );
                        // Assert
                        Expect(result.Results())
                            .To.Be.Equivalent.To(new[] { 1, 2, 3 });
                        Expect(count)
                            .To.Equal(expected);
                        Expect(thread1Saw)
                            .To.Equal(expected);
                        Expect(thread2Saw)
                            .To.Equal(expected);
                        Expect(thread3Saw)
                            .To.Equal(expected);
                    }

                    [Test]
                    public void ShouldRespectMaxDegreeOfParallelismWhenProvided()
                    {
                        // Arrange
                        var lockObject = new object();
                        var concurrent = 0;
                        var maxConcurrent = 0;
                        var counter = new Counter();
                        // Act
                        var result = Run.InParallel(
                            1,
                            Once,
                            Once,
                            Once,
                            Once,
                            Once,
                            Once,
                            Once,
                            Once,
                            Once,
                            Once,
                            Once
                        );
                        // Assert
                        var expected = Range(0, counter).Select(i => i + 1).ToArray();
                        Expect(result.Results())
                            .To.Be.Equivalent.To(expected);
                        Expect(maxConcurrent)
                            .To.Equal(1);

                        int Once()
                        {
                            lock (lockObject)
                            {
                                concurrent++;
                                maxConcurrent = Math.Max(concurrent, maxConcurrent);
                            }

                            Thread.Sleep(GetRandomInt(25, 100));
                            lock (lockObject)
                            {
                                concurrent--;
                                maxConcurrent = Math.Max(concurrent, maxConcurrent);
                                return ++counter;
                            }
                        }
                    }

                    private class Counter
                    {
                        public int Value { get; private set; }

                        public Counter() : this(0)
                        {
                        }

                        public Counter(int value)
                        {
                            Value = value;
                        }

                        public static Counter operator ++(Counter a)
                        {
                            return new Counter(a.Value + 1);
                        }

                        public static implicit operator int(Counter a)
                        {
                            return a.Value;
                        }
                    }
                }
            }
        }

        [TestFixture]
        public class RunnerEdgeCases
        {
            [Test]
            public void ShouldBeAbleToAddWorkWhilstWorking()
            {
                // Arrange
                var runner = Create<int>();
                runner.AddWorker(() =>
                {
                    runner.AddWorker(() => 2);
                    return 1;
                });
                // Act
                var result = runner.RunAll();
                // Assert
                Expect(result.Results())
                    .To.Be.Equivalent.To(new[] { 1, 2 });
            }
        }

        private static ParallelWorker Create()
        {
            return new();
        }

        private static ParallelWorker<T> Create<T>()
        {
            return new();
        }
    }
}