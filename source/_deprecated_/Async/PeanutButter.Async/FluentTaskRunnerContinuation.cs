using System;
using System.Threading.Tasks;
using PeanutButter.Async.Interfaces;

namespace PeanutButter.Async
{
    /// <inheritdoc />
    public class FluentTaskRunnerContinuation<T> : IFluentTaskRunnerContinuation<T>
    {
        private readonly ITaskRunner _taskRunner;
        private readonly Task _initialWithoutResult;
        private readonly Task<T> _initialWithResult;

        /// <summary>
        /// Creates the continuation with the provided runner and task
        /// </summary>
        /// <param name="initialWithoutResult"></param>
        /// <param name="taskRunner"></param>
        public FluentTaskRunnerContinuation(Task initialWithoutResult, ITaskRunner taskRunner)
        {
            _taskRunner = taskRunner;
            _initialWithoutResult = initialWithoutResult;
        }

        /// <summary>
        /// Creates the continuation with the provided runner and task
        /// </summary>
        /// <param name="initial"></param>
        /// <param name="taskRunner"></param>
        public FluentTaskRunnerContinuation(Task<T> initial, ITaskRunner taskRunner)
        {
            _taskRunner = taskRunner;
            _initialWithResult = initial;
        }

        /// <summary>
        /// Continues with the provide action
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        public Task ContinueWith(Action<Task> next)
        {
            return _taskRunner.Continue(_initialWithoutResult).With(next);
        }

        /// <summary>
        /// Continues with the provided func
        /// </summary>
        /// <param name="next"></param>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        public Task<T2> ContinueWith<T2>(Func<Task<T>, T2> next)
        {
            return _taskRunner.Continue(_initialWithResult).With(next);
        }
    }
}
