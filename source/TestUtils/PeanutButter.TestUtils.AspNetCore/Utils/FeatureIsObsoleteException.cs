using System;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Utils;
#else
namespace PeanutButter.TestUtils.AspNetCore.Utils;
#endif

/// <summary>
/// Thrown when attempting to use an obsolete feature
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class FeatureIsObsoleteException
    : Exception
{
    /// <inheritdoc />
    public FeatureIsObsoleteException(
        string property
    ) : base($"Feature is obsolete: {property} - as such, there are no fakes generated for it.")
    {
    }
}