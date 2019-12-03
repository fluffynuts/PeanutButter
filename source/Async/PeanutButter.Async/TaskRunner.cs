using System;
using System.Threading;
using System.Threading.Tasks;
using PeanutButter.Async.Interfaces;

namespace PeanutButter.Async
{
    /// <inheritdoc />
    public class TaskRunner : ITaskRunner
    {
        /// <inheritdoc />
        public Task Run(Action action)
        {
            return Task.Run(action);
        }

        /// <inheritdoc />
        public Task<T> Run<T>(Func<T> func)
        {
            return Task.Run(func);
        }

        /// <inheritdoc />
        public Task RunLong(Action action, CancellationToken? token = null)
        {
            return Task.Factory.StartNew(action,
                token ?? CancellationToken.None,
                TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach,
                TaskScheduler.Default);
        }

        /// <inheritdoc />
        public Task<T> CreateNotStartedFor<T>(Func<T> func, CancellationToken? cancellationToken = null)
        {
            return new Task<T>(func, cancellationToken ?? CancellationToken.None);
        }

        /// <inheritdoc />
        public Task CreateNotStartedFor(Action action, CancellationToken? cancellationToken = null)
        {
            return new Task(action, cancellationToken ?? CancellationToken.None);
        }

        /// <inheritdoc />
        public IContinuation Continue(Task task1)
        {
            return new Continuation<object>(task1);
        }

        /// <inheritdoc />
        public IContinuation<T> Continue<T>(Task<T> initial)
        {
            return new Continuation<T>(initial);
        }
    }
}
