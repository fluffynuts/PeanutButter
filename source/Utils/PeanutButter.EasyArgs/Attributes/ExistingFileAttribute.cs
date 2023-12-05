using System;

// ReSharper disable ClassNeverInstantiated.Global

#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs.Attributes;
#else
namespace PeanutButter.EasyArgs.Attributes;
#endif
/// <summary>
/// Verify that the provided path is an existing file
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
internal
#else
public
#endif
    class ExistingFileAttribute : Attribute
{
}