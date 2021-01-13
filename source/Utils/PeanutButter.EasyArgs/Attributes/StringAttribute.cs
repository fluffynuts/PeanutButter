using System;

namespace PeanutButter.Args.Attributes
{
    /// <summary>
    /// Stores an arbitrary string
    /// </summary>
    public abstract class StringAttribute : Attribute
    {
        /// <summary>
        /// Stored value
        /// </summary>
        public string Value { get; }

        /// <inheritdoc />
        protected StringAttribute(string value)
        {
            Value = value;
        }
    }
}