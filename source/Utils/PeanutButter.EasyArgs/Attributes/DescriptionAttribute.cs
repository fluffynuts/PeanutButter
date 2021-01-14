namespace PeanutButter.EasyArgs.Attributes
{
    /// <summary>
    /// Sets the description for a parsed argument or the header help
    /// text for the group of parsed arguments
    /// </summary>
    public class DescriptionAttribute : StringAttribute
    {
        /// <inheritdoc />
        public DescriptionAttribute(string value) : base(value)
        {
        }
    }
}