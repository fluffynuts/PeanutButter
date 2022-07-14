using System;

namespace PeanutButter.TestUtils.AspNetCore;

/// <summary>
/// Thrown when attempting to use an obsolete feature
/// </summary>
public class FeatureIsObsoleteException
    : Exception
{
    /// <inheritdoc />
    public FeatureIsObsoleteException(
        string property
    ) : base($"Feature is obsolete: {property} - as such, there are no fakes generated for it.")
    {
    }
}