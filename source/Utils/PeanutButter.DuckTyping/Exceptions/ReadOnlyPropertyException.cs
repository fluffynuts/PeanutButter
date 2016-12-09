using System;

namespace PeanutButter.DuckTyping.Exceptions
{
    /// <summary>
    /// Exception thrown when the ducked property is read-only but the interface
    /// to duck to expects a read/write property
    /// </summary>
    public class ReadOnlyPropertyException: NotImplementedException
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