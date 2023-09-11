using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace
#if BUILD_PEANUTBUTTER_INTERNAL
    Imported.PeanutButter.Utils;
#else
    PeanutButter.Utils;
#endif
/// <summary>
/// Provides a generic pooling mechanism using the Disposable pattern
/// </summary>
/// <typeparam name="T"></typeparam>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class Pool<T> : IDisposable
{
    /// <summary>
    /// The maximum number of items to hold in the pool
    /// </summary>
    public int MaxItems { get; }

    /// <summary>
    /// How many items are currently in the pool
    /// </summary>
    public int Count => _items.Count;

    private readonly ConcurrentBag<PoolItem<T>> _items = new();
    private readonly Func<T> _factory;
    private readonly Action<T> _onRelease;

    /// <summary>
    /// Creates the pool with a factory for the items
    /// </summary>
    /// <param name="factory"></param>
    /// <exception cref="NotImplementedException"></exception>
    public Pool(
        Func<T> factory
    ) : this(factory, int.MaxValue)
    {
    }

    /// <summary>
    /// Creates the pool with a factory for the items
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="maxItems"></param>
    /// <exception cref="NotImplementedException"></exception>
    public Pool(
        Func<T> factory,
        int maxItems
    ) : this(factory, null, maxItems)
    {
    }

    /// <summary>
    /// Creates the pool with the provided factory and an
    /// action to run on releasing the item
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="onRelease"></param>
    public Pool(
        Func<T> factory,
        Action<T> onRelease
    ) : this(factory, onRelease, int.MaxValue)
    {
    }

    /// <summary>
    /// Creates the pool with the provided factory and an
    /// action to run on releasing the item
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="maxItems"></param>
    /// <param name="onRelease"></param>
    public Pool(
        Func<T> factory,
        Action<T> onRelease,
        int maxItems
    )
    {
        _factory = factory;
        _onRelease = onRelease;
        MaxItems = maxItems;
    }

    /// <summary>
    /// Attempt to take an item from the pool. If possible and required,
    /// an item will be created for you. If the pool is full and no item
    /// can be made available, this will a pool item with a null instance.
    /// </summary>
    /// <returns></returns>
    public IPoolItem<T> Take()
    {
        return Take(0);
    }

    /// <summary>
    /// Attempt to take an item from the pool, with a max wait in ms
    /// when the pool is already full and you need to wait on something
    /// else to release an instance. If no instance can be found in time, then
    /// this will return a PoolItem with a default instance.
    /// </summary>
    /// <param name="maxWaitMilliseconds"></param>
    /// <returns></returns>
    public IPoolItem<T> Take(int maxWaitMilliseconds)
    {
        if (TryFindExistingAvailableInstance(0, out var result))
        {
            return result;
        }

        lock (_items)
        {
            if (Count == MaxItems)
            {
                return TryFindExistingAvailableInstance(maxWaitMilliseconds, out result)
                    ? result
                    : throw new NoPooledItemAvailableException(
                        typeof(T),
                        MaxItems,
                        maxWaitMilliseconds
                    );
            }

            result = new PoolItem<T>(
                _factory(),
                _onRelease
            );
            result.TryLock(0);
            _items.Add(result);
            return result;
        }
    }

    private bool TryFindExistingAvailableInstance(int maxWaitMs, out PoolItem<T> result)
    {
        var timeout = DateTime.Now.AddMilliseconds(maxWaitMs);
        do
        {
            foreach (var item in _items)
            {
                if (item.TryLock(0))
                {
                    result = item;
                    return true;
                }
            }

            Thread.Sleep(0);
        } while (DateTime.Now < timeout);

        result = null;
        return false;
    }

    /// <summary>
    /// Disposes of this pool and all items in the pool
    /// </summary>
    public void Dispose()
    {
        lock (_items)
        {
            while (_items.TryTake(out var item))
            {
                if (item.Instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}

/// <summary>
/// Thrown when no item could be resolved from the pool with a pure Take
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class NoPooledItemAvailableException : Exception
{
    /// <inheritdoc />
    public NoPooledItemAvailableException(
        Type type,
        int maxPoolSize,
        int maxWaitMs
    ) : base(
        $"No instance of type {type} could be provided from a pool of max size {maxPoolSize} within {maxWaitMs}ms"
    )
    {
    }
}

/// <summary>
/// Represents a pooled item of type T
/// </summary>
/// <typeparam name="T"></typeparam>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    interface IPoolItem<T>
    : IDisposable
{
    /// <summary>
    /// The instance of the pooled item
    /// </summary>
    T Instance { get; }

    /// <summary>
    /// Flag: is this instance available for usage
    /// </summary>
    bool IsAvailable { get; }
}

/// <inheritdoc />
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class PoolItem<T> : IPoolItem<T>
{
    private readonly Action<T> _onRelease;

    /// <summary>
    /// The instance of the pooled item
    /// </summary>
    public T Instance { get; }

    /// <summary>
    /// Flag: is this instance available for usage
    /// </summary>
    public bool IsAvailable => _lock.CurrentCount == 1;

    private readonly SemaphoreSlim _lock;

    /// <summary>
    /// Constructs a PooledItem around the provided instance of T
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="onRelease"></param>
    public PoolItem(
        T instance,
        Action<T> onRelease
    )
    {
        _onRelease = onRelease;
        Instance = instance;
        _lock = new SemaphoreSlim(1, 1);
        TryLock(0);
    }

    /// <summary>
    /// Locks the pooled item for usage; disposing this
    /// pooled item container releases the lock
    /// </summary>
    public bool TryLock(int maxWaitMilliseconds)
    {
        return _lock.Wait(maxWaitMilliseconds);
    }

    /// <summary>
    /// Releases the instance back to the pool
    /// </summary>
    public void Dispose()
    {
        try
        {
            _onRelease?.Invoke(Instance);
            _lock.Release();
        }
        catch (SemaphoreFullException e)
        {
            throw new ObjectDisposedException(
                "Pooled object was already disposed",
                e
            );
        }
    }
}