using System;

namespace PeanutButter.TinyEventAggregator
{
    public class SubscriptionToken
    {
    }

    public class Subscription<TPayload>
    {
        public enum FuturePublicationsStates
        {
            Infinite,
            NotExpired,
            Expired
        }
        private int _originalLimit;
        public SubscriptionToken Token { get; protected set; }
        public Action<TPayload> Receiver { get; protected set; }
        public int RemainingCalls { get; protected set; }
        public Subscription(Action<TPayload> receiver, int limit = 0)
        {
            this.Receiver = receiver;
            this.Token = new SubscriptionToken();
            this.RemainingCalls = limit;
            this._originalLimit = limit;
        }

        public bool OnlyOneCallLeft()
        {
            lock (this)
            {
                return (_originalLimit > 0 && RemainingCalls == 1);
            }
        }
        public void DecrementRemainingCallCount()
        {
            lock (this)
            {
                if (_originalLimit > 0)
                    RemainingCalls--;
            }
        }
    }
}
