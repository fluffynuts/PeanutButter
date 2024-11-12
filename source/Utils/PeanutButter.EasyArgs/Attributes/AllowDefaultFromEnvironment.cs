using System;

#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs.Attributes;
#else
namespace PeanutButter.EasyArgs.Attributes;
#endif
/// <summary>
/// Sets the default value for a parsed argument
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
    internal
#else
public
#endif
    class AllowDefaultFromEnvironment : Attribute
{
    /// <summary>
    /// When set, this is the environment variable to
    /// observe for a default value
    /// </summary>
    public string EnvironmentVariable { get; }

    /// <summary>
    /// Allows overriding the default value for a
    /// property or class from environment variables
    /// </summary>
    public AllowDefaultFromEnvironment()
    {
    }

    /// <summary>
    /// Allows overriding the default value for a
    /// property by specifying the 
    /// </summary>
    /// <param name="environmentVariable"></param>
    public AllowDefaultFromEnvironment(
        string environmentVariable
    )
    {
        EnvironmentVariable = environmentVariable;
    }
}

/// <summary>
/// Allows defaults for all options on the options
/// object to be overridden from environment variables
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
    internal
#else
public
#endif
    class AllowDefaultsFromEnvironment : Attribute
{
}
