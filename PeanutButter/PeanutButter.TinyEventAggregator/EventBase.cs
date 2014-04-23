using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeanutButter.TinyEventAggregator
{
    public abstract class EventBase { }
    public abstract class EventBase<TPayload>: EventBase
    {
        private List<Subscription<TPayload>> _subscriptions;
        protected EventBase()
        {
            this._subscriptions = new List<Subscription<TPayload>>();
        }

        public SubscriptionToken Subscribe(Action<TPayload> callback)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            lock (this)
            {
                var subscription = new Subscription<TPayload>(callback);
                this._subscriptions.Add(subscription);
                return subscription.Token;
            }
        }

        public void Publish(TPayload data)
        {
            lock(this)
            {
                foreach (var sub in this._subscriptions)
                    sub.Receiver(data);
            }
        }

        public void Unsubscribe(SubscriptionToken token)
        {
            if (token == null) throw new ArgumentNullException("token");
            lock (this)
            {
                var match = this._subscriptions.FirstOrDefault(s => s.Token == token);
                if (match != null)
                {
                    this._subscriptions.Remove(match);
                }
            }
        }
    }
}
