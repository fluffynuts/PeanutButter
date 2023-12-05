using System;

#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs.Attributes;
#else
namespace PeanutButter.EasyArgs.Attributes;
#endif
/// <summary>
/// Decorate a property with this to specify a minimum value for the property
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
internal
#else
public
#endif
    class MaxAttribute : NumericAttribute
{
    /// <inheritdoc />
    public MaxAttribute(decimal min) : base(min)
    {
    }

    /// <inheritdoc />
    public MaxAttribute(long min) : base(min)
    {
    }
}