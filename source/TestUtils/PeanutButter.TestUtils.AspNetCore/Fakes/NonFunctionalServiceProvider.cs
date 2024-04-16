using System;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
#else
namespace PeanutButter.TestUtils.AspNetCore.Fakes;
#endif

internal class NonFunctionalServiceProvider : IServiceProvider, IFake
{
    public object GetService(Type serviceType)
    {
        throw new ServiceProviderImplementationRequiredException();
    }
}