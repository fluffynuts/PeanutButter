using System;

namespace PeanutButter.Utils
{
    public class AutoResetter: IDisposable
    {
        private object _lock = new object();
        private Action _disposalAction;

        public AutoResetter(Action constructionAction, Action disposalAction)
        {
            constructionAction();
            _disposalAction = disposalAction;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposalAction == null)
                    return;
                try
                {
                    _disposalAction();
                }
                finally
                {
                    _disposalAction = null;
                }
            }
        }
    }

    public class AutoResetter<T>: IDisposable
    {
        private T _initialValue;
        private object _lock = new object();
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
