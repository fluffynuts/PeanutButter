using System;

namespace PeanutButter.DuckTyping.Exceptions
{
    /// <summary>
    /// Exception thrown when a forced ducked type does not implement
    /// a method that it is expected to implement
    /// </summary>
    public class MethodNotFoundException : NotImplementedException
    {
        /// <summary>
        /// Constructs an instance of the exception
        /// </summary>
        /// <param name="owningType">Type which was expected to implement the method</param>
        /// <param name="methodName">Name of the method which was expected, but not found</param>
        public MethodNotFoundException(
            Type owningType,
            string methodName
        )
            : base($"Public method {methodName} not found on type {owningType.Name}")
        {
        }
    }
}