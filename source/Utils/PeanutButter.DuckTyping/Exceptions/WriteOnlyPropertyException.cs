using System;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.Exceptions
#else
namespace PeanutButter.DuckTyping.Exceptions
#endif
{
    /// <summary>
    /// Exception thrown when a ducking operation expects a read/write
    /// property but the object being ducked implements a write-only property
    /// </summary>
#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
    internal
#else
    public
#endif
    class WriteOnlyPropertyException: NotImplementedException
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