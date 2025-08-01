#if NETFX
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
#endif
using System.Collections.Concurrent;
using System.Threading.Tasks;
using static PeanutButter.Utils.PyLike;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestSlidingWindow
{
    [Test]
    [Parallelizable]
    public void ShouldBeAbleToStoreUpToWindowSize()
    {
        // Arrange
        var sut = Create<int>(3);
        // Act
        sut.Add(1);
        sut.Add(2);
        sut.Add(3);
        Expect(sut.ToArray())
            .To.Equal(
                new[]
                {
                    1,
                    2,
                    3
                }
            );
        sut.Add(4);
        // Assert
        Expect(sut.ToArray())
            .To.Equal(
                new[]
                {
                    2,
                    3,
                    4
                }
            );
    }

    [Test]
    [Parallelizable]
    public void ShouldBeAbleToClear()
    {
        // Arrange
        var sut = Create<int>(3);
        // Act
        sut.Add(1);
        sut.Add(10);
        sut.Clear();
        // Assert
        Expect((IList<int>)sut)
            .To.Be.Empty();
    }

    [Test]
    [Parallelizable]
    public void ShouldBeAbleToCopyValuesToAnotherArray()
    {
        // Arrange
        var target = new int[4];
        var sut = Create<int>(3);
        // Act
        sut.Add(3);
        sut.Add(4);
        sut.Add(5);
        sut.CopyTo(target, 1);
        // Assert
        Expect(target)
            .To.Equal(
                new[]
                {
                    0,
                    3,
                    4,
                    5
                }
            );
    }

    [Test]
    [Parallelizable]
    public void ShouldBeAbleToRemoveAValue()
    {
        // Arrange
        var sut = Create<int>(5);
        // Act
        sut.Add(1);
        sut.Add(2);
        sut.Add(1);
        var result = sut.Remove(1);
        // Assert
        Expect(result)
            .To.Be.True();
        Expect((IList<int>)sut)
            .To.Equal(
                new[]
                {
                    2,
                    1
                }
            );
    }

    [Test]
    [Parallelizable]
    public void ShouldBeAbleToRemoveValuesUntilNoneLeftAndReturnsFalse()
    {
        // Arrange
        var sut = Create<int>(5);
        // Act
        sut.Add(1);
        sut.Add(2);
        sut.Add(1);
        var result1 = sut.Remove(1);
        var result2 = sut.Remove(1);
        var result3 = sut.Remove(1);
        // Assert
        Expect(result1)
            .To.Be.True();
        Expect(result2)
            .To.Be.True();
        Expect(result3)
            .To.Be.False();
        Expect((IList<int>)sut)
            .To.Equal(
                new[]
                {
                    2
                }
            );
    }

    [Test]
    [Parallelizable]
    public void ShouldReturnTheFirstIndexOfAValue()
    {
        // Arrange
        var sut = Create<int>(3);
        // Act
        sut.Add(5);
        sut.Add(10);
        sut.Add(5);
        var result1 = sut.IndexOf(5);
        var result2 = sut.IndexOf(10);
        var result3 = sut.IndexOf(13);
        // Assert
        Expect(result1)
            .To.Equal(0);
        Expect(result2)
            .To.Equal(1);
        Expect(result3)
            .To.Equal(-1);
    }

    [TestFixture]
    [Parallelizable]
    public class Insert
    {
        [Test]
        [Parallelizable]
        public void ShouldBeAbleToInsertAtTheProvidedIndex()
        {
            // Arrange
            var sut = Create<int>(3);
            // Act
            sut.Add(1);
            sut.Add(2);
            sut.Add(3);
            sut.Insert(1, 10);
            // Assert
            Expect((IList<int>)sut)
                .To.Equal(
                    new[]
                    {
                        10,
                        2,
                        3
                    },
                    () => "Trimming after insertion should have chucked out the 1"
                );
        }

        [Test]
        [Parallelizable]
        public void InsertedItemsShouldHaveTheirCreatedInterpolated()
        {
            // Arrange
            var sut = Create<int>(3);
            // Act
            sut.Add(1);
            Thread.Sleep(50);
            sut.Add(1);
            Thread.Sleep(50);
            sut.Add(1);
            Thread.Sleep(50);
            sut.Insert(2, 2);
            // Assert
            Expect((IList<int>)sut)
                .To.Equal(
                    new[]
                    {
                        1,
                        2,
                        1
                    }
                );
            var before = sut.ItemAt(0);
            var inserted = sut.ItemAt(1);
            var after = sut.ItemAt(2);
            Expect(inserted.Created)
                .To.Be.Greater.Than(before.Created)
                .And
                .To.Be.Less.Than(after.Created);
        }

        [Test]
        [Parallelizable]
        public void ItemInsertedAtTheStartShouldGetStartingDateTime()
        {
            // Arrange
            var sut = Create<int>(3);
            // Act
            sut.Add(1);
            Thread.Sleep(50);
            sut.Add(1);
            Thread.Sleep(50);
            sut.Insert(0, 2);
            // Assert
            Expect((IList<int>)sut)
                .To.Equal(
                    new[]
                    {
                        2,
                        1,
                        1
                    }
                );
            var inserted = sut.ItemAt(0);
            var after = sut.ItemAt(1);
            Expect(inserted.Created)
                .To.Equal(after.Created);
        }

        [Test]
        [Parallelizable]
        public void ItemInsertedAtTheEndShouldGetCurrentDateTime()
        {
            // Arrange
            var sut = Create<int>(3);
            // Act
            sut.Add(1);
            Thread.Sleep(50);
            sut.Add(1);
            Thread.Sleep(50);
            var before = DateTime.Now;
            Thread.Sleep(50);
            sut.Insert(2, 2);
            Thread.Sleep(50);
            var after = DateTime.Now;
            // Assert
            Expect((IList<int>)sut)
                .To.Equal(
                    new[]
                    {
                        1,
                        1,
                        2
                    }
                );
            var inserted = sut.ItemAt(2);
            Expect(inserted.Created)
                .To.Be.Greater.Than(before)
                .And
                .To.Be.Less.Than(after);
        }
    }

    [Test]
    [Parallelizable]
    public void ShouldBeAbleToSetTimeBasedWindow()
    {
        // Arrange
        var sut = Create<int>(TimeSpan.FromSeconds(1));
        // Act
        sut.Add(1);
        Thread.Sleep(550);
        sut.Add(2);
        Thread.Sleep(550);
        sut.Add(3);
        // Assert
        Expect((IList<int>)sut)
            .To.Equal(
                new[]
                {
                    2,
                    3
                }
            );
    }

    [Test]
    [Parallelizable]
    public void ShouldBeAbleToRemoveItemAtIndex()
    {
        // Arrange
        var sut = Create<int>(3);
        // Act
        sut.Add(1);
        sut.Add(2);
        sut.Add(3);
        sut.RemoveAt(1);
        // Assert
        Expect((IList<int>)sut)
            .To.Equal(
                new[]
                {
                    1,
                    3
                }
            );
    }

    [Test]
    [Parallelizable]
    public void ShouldBeAbleToIndexIntoListForRead()
    {
        // Arrange
        var sut = Create<int>(3);
        // Act
        sut.Add(0);
        sut.Add(2);
        sut.Add(4);
        var result = sut[1];
        // Assert
        Expect(result)
            .To.Equal(2);
    }

    [Test]
    [Parallelizable]
    public void ShouldBeAbleToSetTheValueAtAnIndex()
    {
        // Arrange
        var sut = Create<int>(3);
        var expected = 123;
        // Act
        sut.Add(10);
        sut.Add(20);
        sut.Add(30);
        var originalItem = sut.ItemAt(1);
        var originalCreated = originalItem.Created;
        sut[1] = expected;
        // Assert
        Expect(sut[1])
            .To.Equal(expected);
        var currentItem = sut.ItemAt(1);
        Expect(currentItem.Value)
            .To.Equal(expected);
        Expect(currentItem)
            .To.Be(originalItem);
        Expect(currentItem.Created)
            .To.Equal(originalCreated);
    }

    [TestFixture]
    [Parallelizable]
    public class Rates
    {
        [Test]
        [Parallelizable]
        public void IntsShouldBeAbleToQueryCurrentRateInItemsPerSecond()
        {
            // Arrange
            var sut = Create<int>(TimeSpan.FromSeconds(10));
            // Act
            sut.Add(3);
            Thread.Sleep(1000);
            sut.Add(4);
            Thread.Sleep(1000);
            sut.Add(5);
            Thread.Sleep(1000);
            var result = sut.CurrentRate();
            // Assert
            Expect(Math.Round(result))
                .To.Equal(4);
        }

        [Test]
        [Parallelizable]
        public void LongsShouldBeAbleToQueryCurrentRateInItemsPerSecond()
        {
            // Arrange
            var sut = Create<long>(TimeSpan.FromSeconds(10));
            // Act
            sut.Add(3);
            Thread.Sleep(1000);
            sut.Add(4);
            Thread.Sleep(1000);
            sut.Add(5);
            Thread.Sleep(1000);
            var result = sut.CurrentRate();
            // Assert
            Expect(Math.Round(result))
                .To.Equal(4);
        }

        [Test]
        [Parallelizable]
        public void DecimalsShouldBeAbleToQueryCurrentRateInItemsPerSecond()
        {
            // Arrange
            var sut = Create<decimal>(TimeSpan.FromSeconds(10));
            // Act
            sut.Add(3);
            Thread.Sleep(1000);
            sut.Add(4);
            Thread.Sleep(1000);
            sut.Add(5);
            Thread.Sleep(1000);
            var result = sut.CurrentRate();
            // Assert
            Expect(Math.Round(result))
                .To.Equal(4);
        }

        [Test]
        [Parallelizable]
        public void DoublesShouldBeAbleToQueryCurrentRateInItemsPerSecond()
        {
            // Arrange
            var sut = Create<double>(TimeSpan.FromSeconds(10));
            // Act
            sut.Add(3);
            Thread.Sleep(1000);
            sut.Add(4);
            Thread.Sleep(1000);
            sut.Add(5);
            Thread.Sleep(1000);
            var result = sut.CurrentRate();
            // Assert
            Expect(Math.Round(result))
                .To.Equal(4);
        }
    }

    [TestFixture]
    [Parallelizable]
    public class EstimatedTimeRemaining
    {
        [Test]
        [Parallelizable]
        public void IntsShouldBeAbleToQueryCurrentRateInItemsPerSecond()
        {
            // Arrange
            var sut = Create<int>(TimeSpan.FromSeconds(10));
            // Act
            sut.Add(3);
            Thread.Sleep(1000);
            sut.Add(4);
            Thread.Sleep(1000);
            sut.Add(5);
            Thread.Sleep(1000);
            var result = sut.EstimatedTimeRemaining(sut.Sum());
            // Assert
            Expect(Math.Round(result.TotalSeconds))
                .To.Equal(3);
        }

        [Test]
        [Parallelizable]
        public void LongsShouldBeAbleToQueryCurrentRateInItemsPerSecond()
        {
            // Arrange
            var sut = Create<long>(TimeSpan.FromSeconds(10));
            // Act
            sut.Add(3);
            Thread.Sleep(1000);
            sut.Add(4);
            Thread.Sleep(1000);
            sut.Add(5);
            Thread.Sleep(1000);
            var result = sut.EstimatedTimeRemaining(sut.Sum());
            // Assert
            Expect(Math.Round(result.TotalSeconds))
                .To.Equal(3);
        }

        [Test]
        [Parallelizable]
        public void DecimalsShouldBeAbleToQueryCurrentRateInItemsPerSecond()
        {
            // Arrange
            var sut = Create<decimal>(TimeSpan.FromSeconds(10));
            // Act
            sut.Add(3);
            Thread.Sleep(1000);
            sut.Add(4);
            Thread.Sleep(1000);
            sut.Add(5);
            Thread.Sleep(1000);
            var result = sut.EstimatedTimeRemaining(sut.Sum());
            // Assert
            Expect(Math.Round(result.TotalSeconds))
                .To.Equal(3);
        }

        [Test]
        [Parallelizable]
        public void DoublesShouldBeAbleToQueryCurrentRateInItemsPerSecond()
        {
            // Arrange
            var sut = Create<double>(TimeSpan.FromSeconds(10));
            // Act
            sut.Add(3);
            Thread.Sleep(1000);
            sut.Add(4);
            Thread.Sleep(1000);
            sut.Add(5);
            Thread.Sleep(1000);
            var result = sut.EstimatedTimeRemaining(sut.Sum());
            // Assert
            Expect(Math.Round(result.TotalSeconds))
                .To.Equal(3);
        }

        [Test]
        [Parallelizable]
        public void IntsShouldBeAbleToQueryCurrentRateInItemsPerEmpty()
        {
            // Arrange
            var sut = Create<int>(TimeSpan.FromSeconds(10));
            // Act
            var result = sut.EstimatedTimeRemaining(sut.Sum());
            // Assert
            Expect(result)
                .To.Equal(TimeSpan.MaxValue);
        }

        [Test]
        [Parallelizable]
        public void LongsShouldBeAbleToQueryCurrentRateInItemsPerEmpty()
        {
            // Arrange
            var sut = Create<long>(TimeSpan.FromSeconds(10));
            // Act
            var result = sut.EstimatedTimeRemaining(sut.Sum());
            // Assert
            Expect(result)
                .To.Equal(TimeSpan.MaxValue);
        }

        [Test]
        [Parallelizable]
        public void DecimalsShouldBeAbleToQueryCurrentRateInItemsPerEmpty()
        {
            // Arrange
            var sut = Create<decimal>(TimeSpan.FromSeconds(10));
            // Act
            var result = sut.EstimatedTimeRemaining(sut.Sum());
            // Assert
            Expect(result)
                .To.Equal(TimeSpan.MaxValue);
        }

        [Test]
        [Parallelizable]
        public void DoublesShouldBeAbleToQueryCurrentRateInItemsPerEmpty()
        {
            // Arrange
            var sut = Create<double>(TimeSpan.FromSeconds(10));
            // Act
            var result = sut.EstimatedTimeRemaining(sut.Sum());
            // Assert
            Expect(result)
                .To.Equal(TimeSpan.MaxValue);
        }
    }

    [TestFixture]
    public class LiveIssues
    {
        [Test]
        public void ShouldBeAbleToStoreNullAndTestForIt()
        {
            // Arrange
            var sut = Create<string>(10);
            // Act
            sut.Add(null);
            var result = sut.Contains(null);
            // Assert
            Expect(result)
                .To.Be.True();
        }

        [Test]
        public void ShouldHandleConcurrency()
        {
            // Arrange
            var threadCount = 100;
            var sut = Create<string>(10);
            var collected = new ConcurrentBag<bool>();
            var errors = new ConcurrentBag<Exception>();
            var startBarrier = new Barrier(threadCount);
            var endBarrier = new Barrier(threadCount + 1);
            // Act

            var threads = Range(0, threadCount)
                .Select(
                    _ =>
                    {
                        var result = new Thread(
                            () =>
                            {
                                try
                                {
                                    startBarrier.SignalAndWait();
                                    var value = GetRandomString();
                                    sut.Add(value);
                                    collected.Add(sut.Contains(value));
                                    endBarrier.SignalAndWait();
                                }
                                catch (Exception ex)
                                {
                                    errors.Add(ex);
                                }
                            }
                        );
                        result.Start();
                        return result;
                    }
                );
            foreach (var t in threads)
            {
                Task.Run(() => t.Join());
            }

            endBarrier.SignalAndWait();
            // Assert
            Expect(collected)
                .To.Contain.Only(threadCount).Items();
            Expect(errors)
                .To.Be.Empty();
        }
    }

    private static ISlidingWindow<T> Create<T>(
        int maxItems
    )
    {
        return new SlidingWindow<T>(maxItems);
    }

    private static ISlidingWindow<T> Create<T>(
        TimeSpan ttl
    )
    {
        return new SlidingWindow<T>(ttl);
    }
}