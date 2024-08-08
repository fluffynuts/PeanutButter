using System;
using System.Diagnostics;
using System.Threading;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils;
#else
namespace PeanutButter.Utils;
#endif
/// <summary>
/// Class to use the using() {} pattern to take care of locking / unlocking one of:
/// - Semaphore
/// - SemaphoreSlim
/// - Mutex
/// without the consumer having to worry about unlocking in the event of exception
/// handling
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
public
#endif
    class AutoLocker : IDisposable
{
    private readonly object _disposeLock = new object();
    private Semaphore _fatty;
    private Mutex _mutex;
    private SemaphoreSlim _slim;

    /// <summary>
    /// Subscribe to this to be notified when this
    /// locker is disposed, or pass an action in
    /// on the constructor
    /// </summary>
    public EventHandler OnDisposed { get; set; }

    /// <summary>
    /// Constructs a new AutoLocker, immediately locking the provided Semaphore
    /// </summary>
    /// <param name="semaphore">Semaphore to lock immediately</param>
    /// <exception cref="ArgumentNullException">Throws ArgumentNullException if the provided Semaphore is null</exception>
    public AutoLocker(Semaphore semaphore) : this(semaphore, null)
    {
    }

    /// <summary>
    /// Constructs a new AutoLocker, immediately locking the provided Semaphore,
    /// with an OnDisposed event handler
    /// </summary>
    /// <param name="semaphore"></param>
    /// <param name="onDisposed"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public AutoLocker(Semaphore semaphore, Action onDisposed)
    {
        _fatty = semaphore ?? throw new ArgumentNullException(nameof(semaphore));
        SetOnDisposed(onDisposed);
        _fatty.WaitOne();
    }

    /// <summary>
    /// Constructs a new AutoLocker, immediately locking the provided SemaphoreSlim
    /// </summary>
    /// <param name="semaphore">SemaphoreSlim to lock immediately</param>
    /// <exception cref="ArgumentNullException">Thrown if the provided SemaphoreSlim is null</exception>
    public AutoLocker(SemaphoreSlim semaphore): this(semaphore, null)
    {
    }

    /// <summary>
    /// Constructs a new AutoLocker, immediately locking the provided SemaphoreSlim,
    /// with an OnDisposed event handler
    /// </summary>
    /// <param name="semaphore"></param>
    /// <param name="onDisposed"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public AutoLocker(SemaphoreSlim semaphore, Action onDisposed)
    {
        _slim = semaphore ?? throw new ArgumentNullException(nameof(semaphore));
        SetOnDisposed(onDisposed);
        _slim.Wait();
    }

    /// <summary>
    /// Constructs a new AutoLocker, immediately locking the provided Mutex
    /// </summary>
    /// <param name="mutex">Mutex to lock immediately</param>
    /// <exception cref="ArgumentNullException">Thrown if the provided Mutex is null</exception>
    public AutoLocker(Mutex mutex): this(mutex, null)
    {
    }

    /// <summary>
    /// Constructs a new AutoLocker, immediately locking the provided Mutex,
    /// with an OnDisposed event handler
    /// </summary>
    /// <param name="mutex">Mutex to lock immediately</param>
    /// <param name="onDisposed"></param>
    /// <exception cref="ArgumentNullException">Thrown if the provided Mutex is null</exception>
    public AutoLocker(Mutex mutex, Action onDisposed)
    {
        _mutex = mutex ?? throw new ArgumentNullException(nameof(mutex));
        SetOnDisposed(onDisposed);
        _mutex.WaitOne();
    }

    private void SetOnDisposed(Action onDisposed)
    {
        if (onDisposed is null)
        {
            return;
        }

        OnDisposed += (_, _) => onDisposed();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_disposeLock)
        {
            if (_fatty is not null)
            {
                _fatty.Release();
                _fatty = null;
                TriggerOnDisposed();
                return;
            }

            if (_slim is not null)
            {
                _slim.Release();
                _slim = null;
                TriggerOnDisposed();
                return;
            }

            if (_mutex is not null)
            {
                _mutex.ReleaseMutex();
                TriggerOnDisposed();
                _mutex = null;
            }
        }
    }

    private void TriggerOnDisposed()
    {
        var handlers = OnDisposed;
        OnDisposed = null;
        if (handlers is null)
        {
            return;
        }

        try
        {
            handlers.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"OnDisposed handler error: {ex.Message}");
        }
    }
}