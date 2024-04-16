using System;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
#else
namespace PeanutButter.TestUtils.AspNetCore.Fakes;
#endif

/// <summary>
/// Thrown when unable to cast up to the required fake
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class ServiceProviderImplementationRequiredException : Exception
{
    /// <inheritdoc />
    public ServiceProviderImplementationRequiredException()
        : base(
            $"A functional service provider was not provided. Please set on the HttpContext via RequestServices. A {nameof(MinimalServiceProvider)} is provided if you have very simple requirements."
        )
    {
    }
}