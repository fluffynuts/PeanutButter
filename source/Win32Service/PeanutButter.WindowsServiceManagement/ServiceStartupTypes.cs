using System;

namespace PeanutButter.WindowsServiceManagement
{
    // read-only view, ie what is read from the registry
    public enum ServiceStartupTypes
    {
        DelayedAutomatic = -2,
        Unknown = -1,
        [Obsolete(Messages.MEANT_FOR_DRIVERS)]
        Boot = 0,
        [Obsolete(Messages.MEANT_FOR_DRIVERS)]
        System = 1,
        Automatic = 2,
        Manual = 3,
        Disabled = 4
    }
    
    internal static class Messages
    {
        public const string MEANT_FOR_DRIVERS =
            "It's highly unlikely that this is what you want (it's for drivers). #pragma to suppress if you know what you're doing";
    }
}