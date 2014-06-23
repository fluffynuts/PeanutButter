using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PeanutButter.Utils
{
    public class AutoLocker: IDisposable
    {
        private Semaphore _semaphore;
        private Mutex _mutex;

        public AutoLocker(Semaphore semaphore)
        {
            if (semaphore == null) throw new ArgumentNullException("semaphore");
            _semaphore = semaphore;
            _semaphore.WaitOne();
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
                if (_semaphore != null)
                {
                    _semaphore.Release();
                    _semaphore = null;
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
