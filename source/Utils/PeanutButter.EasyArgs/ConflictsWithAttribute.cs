using PeanutButter.EasyArgs.Attributes;

#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs
#else
namespace PeanutButter.EasyArgs
#endif
{
    /// <summary>
    /// Marks this argument as conflicting with another argument by property name (key)
    /// </summary>
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
    internal
#else
    public
#endif
        class ConflictsWithAttribute : StringAttribute
    {
        /// <inheritdoc />
        public ConflictsWithAttribute(string value) : base(value)
        {
        }
    }
}