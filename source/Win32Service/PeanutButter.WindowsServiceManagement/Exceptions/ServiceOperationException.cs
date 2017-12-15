namespace PeanutButter.WindowsServiceManagement.Exceptions
{
    public class ServiceOperationException : WindowsServiceUtilException
    {
        public ServiceOperationException(string serviceName, string operation, string info)
            : base("Unable to perform " + (operation ?? "(null)") + " on service " + (serviceName ?? "(null)") + ": " + (info ?? "(null)"))
        {
        }
    }
}