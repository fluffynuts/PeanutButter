using System;

namespace PeanutButter.Args.Attributes
{
    /// <summary>
    /// Stores an arbitrary object
    /// </summary>
    public abstract class ObjectAttribute : Attribute
    {
        /// <summary>
        /// Stored value
        /// </summary>
        public object Value { get; }

        /// <inheritdoc />
        protected ObjectAttribute(object value)
        {
            Value = value;
        }
    }
}