using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PeanutButter.Utils
{
    public class AutoLocker: IDisposable
    {
        private Semaphore _fatty;
        private Mutex _mutex;
        private SemaphoreSlim _slim;

        public AutoLocker(Semaphore semaphore)
        {
            if (semaphore == null) throw new ArgumentNullException("semaphore");
            _fatty = semaphore;
            _fatty.WaitOne();
        }

        public AutoLocker(SemaphoreSlim semaphore)
        {
            if (semaphore == null) throw new ArgumentNullException("semaphore");
            _slim = semaphore;
            _slim.Wait();
        }

        public AutoLocker(Mutex mutex)
        {
            if (mutex == null) throw new ArgumentNullException("mutex");
            _mutex = mutex;
            _mutex.WaitOne();
        }

        public void Dispose()
        {
            lock (this)
            {
                if (_fatty != null)
                {
                    _fatty.Release();
                    _fatty = null;
                }
                if (_slim != null)
                {
                    _slim.Release();
                    _slim = null;
                }
                if (_mutex != null)
                {
                    _mutex.ReleaseMutex();
                    _mutex = null;
                }
            }
        }
    }
}
