using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global

namespace PeanutButter.TinyEventAggregator
{
    public abstract class EventBase<TPayload> : EventBase
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
        private readonly string _eventName;

        protected EventBase()
        {
            _subscriptions = new List<Subscription<TPayload>>();
            _eventName = GetType().Name;
        }

        public SubscriptionToken Subscribe(
            Action<TPayload> callback,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string requestingMethod = "(unknown)",
            [CallerLineNumber] int subscribingSourceLine = -1)
        {
            lock (this)
            {
                var token = PerformSubscription(callback, 0);
                Debug.WriteLine(
                    $"Subscribing [{token}] indefinitely to event [{GetType().Name}] ({sourceFile}:{requestingMethod}:{subscribingSourceLine})");
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
            catch
            {
                /* intentionally left blank */
            }
        }

        public SubscriptionToken SubscribeOnce(
            Action<TPayload> action,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string requestingMethod = "(unknown)",
            [CallerLineNumber] int subscribingSourceLine = -1)
        {
            lock (this)
            {
                var token = PerformSubscription(action, 1);
                Debug.WriteLine(
                    $"Subscribing [{token}] once-off to event [{_eventName}] ({sourceFile}:{requestingMethod}:{subscribingSourceLine})");
                return token;
            }
        }

        public SubscriptionToken LimitedSubscription(
            Action<TPayload> action,
            int limit,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string requestingMethod = "(unknown)",
            [CallerLineNumber] int subscribingSourceLine = -1)
        {
            lock (this)
            {
                var token = PerformSubscription(action, limit);
                Debug.WriteLine(
                    $"Subscribing [{token}] to event [{_eventName}] for {limit} publications ({sourceFile}:{requestingMethod}:{subscribingSourceLine})");
                return token;
            }
        }

        private SubscriptionToken PerformSubscription(Action<TPayload> action, int limit)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            lock (this)
            {
                var subscription = new Subscription<TPayload>(action, limit);
                _subscriptions.Add(subscription);
                RaiseSubscriptionAddedEventHandler(subscription.Token);
                return subscription.Token;
            }
        }

        public void Publish(
            TPayload data,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string requestingMethod = "(unknown)",
            [CallerLineNumber] int publishingSourceLine = -1)
        {
            lock (this)
            {
                WaitForSuspension();
                var subscriptions = _subscriptions.ToArray();
                Debug.WriteLine(
                    $"Publishing event [{_eventName}] to {subscriptions.Length} subscribers ({sourceFile}:{requestingMethod}:{publishingSourceLine})");
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

        public void Unsubscribe(
            SubscriptionToken token,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string requestingMethod = "(unknown)",
            [CallerLineNumber] int unsubscribingSourceLine = -1)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            lock (this)
            {
                var match = _subscriptions.FirstOrDefault(s => s.Token == token);
                if (match == null)
                    return;

                Debug.WriteLine(
                    $"Unsubscribing [{match.Token}] from event [{_eventName}] ({sourceFile}:{requestingMethod}:{unsubscribingSourceLine})"
                );
                _subscriptions.Remove(match);
                RaiseSubscriptionRemovedEventHandler(match.Token);
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
            catch (Exception ex)
            {
                Debug.WriteLine($"Event handler for subscription removal throws {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}