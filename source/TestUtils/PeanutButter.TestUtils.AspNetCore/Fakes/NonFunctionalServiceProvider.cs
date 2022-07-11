using System;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

internal class NonFunctionalServiceProvider : IServiceProvider, IFake
{
    public object GetService(Type serviceType)
    {
        throw new ServiceProviderImplementationRequiredException();
    }
}