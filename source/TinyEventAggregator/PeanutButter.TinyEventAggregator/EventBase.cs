using System.Threading;

namespace PeanutButter.TinyEventAggregator
{
    /// <summary>
    /// Delegate type for handling when subscriptions are added
    /// </summary>
    public delegate void SubscriptionAddedEventHandler(object sender, SubscriptionsChangedEventArgs args);

    /// <summary>
    /// Delegate type for handling when subscriptions are removed
    /// </summary>
    public delegate void SubscriptionRemovedEventHandler(object sender, SubscriptionsChangedEventArgs args);

    /// <summary>
    /// Base class for events
    /// </summary>
    public abstract class EventBase
    {
        private readonly SemaphoreSlim _suspensionLock = new SemaphoreSlim(1);
        private readonly object _lock = new object();
        private readonly object _waitLock = new object();
        /// <summary>
        /// Flag: is this event suspended
        /// </summary>
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

        /// <summary>
        /// Unsuspend this event
        /// </summary>
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

        /// <summary>
        /// Suspend this event
        /// </summary>
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