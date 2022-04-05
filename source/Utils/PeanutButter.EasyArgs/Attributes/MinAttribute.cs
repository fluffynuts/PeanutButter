#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
namespace Imported.PeanutButter.EasyArgs.Attributes
#else
namespace PeanutButter.EasyArgs.Attributes
#endif
{
    /// <summary>
    /// Decorate a property with this to specify a minimum value for the property
    /// </summary>
#if BUILD_PEANUTBUTTER_EASYARGS_INTERNAL
    internal
#else
    public
#endif
        class MinAttribute : NumericAttribute
    {
        /// <inheritdoc />
        public MinAttribute(decimal min) : base(min)
        {
        }

        /// <inheritdoc />
        public MinAttribute(long min) : base(min)
        {
        }
    }
}