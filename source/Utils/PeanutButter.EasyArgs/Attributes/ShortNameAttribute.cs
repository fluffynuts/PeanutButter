namespace PeanutButter.Args.Attributes
{
    /// <summary>
    /// Explicitly sets the short name for a parsed argument
    /// If you had an option property like "RemoteServer",
    /// the default long name would be "r", which could
    /// be reached on the cli via "-r" or "-R", depending
    /// on if another property starting with "r" was found before
    /// RemoteServer. Override this here.
    /// </summary>
    public class ShortNameAttribute : StringAttribute
    {
        /// <inheritdoc />
        public ShortNameAttribute(char name) : base(name.ToString())
        {
        }
    }
}