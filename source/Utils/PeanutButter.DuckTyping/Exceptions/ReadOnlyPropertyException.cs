using System;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.Exceptions
#else
namespace PeanutButter.DuckTyping.Exceptions
#endif
{
    /// <summary>
    /// Exception thrown when the ducked property is read-only but the interface
    /// to duck to expects a read/write property
    /// </summary>
#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
    internal
#else
    public
#endif
    class ReadOnlyPropertyException: NotImplementedException
    {
        /// <summary>
        /// Constructs an instance of the exception
        /// </summary>
        /// <param name="owningType">Type implementing read-only property</param>
        /// <param name="propertyName">Name of the property which was expected to be writable</param>
        public ReadOnlyPropertyException(Type owningType, string propertyName)
            : base($"Property {propertyName} on type {owningType.Name} is read-only")
        {
        }
    }
}