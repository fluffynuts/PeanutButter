using System.Threading;

namespace PeanutButter.TinyEventAggregator
{
    public delegate void SubscriptionAddedEventHandler(object sender, SubscriptionsChangedEventArgs args);

    public delegate void SubscriptionRemovedEventHandler(object sender, SubscriptionsChangedEventArgs args);

    public abstract class EventBase
    {
        private readonly SemaphoreSlim _suspensionLock = new SemaphoreSlim(1);
        private readonly object _lock = new object();
        private readonly object _waitLock = new object();
        public bool IsSuspended 
        {
            get {
                lock (_lock)
                {
                    return _isSuspended;
                }
            }
        }

        private bool _isSuspended;

        public void Unsuspend()
        {
            lock (_lock)
            {
                if (!_isSuspended)
                {
                    return;
                }

                _suspensionLock.Release();
                _isSuspended = false;
            }
        }

        public void Suspend()
        {
            lock (_lock)
            {
                if (_isSuspended)
                {
                    return;
                }

                _suspensionLock.Wait();
                _isSuspended = true;
            }
        }

        internal void WaitForSuspension()
        {
            lock (_waitLock)
            {
                lock (_lock)
                {
                    if (!IsSuspended)
                    {
                        return;
                    }
                }

                _suspensionLock.Wait();
                _suspensionLock.Release();
                _isSuspended = false;
            }
        }
    }
}