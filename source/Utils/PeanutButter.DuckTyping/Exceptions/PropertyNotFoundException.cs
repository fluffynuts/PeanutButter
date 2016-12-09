using System;

namespace PeanutButter.DuckTyping.Exceptions
{
    /// <summary>
    /// Exception thrown when an expected property was not found on a ducked type
    /// </summary>
    public class PropertyNotFoundException : NotImplementedException
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