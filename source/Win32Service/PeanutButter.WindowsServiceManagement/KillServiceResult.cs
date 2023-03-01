namespace PeanutButter.WindowsServiceManagement
{
    /// <summary>
    /// The result of attempting to kill a service
    /// </summary>
    public enum KillServiceResult
    {
        /// <summary>
        /// Service was already not running
        /// </summary>
        NotRunning,
        /// <summary>
        /// Service was terminated
        /// </summary>
        Killed,
        /// <summary>
        /// Unable to terminate service, possibly due to permissions
        /// </summary>
        UnableToKill
    }
}