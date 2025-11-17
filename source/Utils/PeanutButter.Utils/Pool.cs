using System;
using System.Collections.Generic;
using System.Threading;
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace
#if BUILD_PEANUTBUTTER_INTERNAL
    Imported.PeanutButter.Utils;
#else
    PeanutButter.Utils;
#endif
/// <summary>
/// Describes a pooling service for the underlying type T
/// </summary>
/// <typeparam name="T"></typeparam>
#if BUILD_PEANUTBUTTER_INTERNAL
#if BUILD_PEANUTBUTTER_POOL_PUBLIC
public
#else
internal
#endif
#else
public
#endif
    interface IPool<T>
{
    /// <summary>
    /// The maximum number of items to hold in the pool
    /// </summary>
    int MaxItems { get; }

    /// <summary>
    /// How many items are currently in the pool
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Attempt to take an item from the pool. If possible and required,
    /// an item will be created for you. If the pool is full and no item
    /// can be made available, this will a pool item with a null instance.
    /// </summary>
    /// <returns></returns>
    [Obsolete("This has been renamed to Borrow and will be removed in a future version")]
    IPoolItem<T> Take();

    /// <summary>
    /// Attempt to take an item from the pool, with a max wait in ms
    /// when the pool is already full and you need to wait on something
    /// else to release an instance. If no instance can be found in time, then
    /// this will return a PoolItem with a default instance.
    /// </summary>
    /// <param name="maxWaitMilliseconds"></param>
    /// <returns></returns>
    [Obsolete("This has been renamed to Borrow and will be removed in a future version")]
    IPoolItem<T> Take(int maxWaitMilliseconds);

    /// <summary>
    /// Attempt to take an item from the pool. If possible and required,
    /// an item will be created for you. If the pool is full and no item
    /// can be made available, this will a pool item with a null instance.
    /// </summary>
    /// <returns></returns>
    IPoolItem<T> Borrow();

    /// <summary>
    /// Attempt to take an item from the pool, with a max wait in ms
    /// when the pool is already full and you need to wait on something
    /// else to release an instance. If no instance can be found in time, then
    /// this will return a PoolItem with a default instance.
    /// </summary>
    /// <param name="maxWaitMilliseconds"></param>
    /// <returns></returns>
    IPoolItem<T> Borrow(int maxWaitMilliseconds);

    /// <summary>
    /// Forget the item from the pool
    /// </summary>
    /// <param name="item"></param>
    void Forget(IPoolItem<T> item);

    /// <summary>
    /// Disposes of this pool and all items in the pool
    /// </summary>
    void Dispose();
}

/// <summary>
/// Provides a generic pooling mechanism using the Disposable pattern
/// </summary>
/// <typeparam name="T"></typeparam>
#if BUILD_PEANUTBUTTER_INTERNAL
#if BUILD_PEANUTBUTTER_POOL_PUBLIC
public
#else
internal
#endif
#else
public
#endif
    class Pool<T> : IDisposable, IPool<T>
{
    /// <summary>
    /// The maximum number of items to hold in the pool
    /// </summary>
    public int MaxItems { get; }

    /// <summary>
    /// How many items are currently in the pool
    /// </summary>
    public int Count => _items.Count;

    private readonly List<IPoolItem<T>> _items = new();
    private readonly Func<IPool<T>, T> _factory;
    private readonly Action<T> _onRelease;

    private TResult WithLockedItems<TResult>(Func<List<IPoolItem<T>>, TResult> toRun)
    {
        VerifyNotDisposed();
        lock (_items)
        {
            return toRun(_items);
        }
    }

    private void WithLockedItems(Action<List<IPoolItem<T>>> toRun)
    {
        VerifyNotDisposed();
        WithLockedItemsInternal(toRun);
    }

