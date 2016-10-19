using System;

namespace PeanutButter.DuckTyping.Exceptions
{
    public class PropertyNotFoundException : NotImplementedException
    {
        public PropertyNotFoundException(Type owningType, string propertyName)
            : base($"Public property {propertyName} not found on type {owningType.Name}")
        {
        }
    }
}