using System;

#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs.Attributes;
#else
namespace PeanutButter.EasyArgs.Attributes;
#endif
/// <summary>
/// Adds more information to the help screen, as a footer
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
    internal
#else
public
#endif
    class CopyrightAttribute : StringAttribute
{
    /// <inheritdoc />
    public CopyrightAttribute(string value) : base(value)
    {
    }
}