using PeanutButter.Args.Attributes;

namespace PeanutButter.Args
{
    /// <summary>
    /// Marks this argument as conflicting with another argument by property name (key)
    /// </summary>
    public class ConflictsWithAttribute : StringAttribute
    {
        /// <inheritdoc />
        public ConflictsWithAttribute(string value) : base(value)
        {
        }
    }
}