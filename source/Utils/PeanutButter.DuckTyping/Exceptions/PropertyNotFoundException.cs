using System;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.Exceptions
#else
namespace PeanutButter.DuckTyping.Exceptions
#endif
{
    /// <summary>
    /// Exception thrown when an expected property was not found on a ducked type
    /// </summary>
#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
    internal
#else
    public
#endif
    class PropertyNotFoundException : NotImplementedException
    {
        /// <summary>
        /// Constructs a new instance of the exception
        /// </summary>
        /// <param name="owningType">Name of the type expected to implement the property</param>
        /// <param name="propertyName">Name of the missing expected property</param>
        public PropertyNotFoundException(Type owningType, string propertyName)
            : base($"Public property {propertyName} not found on type {owningType.Name}")
        {
        }
    }
}