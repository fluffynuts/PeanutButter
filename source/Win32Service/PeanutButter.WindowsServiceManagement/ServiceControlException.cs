using System;

namespace PeanutButter.WindowsServiceManagement
{
    /// <summary>
    /// Thrown when a service control request (eg start/stop/pause/continue) cannot
    /// be completed
    /// </summary>
    public class ServiceControlException : Exception
    {
        /// <summary>
        /// Constructs the exception
        /// </summary>
        /// <param name="message"></param>
        public ServiceControlException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructs the exception
        /// </summary>
        /// <param name="message"></param>
        /// <param name="fullServiceControlCommandline"></param>
        public ServiceControlException(
            string message,
            string fullServiceControlCommandline
        ) : base($"{message}\ncli: {fullServiceControlCommandline}")
        {
        }
    }
}