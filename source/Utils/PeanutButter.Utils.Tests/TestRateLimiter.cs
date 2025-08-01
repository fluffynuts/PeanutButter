using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestRateLimiter
{
    [TestFixture]
    public class SynchronousOperations
    {
        [Test]
        public void ShouldRunTheProvidedAction()
        {
            // Arrange
            var called = false;
            var sut = Create(GetRandomInt(1));
            // Act
            sut.Run(() => called = true);
            // Assert
            Expect(called)
                .To.Be.True();
        }

        [Test]
        public void ShouldNotExceedCallMaximumForImmediateCalls()
        {
            // Arrange
            var count = 0;
            var sut = Create(2);

            // Act
            sut.Run(() => count++);
            sut.Run(() => count++);
            sut.Run(() => count++);
            // Assert
            Expect(count)
                .To.Equal(2);
        }

        [Test]
        public void ShouldRunNewActionsOnceTheOldOnesHaveExpired()
        {
            // Arrange
            var collected = new List<DateTime>();
            var sut = Create(2, TimeSpan.FromSeconds(1));

            // Act
            var runUntil = DateTime.Now.AddSeconds(2);
            while (DateTime.Now < runUntil)
            {
                sut.Run(
                    () => collected.Add(DateTime.Now)
                );
                Thread.Sleep(250);
            }

            // Assert
            Expect(collected)
                .To.Contain.Only(4)
                .Items();
        }
    }

    [TestFixture]
    public class AsynchronousOperations
    {
        [Test]
        public async Task ShouldRunTheProvidedAction()
        {
            // Arrange
            var called = false;
            var sut = Create(GetRandomInt(1));
            // Act
            await sut.RunAsync(
                async () =>
                {
                    await Task.Delay(1);
                    called = true;
                }
            );
            // Assert
            Expect(called)
                .To.Be.True();
        }

        [Test]
        public async Task ShouldNotExceedCallMaximumForImmediateCalls()
        {
            // Arrange
            var count = 0;
            var sut = Create(2);

            // Act
            await sut.RunAsync(Increment);
            await sut.RunAsync(Increment);
            await sut.RunAsync(Increment);
            // Assert
            Expect(count)
                .To.Equal(2);

            async Task Increment()
            {
                await Task.Delay(0);
                count++;
            }
        }

        [Test]
        public async Task ShouldRunNewActionsOnceTheOldOnesHaveExpired()
        {
            // Arrange
            var collected = new List<DateTime>();
            var sut = Create(2, TimeSpan.FromSeconds(1));

            // Act
            var runUntil = DateTime.Now.AddSeconds(2);
            while (DateTime.Now < runUntil)
            {
                await sut.RunAsync(CollectRecord);
                Thread.Sleep(250);
            }

            // Assert
            Expect(collected)
                .To.Contain.Only(4)
                .Items();

            async Task CollectRecord()
            {
                await Task.Delay(0);
                collected.Add(DateTime.Now);
            }
        }
    }

    private static IRateLimiter Create(
        int maxCalls,
        TimeSpan? period = null
    )
    {
        period ??= TimeSpan.FromMinutes(1);
        return new RateLimiter(
            maxCalls,
            period.Value
        );
    }
}