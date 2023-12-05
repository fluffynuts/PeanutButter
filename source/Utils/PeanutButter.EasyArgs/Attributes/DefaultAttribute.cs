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
    class DefaultAttribute : ObjectAttribute
{
    /// <inheritdoc />
    public DefaultAttribute(object value) : base(value)
    {
    }
}