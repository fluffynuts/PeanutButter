using System.Threading;

namespace PeanutButter.TinyEventAggregator
{
    public delegate void SubscriptionAddedEventHandler(object sender, SubscriptionsChangedEventArgs args);
    public delegate void SubscriptionRemovedEventHandler(object sender, SubscriptionsChangedEventArgs args);

    public abstract class EventBase 
    {
        private object _suspensionLock = new object();
        internal bool IsSuspended { get; private set; }
        public void Unsuspend()
        {
            lock (_suspensionLock)
            {
                IsSuspended = false;
            }
        }

        public void Suspend()
        {
            lock (_suspensionLock)
            {
                IsSuspended = true;
            }
        }

        internal void WaitForSuspension()
        {
            while (true)
            {
                lock (_suspensionLock)
                {
                    if (!IsSuspended)
                        return;
                    Thread.Sleep(10);
                }
            }
        }

    }
}
