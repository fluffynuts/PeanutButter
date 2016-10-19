using System;

namespace PeanutButter.DuckTyping.Exceptions
{
    public class WriteOnlyPropertyException: NotImplementedException
    {
        public WriteOnlyPropertyException(Type owningType, string propertyName)
            : base($"Property {propertyName} on type {owningType.Name} is read-only")
        {
        }
    }
}