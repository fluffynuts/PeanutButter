using System;

namespace PeanutButter.TinyEventAggregator
{
    /// <summary>
    /// Represents a subscription for a specific event and payload
    /// </summary>
    /// <typeparam name="TPayload"></typeparam>
    public class Subscription<TPayload>
    {
        /// <summary>
        /// States for future publication
        /// </summary>
        public enum FuturePublicationsStates
        {
            /// <summary>
            /// Infinite
            /// </summary>
            Infinite,

            /// <summary>
            /// Not expired
            /// </summary>
            NotExpired,

            /// <summary>
            /// Has expired
            /// </summary>
            Expired
        }

        private readonly int _originalLimit;

        /// <summary>
        /// The token representing this subscription
        /// </summary>
        public SubscriptionToken Token { get; protected set; }

        /// <summary>
        /// The receiver when this subscription is fired
        /// </summary>
        public Action<TPayload> Receiver { get; protected set; }

        /// <summary>
        /// How many times this subscription can be called in the future
        /// </summary>
        public int RemainingCalls { get; protected set; }

        /// <summary>
        /// Create a subscription with the provided receiver and
        /// an optional limit. If the limit is set to zero or lower
        /// then the subscription is fired until unsubscribed.
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="limit"></param>
        public Subscription(Action<TPayload> receiver, int limit = 0)
        {
            Receiver = receiver;
            Token = new SubscriptionToken();
            RemainingCalls = limit;
            _originalLimit = limit < 0
                ? 0
                : limit;
        }

        /// <summary>
        /// Helper: if there's only one call left
        /// </summary>
        /// <returns></returns>
        public bool OnlyOneCallLeft()
        {
            lock (this)
            {
                return _originalLimit > 0 && RemainingCalls == 1;
            }
        }

        /// <summary>
        /// Decrements the remaining allowed call count, if applicable
        /// </summary>
        public void DecrementRemainingCallCount()
        {
            lock (this)
            {
                if (_originalLimit > 0)
                {
                    RemainingCalls--;
                }
            }
        }
    }
}