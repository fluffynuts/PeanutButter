using System;

#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs.Attributes;
#else
namespace PeanutButter.EasyArgs.Attributes;
#endif
/// <summary>
/// Sets the description for a parsed argument or the header help
/// text for the group of parsed arguments
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Interface)]
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
    internal
#else
public
#endif
    class DescriptionAttribute : StringAttribute
{
    /// <inheritdoc />
    public DescriptionAttribute(string value) : base(value)
    {
    }
}