using System;

namespace PeanutButter.TestUtils.AspNetCore.Fakes;

/// <summary>
/// Thrown when unable to cast up to the required fake
/// </summary>
public class ServiceProviderImplementationRequiredException : Exception
{
    /// <inheritdoc />
    public ServiceProviderImplementationRequiredException()
        : base($"A functional service provider was not provided. Please set on the HttpContext via RequestServices. A {nameof(MinimalServiceProvider)} is provided if you have very simple requirements.")
    {
    }
}