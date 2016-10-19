using System;

namespace PeanutButter.DuckTyping.Exceptions
{
    public class MethodNotFoundException : NotImplementedException
    {
        public MethodNotFoundException(Type owningType, string methodName)
            : base($"Public method {methodName} not found on type {owningType.Name}")
        {
        }
    }
}