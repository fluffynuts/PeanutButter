using System;
// ReSharper disable ClassNeverInstantiated.Global

namespace PeanutButter.EasyArgs.Attributes
{
    /// <summary>
    /// Verify that the provided path is an existing file
    /// </summary>
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
    internal
#else
    public
#endif
        class ExistingFileAttribute: Attribute
    {
    }
}