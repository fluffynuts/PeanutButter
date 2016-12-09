using System;

namespace PeanutButter.DuckTyping.Exceptions
{
    /// <summary>
    /// Exception thrown when a ducking operation expects a read/write
    /// property but the object being ducked implements a write-only property
    /// </summary>
    public class WriteOnlyPropertyException: NotImplementedException
    {
        /// <summary>
        /// Constructs a new instance of the exception
        /// </summary>
        /// <param name="owningType">Underlying type owning the property</param>
        /// <param name="propertyName">Name of the write-only property</param>
        public WriteOnlyPropertyException(Type owningType, string propertyName)
            : base($"Property {propertyName} on type {owningType.Name} is write-only")
        {
        }
    }
}