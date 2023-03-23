#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// What to do when one or more errors are encountered
    /// in a collection of WorkResults
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        enum ErrorStrategies
    {
        /// <summary>
        /// Throw all errors in an AggregateException
        /// </summary>
        Throw,

        /// <summary>
        /// Suppress the errors - just return the results
        /// </summary>
        Suppress
    }
}