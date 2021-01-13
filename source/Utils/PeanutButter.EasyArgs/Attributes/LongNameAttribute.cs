using PeanutButter.Utils;

namespace PeanutButter.Args.Attributes
{
    /// <summary>
    /// Explicitly sets the long name for a parsed argument
    /// If you had an option property like "RemoteServer",
    /// the default long name would be "remote-server", which could
    /// be reached on the cli via "--remote-server". Override this here.
    /// </summary>
    public class LongNameAttribute : StringAttribute
    {
        /// <inheritdoc />
        public LongNameAttribute(string value) : base(value.RegexReplace("^--", ""))
        {
        }
    }
}