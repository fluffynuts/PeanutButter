using System;

#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs.Attributes
#else
namespace PeanutButter.EasyArgs.Attributes
#endif
{
    /// <summary>
    /// Stores an arbitrary object
    /// </summary>
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
    internal
#else
    public
#endif
        abstract class ObjectAttribute : Attribute
    {
        /// <summary>
        /// Stored value
        /// </summary>
        public object Value { get; }

        /// <inheritdoc />
        protected ObjectAttribute(object value)
        {
            Value = value;
        }
    }
}