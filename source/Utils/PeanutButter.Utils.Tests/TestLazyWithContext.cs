using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;
using static PeanutButter.Utils.PyLike;

namespace PeanutButter.Utils.Tests
{
    [TestFixture]
    public class TestLazyWithContext
    {
        [TestFixture]
        public class SyncResolution
        {
            [Test]
            public void ShouldProvideTheValueFromTheSynchronousMethod()
            {
                // Arrange
                var expected = GetRandomInt();
                var host = Create(expected);
                // Pre-assert
                // Act
                var result = host.SyncValue.Value;
                // Assert
                Expect(result).To.Equal(expected);
            }

            [Test]
            public void ShouldOnlyResolveSyncOnce()
            {
                // Arrange
                var expected = GetRandomInt();
                var host = Create(expected);
                // Pre-assert
                // Act
                var value1 = host.SyncValue.Value;
                var value2 = host.SyncValue.Value;
                var result = host.ResolveCalls;
                // Assert
                Expect(value1).To.Equal(expected);
                Expect(value2).To.Equal(expected);
                Expect(result).To.Equal(1);
            }
            
            [Test]
            public void ShouldOnlyResolveOnceIfHammered()
            {
                // Arrange
                var threadCount = GetRandomInt(1024, 2048);
                var results = new ConcurrentBag<int>();
                var expected = GetRandomInt();
                var host = Create(expected);
                var threads = Range(0, threadCount).Select(i =>
                {
                    var thread = new Thread(() => results.Add(host.SyncValue.Value));
                    thread.Start(); // this will only start once the Select is actualized
                    return thread;
                });
                // Pre-assert
                // Act
                threads.ToArray().ForEach(t => t.Join()); // actualize!
                // Assert
                Expect(results.Count).To.Equal(threadCount);
                Expect(results.All(o => o == expected))
                    .To.Be.True();
                Expect(host.ResolveCalls).To.Equal(1);
            }
        }

        [TestFixture]
        public class AsyncResolution
        {
            [Test]
            public void ShouldProvideTheValueFromTheSynchronousMethod()
            {
                // Arrange
                var expected = GetRandomInt();
                var host = Create(expected);
                // Pre-assert
                // Act
                var result = host.AsyncValue.Value;
                // Assert
                Expect(result).To.Equal(expected);
            }

            [Test]
            public void ShouldOnlyResolveSyncOnce()
            {
                // Arrange
                var expected = GetRandomInt();
                var host = Create(expected);
                // Pre-assert
                // Act
                var value1 = host.AsyncValue.Value;
                var value2 = host.AsyncValue.Value;
                var result = host.ResolveCalls;
                // Assert
                Expect(value1).To.Equal(expected);
                Expect(value2).To.Equal(expected);
                Expect(result).To.Equal(1);
            }

            [Test]
            public void ShouldOnlyResolveOnceIfHammered()
            {
                // Arrange
                var threadCount = GetRandomInt(1024, 2048);
                var results = new ConcurrentBag<int>();
                var expected = GetRandomInt();
                var host = Create(expected);
                var threads = Range(0, threadCount).Select(i =>
                {
                    var thread = new Thread(() => results.Add(host.AsyncValue.Value));
                    thread.Start(); // this will only start once the Select is actualized
                    return thread;
                });
                // Pre-assert
                // Act
                threads.ToArray().ForEach(t => t.Join()); // actualize!
                // Assert
                Expect(results.Count).To.Equal(threadCount);
                Expect(results.All(o => o == expected))
                    .To.Be.True();
                Expect(host.ResolveCalls).To.Equal(1);
            }
        }

        public class LazyHost
        {
            public LazyWithContext<LazyHost, int> SyncValue { get; }
            public LazyWithContext<LazyHost, int> AsyncValue { get; }
            public int ResolveCalls { get; private set; }

            private async Task<int> ResolveValueAsync()
            {
                return await Task.FromResult(ResolveValue());
            }

            private int ResolveValue()
            {
                ResolveCalls++;
                return _value;
            }

            private readonly int _value;

            public LazyHost(
                int value)
            {
                // can't use 'this' in a property initializer...
                SyncValue = new LazyWithContext<LazyHost, int>(
                    this,
                    host => host.ResolveValue()
                );
                AsyncValue = new LazyWithContext<LazyHost, int>(
                    this,
                    host => host.ResolveValueAsync()
                );
                _value = value;
            }
        }


        private static LazyHost Create(
            int expected)
        {
            return new LazyHost(expected);
        }
        
    }
}