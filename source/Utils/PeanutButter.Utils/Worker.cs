using System;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// base class for parallel workers
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        abstract class Worker
    {
        /// <summary>
        /// Indicates the default MaxDegreeOfParallelism for ParallelWorker
        /// (should default to the number of available cores, on the assumption
        /// that workloads are more often cpu-bound than I/O-bound).
        /// </summary>
        public int DefaultMaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;
    }
}