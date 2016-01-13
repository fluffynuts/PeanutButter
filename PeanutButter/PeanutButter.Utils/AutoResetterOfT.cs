using System;

namespace PeanutButter.Utils
{
    public class AutoResetter<T>: IDisposable
    {
        private readonly T _initialValue;
        private readonly object _lock = new object();
        private Action<T> _disposalAction;

        public AutoResetter(Func<T> start, Action<T> end)
        {
            _initialValue = start();
            _disposalAction = end;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposalAction == null)
                    return;
                try
                {
                    _disposalAction(_initialValue);
                }
                finally
                {
                    _disposalAction = null;
                }
            }
        }
    }
}
