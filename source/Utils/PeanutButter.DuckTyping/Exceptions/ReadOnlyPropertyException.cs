using System;

namespace PeanutButter.DuckTyping.Exceptions
{
    public class ReadOnlyPropertyException: NotImplementedException
    {
        public ReadOnlyPropertyException(Type owningType, string propertyName)
            : base($"Property {propertyName} on type {owningType.Name} is read-only")
        {
        }
    }
}