    private void WithLockedItemsInternal(Action<List<IPoolItem<T>>> toRun)
    {
        lock (_items)
        {
            toRun(_items);
        }
    }

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
        _factory = o => factory();
        _onRelease = onRelease;
        MaxItems = maxItems;
    }

    /// <summary>
    /// Creates the pool with a factory for the items
    /// </summary>
    /// <param name="factory"></param>
    /// <exception cref="NotImplementedException"></exception>
    public Pool(
        Func<IPool<T>, T> factory
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
        Func<IPool<T>, T> factory,
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
        Func<IPool<T>, T> factory,
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
        Func<IPool<T>, T> factory,
        Action<T> onRelease,
        int maxItems
    )
    {
        _factory = factory;
        _onRelease = onRelease;
        MaxItems = maxItems;
    }

    /// <inheritdoc />
    [Obsolete("This has been renamed to Borrow and will be removed in a future version")]
    public IPoolItem<T> Take()
    {
        return Borrow();
    }

    /// <inheritdoc />
    [Obsolete("This has been renamed to Borrow and will be removed in a future version")]
    public IPoolItem<T> Take(int maxWaitMilliseconds)
    {
        return Borrow(maxWaitMilliseconds);
    }

    /// <summary>
    /// Attempt to take an item from the pool. If possible and required,
    /// an item will be created for you. If the pool is full and no item
    /// can be made available, this will a pool item with a null instance.
    /// </summary>
    /// <returns></returns>
    public IPoolItem<T> Borrow()
    {
        return Borrow(0);
    }

    /// <summary>
    /// Attempt to take an item from the pool, with a max wait in ms
    /// when the pool is already full and you need to wait on something
    /// else to release an instance. If no instance can be found in time, then
    /// this will return a PoolItem with a default instance.
    /// </summary>
    /// <param name="maxWaitMilliseconds"></param>
    /// <returns></returns>
    public IPoolItem<T> Borrow(int maxWaitMilliseconds)
    {
        if (TryFindExistingAvailableInstance(0, out var result))
        {
            return result;
        }

        return WithLockedItems(items =>
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
                    _factory(this),
                    _onRelease,
                    this
                );
                result.TryLock(0);
                items.Add(result);
                return result;
            }
        );
    }

    /// <inheritdoc />
    public void Forget(IPoolItem<T> item)
    {
        WithLockedItems(items =>
            {
                items.Remove(item);
            }
        );
    }

    private bool TryFindExistingAvailableInstance(int maxWaitMs, out IPoolItem<T> result)
    {
        VerifyNotDisposed();
        var timeout = DateTime.Now.AddMilliseconds(maxWaitMs);
        do
        {
            result = WithLockedItems(items =>
                {
                    foreach (var item in items)
                    {
                        if (item.TryLock(0))
                        {
                            return item;
                        }
                    }

                    return null;
                }
            );
            if (result is not null)
            {
                return true;
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
        lock (_disposedLock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        WithLockedItemsInternal(items =>
            {
                foreach (var item in items)
                {
                    if (item.Instance is not IDisposable disposable)
                    {
                        continue;
                    }

                    try
                    {
                        disposable.Dispose();
                    }
                    catch
                    {
                        // suppress so we can dispose of others
                    }
                }
            }
        );
    }

    private void VerifyNotDisposed()
    {
        lock (_disposedLock)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(
                    "This pool is already disposed"
                );
            }
        }
    }

    private readonly object _disposedLock = new();
    private bool _disposed;
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
#if BUILD_PEANUTBUTTER_POOL_PUBLIC
public
#else
internal
#endif
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

    /// <summary>
    /// Used internally to try to lock off a pool item
    /// so it can be handed out to another consumer
    /// - you should never need to call this, and it
    ///   should always return false from within consumer code
    /// </summary>
    /// <param name="maxWait"></param>
    bool TryLock(int maxWait);
}

/// <inheritdoc />
#if BUILD_PEANUTBUTTER_INTERNAL
#if BUILD_PEANUTBUTTER_POOL_PUBLIC
    public
#else
internal
#endif
#else
public
#endif
    class PoolItem<T> : IPoolItem<T>
{
    private readonly Action<T> _onRelease;
    private readonly IPool<T> _owner;

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
    /// <param name="owner"></param>
    public PoolItem(
        T instance,
        Action<T> onRelease,
        IPool<T> owner
    )
    {
        _onRelease = onRelease;
        _owner = owner;
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
        catch (Exception)
        {
            // if we can't complete _onRelease successfully, then
            // this shouldn't go back in the pool ):
            _owner.Forget(this);
        }
    }
}