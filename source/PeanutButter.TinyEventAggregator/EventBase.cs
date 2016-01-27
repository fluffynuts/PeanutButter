using System.Threading;

namespace PeanutButter.TinyEventAggregator
{
    public delegate void SubscriptionAddedEventHandler(object sender, SubscriptionsChangedEventArgs args);
    public delegate void SubscriptionRemovedEventHandler(object sender, SubscriptionsChangedEventArgs args);

    public abstract class EventBase 
    {
        private object _suspensionLock = new object();
        internal bool _suspended = false;
        public void Unsuspend()
        {
            lock (_suspensionLock)
            {
                _suspended = false;
            }
        }

        public void Suspend()
        {
            lock (_suspensionLock)
            {
                _suspended = true;
            }
        }

        internal void WaitForSuspension()
        {
            while (true)
            {
                lock (_suspensionLock)
                {
                    if (!_suspended)
                        return;
                    Thread.Sleep(10);
                }
            }
        }

    }
}
