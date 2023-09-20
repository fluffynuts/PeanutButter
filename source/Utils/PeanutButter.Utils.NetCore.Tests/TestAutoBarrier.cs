using NExpect;
using NUnit.Framework;
using static NExpect.Expectations;

namespace PeanutButter.Utils.NetCore.Tests;

[TestFixture]
public class TestAutoBarrier
{
    [Test]
    [Timeout(1000)]
    public void ShouldNotSignalAndWaitOnConstruction()
    {
        // Arrange
        var barrier = new Barrier(1);
        // Act
        Create(barrier);
        var signalled = barrier.SignalAndWait(TimeSpan.FromMilliseconds(10));
        // Assert
        Expect(signalled)
            .To.Be.True();
    }

    [Test]
    [Timeout(1000)]
    public async Task ShouldSignalAndWaitOnDisposal()
    {
        // Arrange
        var barrier = new Barrier(2);
        var signalled = false;
        // Act
        var t = Task.Run(() =>
        {
            Thread.Sleep(100);
            signalled = barrier.SignalAndWait(TimeSpan.FromMilliseconds(10));
        });
        using (Create(barrier))
        {
        }
        // Assert
        await t;
        Expect(signalled)
            .To.Be.True();
    }

    private static AutoBarrier Create(
        Barrier barrier
    )
    {
        return new(barrier);
    }
}