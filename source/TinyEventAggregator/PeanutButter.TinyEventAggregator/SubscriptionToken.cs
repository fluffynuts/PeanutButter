namespace PeanutButter.TinyEventAggregator
{
    public class SubscriptionToken
    {
        private static int _id;
        private static readonly object Lock = new object();
        // only useful to spot the difference between two tokens
        public int Id { get; }
        public SubscriptionToken()
        {
            lock(Lock)
            {
                Id = ++_id;
            }
        }
    }
}