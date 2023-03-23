using System;
using System.Net;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    internal class UnableToFindOpenPortException
        : Exception
    {
        public IPAddress IPAddress { get; }
        public int MinPort { get; }
        public int MaxPort { get; }

        public UnableToFindOpenPortException(
            IPAddress forAddress,
            int min,
            int max
        ) : base($"Unable to find an open port within the range {min}-{max} on {forAddress}")
        {
            IPAddress = forAddress;
            MinPort = min;
            MaxPort = max;
        }
    }
}