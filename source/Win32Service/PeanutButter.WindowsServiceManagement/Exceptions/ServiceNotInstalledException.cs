namespace PeanutButter.Win32ServiceControl.Exceptions
{
    public class ServiceNotInstalledException : WindowsServiceUtilException
    {
        public ServiceNotInstalledException(string serviceName)
            : base(serviceName + " is not installed")
        {
        }
    }
}