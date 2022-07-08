using System;

namespace PeanutButter.TestUtils.AspNetCore.Fakes
{
    internal class NonFunctionalServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            throw new ServiceProviderImplementationRequiredException();
        }
    }
}