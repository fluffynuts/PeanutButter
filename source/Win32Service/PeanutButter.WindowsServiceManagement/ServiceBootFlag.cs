using System;

namespace PeanutButter.WindowsServiceManagement
{
    /// <summary>
    /// write flags, ie what is provided when registering a service
    /// </summary>
    public enum ServiceBootFlag
    {
        [Obsolete(Messages.MEANT_FOR_DRIVERS)]
        BootStart = 0x00000000,
        [Obsolete(Messages.MEANT_FOR_DRIVERS)]
        SystemStart = 0x00000001,
        /// <summary>
        /// starts with the system (delayed auto-start is not catered for) 
        /// </summary>
        AutoStart = 0x00000002,   
        /// <summary>
        /// Manual start (api docs refer to DemandStart, but really mean
        /// that code somewhere "demands" a start with a StartService
        /// call... ie, manual start)
        /// </summary>
        ManualStart = 0x00000003,
        /// <summary>
        /// Disabled -- no start
        /// </summary>
        Disabled = 0x00000004
    }

}