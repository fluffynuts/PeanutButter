using System;
using System.Threading;
using NUnit.Framework;
using NExpect;
using static NExpect.Expectations;
using static PeanutButter.RandomGenerators.RandomValueGen;

namespace PeanutButter.Utils.NetCore.Tests
{
    [TestFixture]
    public class TestSingleItemCache
    {
        [Test]
        public void ShouldExposeSetupForTesting()
        {
            // Arrange
            var expected = GetRandomInt();
            var callCount = 0;
            var ttl = TimeSpan.FromSeconds(GetRandomInt(10, 20));
            var sut = Create(() =>
            {
                callCount++;
                return expected;
            }, ttl);
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
                var sut = Create(() =>
                {
                    callCount++;
                    return expected;
                });
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
                var sut = Create(() =>
                {
                    callCount++;
                    return start++;
                });

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
                var sut = Create(() =>
                {
                    callCount++;
                    return start++;
                }, TimeSpan.FromMilliseconds(90));

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
            var sut = Create(() =>
            {
                callCount++;
                return start++;
            });

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

        private static SingleItemCache<T> Create<T>(
            Func<T> generator,
            TimeSpan? timeToLive = null
        )
        {
            return new(generator, timeToLive ?? TimeSpan.FromSeconds(5));
        }
    }
}