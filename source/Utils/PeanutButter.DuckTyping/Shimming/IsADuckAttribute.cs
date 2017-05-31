using System;

namespace PeanutButter.DuckTyping.Shimming
{
    /// <summary>
    /// Attribute added to all types created by the TypeMaker, usually consumed
    /// during efforts to duck-type
    /// </summary>
    public class IsADuckAttribute : Attribute
    {
    }
}