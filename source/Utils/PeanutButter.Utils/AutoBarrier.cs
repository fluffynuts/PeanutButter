using System;
using System.Threading;

namespace PeanutButter.Utils;

/// <summary>
/// Provides an auto-signaller around a barrier:
/// when this is disposed, it will SignalAndWait
/// on the barrier, meaning you don't need to put
/// required barrier unlocks in a finally clause
/// </summary>
public class AutoBarrier: IDisposable
{
    private Barrier _barrier;

    /// <summary>
    /// Constructs the wrapper around the barrier
    /// </summary>
    /// <param name="barrier"></param>
    public AutoBarrier(Barrier barrier)
    {
        _barrier = barrier;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _barrier.SignalAndWait();
        _barrier = null;
    }
}