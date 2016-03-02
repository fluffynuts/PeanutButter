using System;

namespace PeanutButter.Utils
{
    public class PropertyNotFoundException : Exception
    {
        public PropertyNotFoundException(Type type, string propertyName):
            base("Property '" + propertyName + "' not found on type '" + type.Name + "'")
        {
        }
    }
}