using System;

namespace PeanutButter.Utils
{
    /// <summary>
    /// base class for parallel workers
    /// </summary>
    public abstract class Worker
    {
        /// <summary>
        /// Indicates the default MaxDegreeOfParallelism for ParallelWorker
        /// (should default to the number of available cores, on the assumption
        /// that workloads are more often cpu-bound than I/O-bound).
        /// </summary>
        public int DefaultMaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;
    }
}