using System;
// ReSharper disable ClassNeverInstantiated.Global

#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs.Attributes;
#else
namespace PeanutButter.EasyArgs.Attributes;
#endif
/// <summary>
/// Decorate properties with this to make EasyArgs ignore them
/// eg if you have some configuration that could come from the CLI
/// on an object where other config is loaded, eg, from an ini file
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
internal
#else
public
#endif
    class IgnoreAttribute : Attribute
{
}