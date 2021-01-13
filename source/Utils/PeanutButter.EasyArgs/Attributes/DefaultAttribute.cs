namespace PeanutButter.Args.Attributes
{
    /// <summary>
    /// Sets the default value for a parsed argument
    /// </summary>
    public class DefaultAttribute : ObjectAttribute
    {
        /// <inheritdoc />
        public DefaultAttribute(object value) : base(value)
        {
        }
    }
}