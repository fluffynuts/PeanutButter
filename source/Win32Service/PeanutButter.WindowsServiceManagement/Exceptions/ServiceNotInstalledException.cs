namespace PeanutButter.WindowsServiceManagement.Exceptions
{
    public class ServiceNotInstalledException : WindowsServiceUtilException
    {
        public ServiceNotInstalledException(
            string serviceName,
            string moreInfo = null
        )
            : base($"{serviceName} is not installed{(moreInfo is null ? "" : $": {moreInfo}")}")
        {
        }
    }
}