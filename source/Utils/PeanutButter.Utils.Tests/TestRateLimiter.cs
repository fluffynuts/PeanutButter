using System;
using System.Collections.Generic;
using System.Threading;

namespace PeanutButter.Utils.Tests;

[TestFixture]
public class TestRateLimiter
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