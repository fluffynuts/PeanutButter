namespace PeanutButter.EasyArgs.Attributes
{
    /// <summary>
    /// Decorate a property with this to specify a minimum value for the property
    /// </summary>
    public class MaxAttribute : NumericAttribute
    {
        /// <inheritdoc />
        public MaxAttribute(decimal min) : base(min)
        {
        }

        /// <inheritdoc />
        public MaxAttribute(long min) : base(min)
        {
        }
    }
}