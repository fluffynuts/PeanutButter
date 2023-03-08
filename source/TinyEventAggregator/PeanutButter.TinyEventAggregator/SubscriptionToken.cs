namespace PeanutButter.TinyEventAggregator
{
    /// <summary>
    /// A token which identifies a subscription
    /// </summary>
    public class SubscriptionToken
    {
        private static int _id;
        private static readonly object Lock = new object();

        /// <summary>
        /// The identifier for a token
        ///  - only useful to spot the difference between two tokens by eye
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Constructs the new subscription token
        /// </summary>
        public SubscriptionToken()
        {
            lock (Lock)
            {
                Id = ++_id;
            }
        }
    }
}