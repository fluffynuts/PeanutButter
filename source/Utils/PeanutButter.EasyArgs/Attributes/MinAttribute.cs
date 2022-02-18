namespace PeanutButter.EasyArgs.Attributes
{
    /// <summary>
    /// Decorate a property with this to specify a minimum value for the property
    /// </summary>
    public class MinAttribute : NumericAttribute
    {
        /// <inheritdoc />
        public MinAttribute(decimal min) : base(min)
        {
        }

        /// <inheritdoc />
        public MinAttribute(long min) : base(min)
        {
        }
    }
}