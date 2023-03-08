using System;

namespace PeanutButter.ServiceShell
{
    /// <summary>
    /// Thrown when a service was not completely configured
    /// </summary>
    public class ServiceUnconfiguredException : Exception
    {
        /// <inheritdoc />
        public ServiceUnconfiguredException(string property)
            : base($"This service is not completely configured. Please set the {property} property value.")
        {
        }
    }
}