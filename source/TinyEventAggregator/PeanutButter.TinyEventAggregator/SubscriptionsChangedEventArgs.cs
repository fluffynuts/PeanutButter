namespace PeanutButter.TinyEventAggregator
{
    /// <summary>
    /// Event args when a subscription is changed
    /// </summary>
    public class SubscriptionsChangedEventArgs
    {
        /// <summary>
        /// Constructs the event args with the provided token
        /// </summary>
        /// <param name="token"></param>
        public SubscriptionsChangedEventArgs(SubscriptionToken token)
        {
            Token = token;
        }

        /// <summary>
        /// The token for the changed subscription
        /// </summary>
        public SubscriptionToken Token { get; set; }
    }
}