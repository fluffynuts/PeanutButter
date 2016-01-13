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
}