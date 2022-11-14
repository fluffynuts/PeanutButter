using System;

namespace PeanutButter.WindowsServiceManagement
{
    public class ServiceControlException : Exception
    {
        public ServiceControlException(string message)
            : base(message)
        {
        }

        public ServiceControlException(
            string message,
            string fullServiceControlCommandline
        ) : base($"{message}\ncli: {fullServiceControlCommandline}")
        {
        }
    }
}