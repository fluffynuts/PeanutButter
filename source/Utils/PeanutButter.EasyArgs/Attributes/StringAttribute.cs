using System;

#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs.Attributes
#else
namespace PeanutButter.EasyArgs.Attributes
#endif
{
    /// <summary>
    /// Stores an arbitrary string
    /// </summary>
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
    internal
#else
    public
#endif
        abstract class StringAttribute : Attribute
    {
        /// <summary>
        /// Stored value
        /// </summary>
        public string Value { get; }

        /// <inheritdoc />
        protected StringAttribute(string value)
        {
            Value = value;
        }
    }
}