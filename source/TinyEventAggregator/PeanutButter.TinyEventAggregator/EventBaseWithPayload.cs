using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global

namespace PeanutButter.TinyEventAggregator
{
    /// <inheritdoc />
    public abstract class EventBase<TPayload> : EventBase
    {
        /// <summary>
        /// The number of subscribers to this event
        /// </summary>
        public int SubscriptionCount
        {
            get
            {
                lock (_lock)
                {
                    return _subscriptions.Count;
                }
            }
        }
        private readonly object _lock = new();

        /// <summary>
        /// Fired when a subscription is added for this event
        /// </summary>
        public SubscriptionAddedEventHandler OnSubscriptionAdded { get; set; }
        /// <summary>
        /// Fired when a subscription is removed for this event
        /// </summary>
        public SubscriptionRemovedEventHandler OnSubscriptionRemoved { get; set; }

        private readonly List<Subscription<TPayload>> _subscriptions;
        private readonly string _eventName;

        /// <inheritdoc />
        protected EventBase()
        {
            _subscriptions = new List<Subscription<TPayload>>();
            _eventName = GetType().Name;
        }

        /// <summary>
        /// Subscribe to the event
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="sourceFile"></param>
        /// <param name="requestingMethod"></param>
        /// <param name="subscribingSourceLine"></param>
        /// <returns></returns>
        public SubscriptionToken Subscribe(
            Action<TPayload> callback,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string requestingMethod = "(unknown)",
            [CallerLineNumber] int subscribingSourceLine = -1)
        {
            lock (_lock)
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
            if (handler == null)
            {
                return;
            }

            try
            {
                handler(this, new SubscriptionsChangedEventArgs(token));
            }
            catch
            {
                /* intentionally left blank */
            }
        }

        /// <summary>
        /// Subscribe to the event for one iteration only
        /// </summary>
        /// <param name="action"></param>
        /// <param name="sourceFile"></param>
        /// <param name="requestingMethod"></param>
        /// <param name="subscribingSourceLine"></param>
        /// <returns></returns>
        public SubscriptionToken SubscribeOnce(
            Action<TPayload> action,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string requestingMethod = "(unknown)",
            [CallerLineNumber] int subscribingSourceLine = -1)
        {
            lock (_lock)
            {
                var token = PerformSubscription(action, 1);
                Debug.WriteLine(
                    $"Subscribing [{token}] once-off to event [{_eventName}] ({sourceFile}:{requestingMethod}:{subscribingSourceLine})");
                return token;
            }
        }

        /// <summary>
        /// Subscribe to the event for a limited number of notifications
        /// </summary>
        /// <param name="action"></param>
        /// <param name="limit"></param>
        /// <param name="sourceFile"></param>
        /// <param name="requestingMethod"></param>
        /// <param name="subscribingSourceLine"></param>
        /// <returns></returns>
        public SubscriptionToken LimitedSubscription(
            Action<TPayload> action,
            int limit,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string requestingMethod = "(unknown)",
            [CallerLineNumber] int subscribingSourceLine = -1)
        {
            lock (_lock)
            {
                var token = PerformSubscription(action, limit);
                Debug.WriteLine(
                    $"Subscribing [{token}] to event [{_eventName}] for {limit} publications ({sourceFile}:{requestingMethod}:{subscribingSourceLine})");
                return token;
            }
        }

        private SubscriptionToken PerformSubscription(Action<TPayload> action, int limit)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            lock (_lock)
            {
                var subscription = new Subscription<TPayload>(action, limit);
                _subscriptions.Add(subscription);
                RaiseSubscriptionAddedEventHandler(subscription.Token);
                return subscription.Token;
            }
        }

        /// <summary>
        /// Publish this event with some data to all subscribers
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sourceFile"></param>
        /// <param name="requestingMethod"></param>
        /// <param name="publishingSourceLine"></param>
        public void Publish(
            TPayload data,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string requestingMethod = "(unknown)",
            [CallerLineNumber] int publishingSourceLine = -1)
        {
            lock (_lock)
            {
                WaitForSuspension();
                var subscriptions = _subscriptions.ToArray();
                Debug.WriteLine(
                    $"Publishing event [{_eventName}] to {subscriptions.Length} subscribers ({sourceFile}:{requestingMethod}:{publishingSourceLine})");
                foreach (var sub in subscriptions)
                {
                    if (sub.OnlyOneCallLeft())
                    {
                        _subscriptions.Remove(sub);
                    }
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

        /// <summary>
        /// Publish the event to all subscribers with the provided data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sourceFile"></param>
        /// <param name="requestingMethod"></param>
        /// <param name="publishingSourceLine"></param>
        /// <returns></returns>
        public Task PublishAsync(
            TPayload data,
            [CallerFilePath] string sourceFile = null,
            [CallerMemberName] string requestingMethod = null,
            [CallerLineNumber] int publishingSourceLine = -1)
        {
            return Task.Run(() => Publish(
                data,
                sourceFile,
                requestingMethod,
                publishingSourceLine
            ));
        }

        /// <summary>
        /// Unsubscribe from this event with the provided token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="sourceFile"></param>
        /// <param name="requestingMethod"></param>
        /// <param name="unsubscribingSourceLine"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void Unsubscribe(
            SubscriptionToken token,
            [CallerFilePath] string sourceFile = "",
            [CallerMemberName] string requestingMethod = "(unknown)",
            [CallerLineNumber] int unsubscribingSourceLine = -1)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            lock (_lock)
            {
                var match = _subscriptions.FirstOrDefault(s => s.Token == token);
                if (match == null)
                {
                    return;
                }

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
            if (handler == null)
            {
                return;
            }

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