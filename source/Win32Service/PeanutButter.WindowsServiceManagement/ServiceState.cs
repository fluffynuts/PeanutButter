namespace PeanutButter.WindowsServiceManagement
{
    /// <summary>
    /// Possible run-states for services
    /// </summary>
    public enum ServiceState
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = -1, // The state cannot be (has not been) retrieved.
        /// <summary>
        /// Service not found
        /// </summary>
        NotFound = 0, // The service is not known on the host server.
        /// <summary>
        /// Service is stopped
        /// </summary>
        Stopped = 1,
        /// <summary>
        /// Service is starting up
        /// </summary>
        StartPending = 2,
        /// <summary>
        /// Service is stopping
        /// </summary>
        StopPending = 3,
        /// <summary>
        /// Service is running
        /// </summary>
        Running = 4,
        /// <summary>
        /// Service is continuing
        /// </summary>
        ContinuePending = 5,
        /// <summary>
        /// Service is pausing
        /// </summary>
        PausePending = 6,
        /// <summary>
        /// Service is paused
        /// </summary>
        Paused = 7
    }
}