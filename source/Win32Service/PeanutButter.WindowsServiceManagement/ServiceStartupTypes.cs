using System;

namespace PeanutButter.WindowsServiceManagement
{
    // read-only view, ie what is read from the registry
    /// <summary>
    /// Startup types for services
    /// </summary>
    public enum ServiceStartupTypes
    {
        /// <summary>
        /// Delayed start, automatic
        /// </summary>
        DelayedAutomatic = -2,
        /// <summary>
        /// Unkown
        /// </summary>
        Unknown = -1,
        /// <summary>
        /// Applies to drivers, not services
        /// </summary>
        [Obsolete(Messages.MEANT_FOR_DRIVERS)]
        Boot = 0,
        /// <summary>
        /// Applies to drivers, not services
        /// </summary>
        [Obsolete(Messages.MEANT_FOR_DRIVERS)]
        System = 1,
        /// <summary>
        /// Automatic start
        /// </summary>
        Automatic = 2,
        /// <summary>
        /// Manual start
        /// </summary>
        Manual = 3,
        /// <summary>
        /// Start is disabled
        /// </summary>
        Disabled = 4
    }
    
    internal static class Messages
    {
        public const string MEANT_FOR_DRIVERS =
            "It's highly unlikely that this is what you want (it's for drivers). #pragma to suppress if you know what you're doing";
    }
}