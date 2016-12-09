using System;

namespace PeanutButter.DuckTyping.Exceptions
{
    /// <summary>
    /// Exception which is thrown when a type does not contain the expected backing field
    /// </summary>
    public class BackingFieldForPropertyNotFoundException : NotImplementedException
    {
        /// <summary>
        /// Constructs an instance of the exception
        /// </summary>
        /// <param name="owningType">Type which owns the property which should have a corresponding backing field</param>
        /// <param name="propertyName">Name of the property which should be backed by a field with the same name and leading underscore</param>
        public BackingFieldForPropertyNotFoundException(Type owningType, string propertyName)
            : base($"Unable to find backing field _{propertyName} on type {owningType.Name}")
        {
        }
    }
}