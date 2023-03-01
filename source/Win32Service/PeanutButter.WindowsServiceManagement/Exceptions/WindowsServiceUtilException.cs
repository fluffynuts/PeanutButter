using System;

namespace PeanutButter.WindowsServiceManagement.Exceptions
{
    /// <summary>
    /// The base class from which exceptions in PeanutButter.WindowsServiceManagement
    /// are derived, if you're looking to catch all service-related exceptions
    /// </summary>
    public abstract class WindowsServiceUtilException : Exception
    {
        /// <inheritdoc />
        protected WindowsServiceUtilException(string message)
            : base (message)
        {
        }
    }
}