namespace PeanutButter.WindowsServiceManagement.Exceptions
{
    /// <summary>
    /// Thrown when the service the caller wants to interact with cannot be
    /// found installed on the current machine
    /// </summary>
    public class ServiceNotInstalledException : WindowsServiceUtilException
    {
        /// <inheritdoc />
        public ServiceNotInstalledException(
            string serviceName,
            string moreInfo = null
        )
            : base($"{serviceName} is not installed{(moreInfo is null ? "" : $": {moreInfo}")}")
        {
        }
    }
}