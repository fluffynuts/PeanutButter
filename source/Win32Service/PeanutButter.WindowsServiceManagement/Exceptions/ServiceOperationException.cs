namespace PeanutButter.WindowsServiceManagement.Exceptions
{
    /// <summary>
    /// Thrown when a requested service operation (eg start, stop)
    /// cannot be completed
    /// </summary>
    public class ServiceOperationException : WindowsServiceUtilException
    {
        /// <inheritdoc />
        public ServiceOperationException(string serviceName, string operation, string info)
            : base(
                $@"Unable to perform {
                        (operation ?? "(null)")
                    } on service {
                        (serviceName ?? "(null)")
                    }: {
                        (info ?? "(null)")
                    }")
        {
        }
    }
}