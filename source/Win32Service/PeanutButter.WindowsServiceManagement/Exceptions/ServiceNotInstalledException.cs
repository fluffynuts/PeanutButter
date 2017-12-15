namespace PeanutButter.WindowsServiceManagement.Exceptions
{
    public class ServiceNotInstalledException : WindowsServiceUtilException
    {
        public ServiceNotInstalledException(string serviceName)
            : base(serviceName + " is not installed")
        {
        }
    }
}