using System;

namespace PeanutButter.Args.Attributes
{
    public abstract class StringAttribute : Attribute
    {
        public string Value { get; }

        protected StringAttribute(string value)
        {
            Value = value;
        }
    }
}