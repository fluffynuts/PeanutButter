using System;
using System.Threading;
using System.Threading.Tasks;
using PeanutButter.Async.Interfaces;

namespace PeanutButter.Async
{
    public class TaskRunner : ITaskRunner
    {
        public Task Run(Action action)
        {
            return Task.Run(action);
        }

        public Task<T> Run<T>(Func<T> func)
        {
            return Task.Run(func);
        }

        public Task RunLong(Action action, CancellationToken? token = null)
        {
            return Task.Factory.StartNew(action,
                token ?? CancellationToken.None,
                TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach,
                TaskScheduler.Default);
        }

        public Task<T> CreateNotStartedFor<T>(Func<T> func, CancellationToken? cancellationToken = null)
        {
            return new Task<T>(func, cancellationToken ?? CancellationToken.None);
        }

        public Task CreateNotStartedFor(Action action, CancellationToken? cancellationToken = null)
        {
            return new Task(action, cancellationToken ?? CancellationToken.None);
        }

        public IContinuation Continue(Task task1)
        {
            return new Continuation<object>(task1);
        }

        public IContinuation<T> Continue<T>(Task<T> initial)
        {
            return new Continuation<T>(initial);
        }
    }
}
