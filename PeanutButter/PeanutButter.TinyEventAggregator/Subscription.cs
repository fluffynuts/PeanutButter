using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeanutButter.TinyEventAggregator
{
    public class SubscriptionToken
    {
    }

    public class Subscription<TPayload>
    {
        public SubscriptionToken Token { get; protected set; }
        public Action<TPayload> Receiver { get; protected set; }
        public Subscription(Action<TPayload> receiver)
        {
            this.Receiver = receiver;
            this.Token = new SubscriptionToken();
        }
    }
}
