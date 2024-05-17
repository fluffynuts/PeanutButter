
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
using Imported.PeanutButter.EasyArgs.Attributes;
namespace Imported.PeanutButter.EasyArgs
#else
using PeanutButter.EasyArgs.Attributes;
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