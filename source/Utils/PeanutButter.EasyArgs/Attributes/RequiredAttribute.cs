using System;

#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs.Attributes
#else
namespace PeanutButter.EasyArgs.Attributes
#endif
{
    /// <summary>
    /// Marks an option as required
    /// </summary>
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
    internal
#else
    public
#endif
        class RequiredAttribute : Attribute
    {
    }
}