using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Describes a worker
    /// </summary>
    public interface IWorker
    {
        /// <summary>
        /// Will be true whilst the worker is busy
        /// </summary>
        bool Busy { get; }
        
        /// <summary>
        /// Indicates the default MaxDegreeOfParallelism for ParallelWorker
        /// (should default to the number of available cores, on the assumption
        /// that workloads are more often cpu-bound than I/O-bound).
        /// </summary>
        public int DefaultMaxDegreeOfParallelism { get; set; }
    }

    /// <summary>
    /// Describes a parallel worker for actions
    /// </summary>
    public interface IParallelWorker: IWorker
    {
        /// <summary>
        /// Add some workers to the queue. If the work is already running,
        /// they will be added to the end and executed when the queue is complete
        /// </summary>
        /// <param name="workers"></param>
        void AddWorkers(IEnumerable<Action> workers);

        /// <summary>
        /// Add a single worker to the queue. If the work is already running,
        /// it will be added to the end and executed when the queue is complete
        /// </summary>
        /// <param name="worker"></param>
        void AddWorker(Action worker);

        /// <summary>
        /// Run all the queued work with the default max degree of parallelism
        /// </summary>
        /// <returns></returns>
        Exception[] RunAll();

        /// <summary>
        /// Run all the queued work with the provided max degree of parallelism
        /// </summary>
        /// <returns></returns>
        Exception[] RunAll(int maxDegreeOfParallelism);
    }

    /// <summary>
    /// Encapsulates some work which must be run in parallel
    /// </summary>
    public class ParallelWorker : Worker, IParallelWorker
    {
        /// <summary>
        /// Will be true whilst the worker is busy
        /// </summary>
        public bool Busy => _actual.Busy;

        private readonly ParallelWorker<int> _actual;

        /// <summary>
        /// Provide some initial work
        /// </summary>
        /// <param name="workers"></param>
        public ParallelWorker(params Action[] workers)
        {
            _actual = new ParallelWorker<int>();
            AddWorkers(workers);
        }

        /// <summary>
        /// Add some workers to the queue. If the work is already running,
        /// they will be added to the end and executed when the queue is complete
        /// </summary>
        /// <param name="workers"></param>
        public void AddWorkers(IEnumerable<Action> workers)
        {
            _actual.AddWorkers(Wrap(workers));
        }

        /// <summary>
        /// Run all the queued work with the default max degree of parallelism
        /// </summary>
        /// <returns></returns>
        public Exception[] RunAll()
        {
            return RunAll(DefaultMaxDegreeOfParallelism);
        }

        /// <summary>
        /// Run all the queued work with the provided max degree of parallelism
        /// </summary>
        /// <returns></returns>
        public Exception[] RunAll(int maxDegreeOfParallelism)
        {
            return _actual.RunAll(maxDegreeOfParallelism).Errors();
        }

        /// <summary>
        /// Add a single worker to the queue. If the work is already running,
        /// it will be added to the end and executed when the queue is complete
        /// </summary>
        /// <param name="worker"></param>
        public void AddWorker(Action worker)
        {
            _actual.AddWorker(Wrap(worker));
        }

        private IEnumerable<Func<int>> Wrap(IEnumerable<Action> workers)
        {
            foreach (var w in workers)
            {
                yield return Wrap(w);
            }
        }

        private Func<int> Wrap(Action action)
        {
            return () =>
            {
                action();
                return 0;
            };
        }
    }

    /// <summary>
    /// Describes a parallel worker for functions returning values
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IParallelWorker<T>
    {
        /// <summary>
        /// Will be true whilst the worker is busy
        /// </summary>
        bool Busy { get; }

        /// <summary>
        /// Add a single worker
        /// </summary>
        /// <param name="worker"></param>
        void AddWorker(Func<T> worker);

        /// <summary>
        /// Add a bunch of workers
        /// </summary>
        /// <param name="workers"></param>
        void AddWorkers(IEnumerable<Func<T>> workers);

        /// <summary>
        /// Run all the queued work with the default max degree of parallelism
        /// </summary>
        /// <returns></returns>
        WorkResult<T>[] RunAll();

        /// <summary>
        /// Run all the queued work with the provided max degree of parallelism
        /// </summary>
        /// <param name="maxDegreeOfParallelism"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        WorkResult<T>[] RunAll(int maxDegreeOfParallelism);
    }

    /// <summary>
    /// Encapsulates some work which must be run in parallel
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ParallelWorker<T> : Worker, IParallelWorker<T>
    {
        /// <summary>
        /// Will be true whilst the worker is busy
        /// </summary>
        public bool Busy { get; private set; }

        private readonly List<Func<T>> _work = new();
        private readonly ConcurrentBag<Func<T>> _deferredWork = new();
        private readonly SemaphoreSlim _runLock = new(1);

        /// <summary>
        /// Provide some workers up-front
        /// </summary>
        /// <param name="workers"></param>
        public ParallelWorker(params Func<T>[] workers)
        {
            AddWorkers(workers);
        }

        /// <summary>
        /// Add a single worker
        /// </summary>
        /// <param name="worker"></param>
        public void AddWorker(Func<T> worker)
        {
            AddWorkers(new[] { worker });
        }

        /// <summary>
        /// Add a bunch of workers
        /// </summary>
        /// <param name="workers"></param>
        public void AddWorkers(IEnumerable<Func<T>> workers)
        {
            if (_runLock.Wait(0))
            {
                _work.AddRange(workers);
                _runLock.Release();
                return;
            }

            ScheduleDeferred(workers);
        }

        private void ScheduleDeferred(IEnumerable<Func<T>> workers)
        {
            foreach (var func in workers)
            {
                _deferredWork.Add(func);
            }
        }

        /// <summary>
        /// Run all the queued work with the default max degree of parallelism
        /// </summary>
        /// <returns></returns>
        public WorkResult<T>[] RunAll()
        {
            return RunAll(DefaultMaxDegreeOfParallelism);
        }

        /// <summary>
        /// Run all the queued work with the provided max degree of parallelism
        /// </summary>
        /// <param name="maxDegreeOfParallelism"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public WorkResult<T>[] RunAll(int maxDegreeOfParallelism)
        {
            if (!_runLock.Wait(0))
            {
                throw new InvalidOperationException(
                    "This ParallelWorker is currently busy"
                );
            }

            Busy = true;
            try
            {
                return RunAll(maxDegreeOfParallelism, _work.ToArray());
            }
            finally
            {
                _runLock.Release();
            }
        }

        private WorkResult<T>[] RunAll(
            int maxDegreeOfParallelism,
            Func<T>[] toRun
        )
        {
            var results = new List<WorkResult<T>>();
            var blocker = new SemaphoreSlim(maxDegreeOfParallelism);
            var threads = toRun.Select(a =>
            {
                var t = new Thread(() =>
                {
                    using var _ = new AutoLocker(blocker);
                    try
                    {
                        results.Add(new WorkResult<T>(a()));
                    }
                    catch (Exception ex)
                    {
                        results.Add(new WorkResult<T>(ex));
                    }
                });
                t.Start();
                return t;
            }).ToArray();

            foreach (var t in threads)
            {
                t.Join();
            }

            var deferred = new List<Func<T>>();
            while (_deferredWork.TryTake(out var d))
            {
                deferred.Add(d);
            }

            var deferredResults = deferred.Any()
                ? RunAll(maxDegreeOfParallelism, deferred.ToArray())
                : Array.Empty<WorkResult<T>>();

            results.AddRange(deferredResults);
            return results.ToArray();
        }
    }
}