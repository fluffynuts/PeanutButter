using System;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.Shimming
#else
namespace PeanutButter.DuckTyping.Shimming
#endif
{
    /// <summary>
    /// Attribute added to all types created by the TypeMaker, usually consumed
    /// during efforts to duck-type
    /// </summary>
#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
    internal
#else
    public
#endif
    class IsADuckAttribute : Attribute
    {
    }
}