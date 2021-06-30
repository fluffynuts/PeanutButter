using System;

namespace PeanutButter.TempDb
{
    public class TempDbDisposedEventArgs
    {
        public string Reason { get; }
        public bool WasAutomatic { get; }
        public TimeSpan? InactivityTimeout { get; }
        public TimeSpan? AbsoluteLifespan { get; }

        public TempDbDisposedEventArgs(
            string reason,
            bool wasAutomatic,
            TimeSpan? inactivityTimeout,
            TimeSpan? absoluteLifespan)
        {
            Reason = reason;
            WasAutomatic = wasAutomatic;
            InactivityTimeout = inactivityTimeout;
            AbsoluteLifespan = absoluteLifespan;
        }
    }
}