using System.Collections.Concurrent;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestPool
    {
        [Test]
        [Parallelizable]
        public void ShouldProvideTheItemFromTheProvidedFactory()
        {
            // Arrange
            using var sut = Create(() => new Service());
            // Act
            using var result = sut.Borrow();
            // Assert
            Expect(result)
                .Not.To.Be.Null();
            Expect(result.Instance)
                .To.Be.An.Instance.Of<Service>();
        }

        [Test]
        [Parallelizable]
        public void ShouldReuseTheSameInstanceWhenItBecomesAvailableAgain()
        {
            // Arrange
            using var sut = Create(() => new Service());

            // Act
            using (var _ = sut.Borrow())
            {
            }

            using (var _ = sut.Borrow())
            {
            }

            // Assert
            Expect(sut.Count)
                .To.Equal(1);
        }

        [Test]
        [Parallelizable]
        public void ShouldDisposeItemsOnPoolDispose()
        {
            // Arrange
            Service provided;
            using (var sut = Create(() => new Service()))
            {
                // Act
                using var result = sut.Borrow();
                // Assert
                Expect(result)
                    .Not.To.Be.Null();
                Expect(result.Instance)
                    .Not.To.Be.Null();
                Expect(result.Instance)
                    .To.Be.An.Instance.Of<Service>();
                provided = result.Instance;
            }

            Expect(provided.IsDisposed)
                .To.Be.True();
        }

        [Test]
        public void ShouldRunTheProvidedOnReleaseWhenReleasing()
        {
            // Arrange
            using var sut = Create(
                () => new Service(),
                onRelease: s => s.UseCount++
            );
            // Act
            using (var _ = sut.Borrow())
            {
            }
            using var result = sut.Borrow();
            // Assert
            Expect(result.Instance.UseCount)
                .To.Equal(1);
        }

        [Test]
        [Parallelizable]
        public void ShouldOnlyCreateProvidedMaxAndWaitForInstanceOnExceedingMax()
        {
            // Arrange
            using var sut = Create(() => new Service(), 2);
            // Act
            sut.Borrow();
            var result2 = sut.Borrow();
            Expect(sut.Count)
                .To.Equal(2);
            var t = new Thread(() =>
            {
                Thread.Sleep(1000);
                result2.Dispose();
            });
            t.Start();
            var result3 = sut.Borrow(1500);
            // Assert
            Expect(sut.Count)
                .To.Equal(2);
            Expect(result3)
                .To.Be(result2);
        }

        [Test]
        [Parallelizable]
        public void ShouldThrowWhenNoItemAvailableInTime()
        {
            // Arrange
            using var sut = Create(() => new Service(), 1);
            // Act
            sut.Borrow();
            Expect(() => sut.Borrow())
                .To.Throw<NoPooledItemAvailableException>();
            // Assert
        }

        [Test]
        [Parallelizable]
        public void StressTestShouldNotExceedMaxCount()
        {
            // Arrange
            using var sut = Create(() => new Service(), 2);
            var threads = new List<Thread>();
            var captured = new ConcurrentBag<IPoolItem<Service>>();
            var errored = 0;
            var capturedTimes = new ConcurrentBag<TimeSpan>();
            var threadCount = 32;
            var requestCount = 32;
            var itemWait = 1000;
            for (var i = 0; i < threadCount; i++)
            {
                var t = new Thread(() =>
                {
                    IPoolItem<Service> item = null;
                    for (var j = 0; j < requestCount; j++)
                    {
                        try
                        {
                            capturedTimes.Add(
                                Benchmark.Time(() => item = sut.Borrow(itemWait))
                            );
                            captured.Add(
                                item
                            );
                            Thread.Sleep(10);
                            item.Dispose();
                        }
                        catch (NoPooledItemAvailableException)
                        {
                            errored++;
                        }
                    }
                });
                t.Start();
                threads.Add(t);
            }

            // Act
            foreach (var t in threads)
            {
                t.Join();
            }

            // Assert
            Expect(captured.ToArray())
                .To.Contain.All
                .Matched.By(o => o != null);
            Expect(sut.Count)
                .To.Equal(2);
            Expect(errored)
                .To.Equal(0);
        }

        private static Pool<T> Create<T>(
            Func<T> factory,
            int? maxItems = null,
            Action<T> onRelease = null
        )
        {
            if (maxItems is null)
            {
                return new Pool<T>(factory, onRelease);
            }

            return new Pool<T>(factory, onRelease, maxItems.Value);
        }

        public interface IService : IDisposable
        {
        }

        public class Service : IService
        {
            public bool IsDisposed { get; private set; }
            public int UseCount { get; set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}