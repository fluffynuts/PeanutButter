namespace PeanutButter.TinyEventAggregator
{
    public class SubscriptionToken
    {
        private static int _id;
        private static object _lock = new object();
        // only useful to spot the difference between two tokens
        public int Id { get; private set; }
        public SubscriptionToken()
        {
            lock(_lock)
            {
                Id = ++_id;
            }
        } 
    }
}