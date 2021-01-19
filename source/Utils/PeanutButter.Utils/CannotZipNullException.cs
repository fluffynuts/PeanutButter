using System;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Thrown when an attempt is made to strict-zip null and anything else
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class CannotZipNullException : Exception
    {
        /// <inheritdoc />
        public CannotZipNullException() : base("Cannot zip null value")
        {
        }
    }
}