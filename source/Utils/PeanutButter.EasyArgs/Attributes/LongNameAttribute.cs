using PeanutButter.Utils;

#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs.Attributes
#else
namespace PeanutButter.EasyArgs.Attributes
#endif
{
    /// <summary>
    /// Explicitly sets the long name for a parsed argument
    /// If you had an option property like "RemoteServer",
    /// the default long name would be "remote-server", which could
    /// be reached on the cli via "--remote-server". Override this here.
    /// </summary>
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
    internal
#else
    public
#endif
        class LongNameAttribute : StringAttribute
    {
        /// <inheritdoc />
        public LongNameAttribute(string value) : base(value.RegexReplace("^--", ""))
        {
        }
    }
}