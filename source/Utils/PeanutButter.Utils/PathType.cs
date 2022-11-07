#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Specifies the kind of path required when joining collections of strings to form a path
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        enum PathType
    {
        /// <summary>
        /// Select the path type for the current platform
        /// </summary>
        Auto,

        /// <summary>
        /// Unix path type, delimited by /
        /// </summary>
        Unix,

        /// <summary>
        /// Windows path type, delimited by \\
        /// </summary>
        Windows
    }
}