using System;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Exception thrown when a property cannot be found by name
    /// </summary>
    public class PropertyNotFoundException : Exception
    {
        /// <summary>
        /// Constructs a new PropertyNotFoundException
        /// </summary>
        /// <param name="type">The Type being searched for the property</param>
        /// <param name="propertyName">The name of the property which was not found</param>
        public PropertyNotFoundException(Type type, string propertyName):
            base("Property '" + propertyName + "' not found on type '" + type.Name + "'")
        {
        }
    }
}