namespace PeanutButter.Args.Attributes
{
    /// <summary>
    /// Adds more information to the help screen, as a footer
    /// </summary>
    public class MoreInfoAttribute : StringAttribute
    {
        /// <inheritdoc />
        public MoreInfoAttribute(string value) : base(value)
        {
        }
    }
}