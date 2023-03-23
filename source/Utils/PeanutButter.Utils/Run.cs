using System;

// ReSharper disable UnusedMember.Global
// ReSharper disable once UnusedType.Global

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides convenience wrapper functions to run small bits of work
    /// in parallel threads, not tasks. No async/await, no Tasks (unless
    /// you want to return them and await them yourself). No TPL. No
    /// handbrakes. No magic context. No guarantees, except that your
    /// work will be executed. May make the host machine very tired.
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class Run
    {
        /// <summary>
        /// Run all actions in parallel, returning any captured exceptions
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="more"></param>
        public static Exception[] InParallel(
            Action first,
            Action second,
            params Action[] more
        )
        {
            return CreateWorkerFor(
                first,
                second,
                more
            ).RunAll();
        }

        /// <summary>
        /// Run all actions in parallel, with the provided max degree of
        /// parallelism, returning any captured exceptions
        /// </summary>
        /// <param name="maxDegreeOfParallelism"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="more"></param>
        public static Exception[] InParallel(
            int maxDegreeOfParallelism,
            Action first,
            Action second,
            params Action[] more
        )
        {
            return CreateWorkerFor(
                first,
                second,
                more
            ).RunAll(maxDegreeOfParallelism);
        }

        /// <summary>
        /// Run all functions in parallel, returning all results / exceptions
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="more"></param>
        public static WorkResult<T>[] InParallel<T>(
            Func<T> first,
            Func<T> second,
            params Func<T>[] more
        )
        {
            return CreateWorkerFor(
                first,
                second,
                more
            ).RunAll();
        }

        /// <summary>
        /// Run all provided functions with the max degree of parallelism,
        /// returning all results / exceptions
        /// </summary>
        /// <param name="maxDegreeOfParallelism"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="more"></param>
        public static WorkResult<T>[] InParallel<T>(
            int maxDegreeOfParallelism,
            Func<T> first,
            Func<T> second,
            params Func<T>[] more
        )
        {
            return CreateWorkerFor(
                first,
                second,
                more
            ).RunAll(maxDegreeOfParallelism);
        }

        private static ParallelWorker<T> CreateWorkerFor<T>(
            Func<T> first,
            Func<T> second,
            params Func<T>[] more
        )
        {
            var worker = new ParallelWorker<T>();
            worker.AddWorker(first);
            worker.AddWorker(second);
            worker.AddWorkers(more);
            return worker;
        }

        private static ParallelWorker CreateWorkerFor(
            Action first,
            Action second,
            params Action[] more
        )
        {
            var worker = new ParallelWorker();
            worker.AddWorker(first);
            worker.AddWorker(second);
            worker.AddWorkers(more);
            return worker;
        }
    }
}