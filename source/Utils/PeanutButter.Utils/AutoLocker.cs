using System;
using System.Threading;

namespace PeanutButter.Utils
{
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
        /// Constructs a new AutoLocker, immediately locking the provided Semaphore
        /// </summary>
        /// <param name="semaphore">Semaphore to lock immediately</param>
        /// <exception cref="ArgumentNullException">Throws ArgumentNullException if the provided Semaphore is null</exception>
        public AutoLocker(Semaphore semaphore)
        {
            _fatty = semaphore ?? throw new ArgumentNullException(nameof(semaphore));
            _fatty.WaitOne();
        }

        /// <summary>
        /// Constructs a new AutoLocker, immediately locking the provided SemaphoreSlim
        /// </summary>
        /// <param name="semaphore">SemaphoreSlim to lock immediately</param>
        /// <exception cref="ArgumentNullException">Thrown if the provided SemaphoreSlim is null</exception>
        public AutoLocker(SemaphoreSlim semaphore)
        {
            _slim = semaphore ?? throw new ArgumentNullException(nameof(semaphore));
            _slim.Wait();
        }

        /// <summary>
        /// Constructs a new AutoLocker, immediately locking the provided Mutex
        /// </summary>
        /// <param name="mutex">Mutex to lock immediately</param>
        /// <exception cref="ArgumentNullException">Thrown if the provided Mutex is null</exception>
        public AutoLocker(Mutex mutex)
        {
            _mutex = mutex ?? throw new ArgumentNullException(nameof(mutex));
            _mutex.WaitOne();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (_fatty != null)
                {
                    _fatty.Release();
                    _fatty = null;
                    return;
                }
                if (_slim != null)
                {
                    _slim.Release();
                    _slim = null;
                    return;
                }
                _mutex.ReleaseMutex();
                _mutex = null;
            }
        }
    }
}