using System;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    public class ServiceProviderImplementationRequiredException : Exception
    {
        public ServiceProviderImplementationRequiredException()
            : base("A functional service provider was not provided. Please set on the HttpContext via RequestServices.")
        {
        }
    }
}