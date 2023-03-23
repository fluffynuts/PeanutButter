using System;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <inheritdoc />
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
    class WorkResult<T> : WorkResult
    {
        /// <summary>
        /// Result of the work, if available
        /// </summary>
        public T Result { get; }

        internal WorkResult(
            T result
        ) : base(null)
        {
            Result = result;
        }

        internal WorkResult(Exception ex) : base(ex)
        {
        }
    }

    /// <summary>
    /// Stores the exception (or none) from a single piece of work
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
    class WorkResult
    {
        /// <summary>
        /// Exception thrown by the work, or null if no exception thrown
        /// </summary>
        public Exception Exception { get; }

        internal WorkResult(Exception ex)
        {
            Exception = ex;
        }
    }
}