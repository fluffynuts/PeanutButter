using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
#else
namespace PeanutButter.TestUtils.AspNetCore.Fakes;
#endif

/// <summary>
/// Provides a fake session
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class FakeSession : ISession, IFake
{
    private readonly Dictionary<string, byte[]> _store = new();

    /// <inheritdoc />
    public Task LoadAsync(
        CancellationToken cancellationToken = new()
    )
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task CommitAsync(
        CancellationToken cancellationToken = new()
    )
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public bool TryGetValue(string key, out byte[] value)
    {
        return _store.TryGetValue(key, out value);
    }

    /// <inheritdoc />
    public void Set(string key, byte[] value)
    {
        _store[key] = value;
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        _store.Remove(key);
    }

    /// <inheritdoc />
    public void Clear()
    {
        _store.Clear();
    }

    /// <inheritdoc />
    public bool IsAvailable { get; } = true;

    /// <inheritdoc />
    public string Id { get; } = Guid.NewGuid().ToString();

    /// <inheritdoc />
    public IEnumerable<string> Keys => _store.Keys;
}