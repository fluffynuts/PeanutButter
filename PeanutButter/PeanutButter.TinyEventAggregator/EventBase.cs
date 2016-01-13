using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PeanutButter.TinyEventAggregator
{
    public delegate void SubscriptionAddedEventHandler(object sender, SubscriptionsChangedEventArgs args);
    public delegate void SubscriptionRemovedEventHandler(object sender, SubscriptionsChangedEventArgs args);

    public abstract class EventBase 
    {
        private object _suspensionLock = new object();
        private bool _suspended = false;
        public void Unsuspend()
        {
            lock (_suspensionLock)
            {
                _suspended = true;
            }
        }

        public void Suspend()
        {
            lock (_suspensionLock)
            {
                _suspended = false;
            }
        }

        protected void WaitForSuspension()
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
