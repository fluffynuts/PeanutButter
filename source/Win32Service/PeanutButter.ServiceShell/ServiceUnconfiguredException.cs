using System;

namespace PeanutButter.ServiceShell
{
    public class ServiceUnconfiguredException : Exception
    {
        public ServiceUnconfiguredException(string property)
            : base("This service is not completely configured. Please set the " + property + " property value.")
        {
        }
    }
}