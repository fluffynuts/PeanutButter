#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs.Attributes
#else
namespace PeanutButter.EasyArgs.Attributes
#endif
{
    /// <summary>
    /// Explicitly sets the short name for a parsed argument
    /// If you had an option property like "RemoteServer",
    /// the default long name would be "r", which could
    /// be reached on the cli via "-r" or "-R", depending
    /// on if another property starting with "r" was found before
    /// RemoteServer. Override this here.
    /// </summary>
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
    internal
#else
    public
#endif
        class ShortNameAttribute : StringAttribute
    {
        /// <inheritdoc />
        public ShortNameAttribute(char name) : base(name.ToString())
        {
        }
    }
}