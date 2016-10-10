using System;
using System.Threading.Tasks;
using PeanutButter.Async.Interfaces;

namespace PeanutButter.Async
{
    public class FluentTaskRunnerContinuation<T> : IFluentTaskRunnerContinuation<T>
    {
        private readonly ITaskRunner _taskRunner;
        private readonly Task _initialWithoutResult;
        private readonly Task<T> _initialWithResult;

        public FluentTaskRunnerContinuation(Task initialWithoutResult, ITaskRunner taskRunner)
        {
            _taskRunner = taskRunner;
            _initialWithoutResult = initialWithoutResult;
        }

        public FluentTaskRunnerContinuation(Task<T> initial, ITaskRunner taskRunner)
        {
            _taskRunner = taskRunner;
            _initialWithResult = initial;
        }

        public Task ContinueWith(Action<Task> next)
        {
            return _taskRunner.Continue(_initialWithoutResult).With(next);
        }

        public Task<T2> ContinueWith<T2>(Func<Task<T>, T2> next)
        {
            return _taskRunner.Continue(_initialWithResult).With(next);
        }
    }
}
