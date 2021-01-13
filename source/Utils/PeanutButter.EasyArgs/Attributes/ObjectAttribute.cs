using System;

namespace PeanutButter.Args.Attributes
{
    public abstract class ObjectAttribute : Attribute
    {
        public object Value { get; }

        protected ObjectAttribute(object value)
        {
            Value = value;
        }
    }
}