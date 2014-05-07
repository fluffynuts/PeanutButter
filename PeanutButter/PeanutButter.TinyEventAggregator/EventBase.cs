using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PeanutButter.TinyEventAggregator
{
    public class SubscriptionsChangedEventArgs
    {
        public SubscriptionsChangedEventArgs(SubscriptionToken token)
        {
            Token = token;
        }

        public SubscriptionToken Token { get; set; }
    }

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
    public abstract class EventBase<TPayload>: EventBase
    {
        public int SubscriptionCount
        {
            get
            {
                lock (this)
                {
                    return _subscriptions.Count;
                }
            }
        }
        public SubscriptionAddedEventHandler OnSubscriptionAdded { get; set; }
        public SubscriptionRemovedEventHandler OnSubscriptionRemoved { get; set; }

        private readonly List<Subscription<TPayload>> _subscriptions;
        private string _eventName;

        protected EventBase()
        {
            this._subscriptions = new List<Subscription<TPayload>>();
            this._eventName = this.GetType().Name;
        }

        public SubscriptionToken Subscribe(Action<TPayload> callback, 
            [CallerFilePath] string sourceFile = "", [CallerMemberName] string requestingMethod = "(unknown)", [CallerLineNumber] int atLine = -1)
        {
            lock (this)
            {
                var token = PerformSubscription(callback, 0);
                Debug.WriteLine("Subscribing [{0}] indefinitely to event [{1}] ({2}:{3}:{4})", token, this.GetType().Name, sourceFile, requestingMethod, atLine);
                return token;
            }
        }

        private void RaiseSubscriptionAddedEventHandler(SubscriptionToken token)
        {
            var handler = OnSubscriptionAdded;
            if (handler == null) return;
            try
            {
                handler(this, new SubscriptionsChangedEventArgs(token));
            }
            catch { }
        }

        public SubscriptionToken SubscribeOnce(Action<TPayload> action,
            [CallerFilePath] string sourceFile = "", [CallerMemberName] string requestingMethod = "(unknown)", [CallerLineNumber] int atLine = -1)
        {
            lock (this)
            {
                var token = PerformSubscription(action, 1);
                Debug.WriteLine("Subscribing [{0}] once-off to event [{1}] ({2}:{3}:{4})", token, _eventName, sourceFile, requestingMethod, atLine);
                return token;
            }
        }

        public SubscriptionToken LimitedSubscription(Action<TPayload> action, int limit,
            [CallerFilePath] string sourceFile = "", [CallerMemberName] string requestingMethod = "(unknown)", [CallerLineNumber] int atLine = -1)
        {
            lock (this)
            {
                var token = PerformSubscription(action, limit);
                Debug.WriteLine("Subscribing [{0}] to event [{1}] for {5} publications ({2}:{3}:{4})", token, _eventName, sourceFile, requestingMethod, atLine, limit);
                return token;
            }
        }

        private SubscriptionToken PerformSubscription(Action<TPayload> action, int limit)
        {
            if (action == null) throw new ArgumentNullException("callback");
            lock (this)
            {
                var subscription = new Subscription<TPayload>(action, limit);
                this._subscriptions.Add(subscription);
                RaiseSubscriptionAddedEventHandler(subscription.Token);
                return subscription.Token;
            }
        }

        public void Publish(TPayload data,
            [CallerFilePath] string sourceFile = "", [CallerMemberName] string requestingMethod = "(unknown)", [CallerLineNumber] int atLine = -1)
        {
            lock(this)
            {
                WaitForSuspension();
                var subscriptions = _subscriptions.ToArray();
                Debug.WriteLine(String.Format("Publishing event [{0}] to {1} subscribers ({2}:{3}:{4})", 
                    _eventName, subscriptions.Length, sourceFile, requestingMethod, atLine));
                foreach (var sub in subscriptions)
                {
                    if (sub.OnlyOneCallLeft())
                        _subscriptions.Remove(sub);
                }
                foreach (var sub in subscriptions)
                {
                    sub.Receiver(data);
                    if (sub.RemainingCalls > 0)
                    {
                        sub.DecrementRemainingCallCount();
                    }
                }
            }
        }

        public void Unsubscribe(SubscriptionToken token, 
            [CallerFilePath] string sourceFile = "", [CallerMemberName] string requestingMethod = "(unknown)", [CallerLineNumber] int atLine = -1)
        {
            if (token == null) throw new ArgumentNullException("token");
            lock (this)
            {
                var match = this._subscriptions.FirstOrDefault(s => s.Token == token);
                if (match != null)
                {
                    Debug.WriteLine(String.Format("Unsubscribing [{0}] from event [{1}] ({2}:{3}:{4})", match.Token, _eventName, sourceFile, requestingMethod, atLine));
                    this._subscriptions.Remove(match);
                    RaiseSubscriptionRemovedEventHandler(match.Token);
                }
            }
        }

        private void RaiseSubscriptionRemovedEventHandler(SubscriptionToken token)
        {
            var handler = OnSubscriptionRemoved;
            if (handler == null) return;
            try
            {
                handler(this, new SubscriptionsChangedEventArgs(token));
            }
            catch { }
        }
    }
}
