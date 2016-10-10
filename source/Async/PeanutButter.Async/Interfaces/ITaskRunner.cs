using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeanutButter.Async.Interfaces
{
    public interface ITaskRunner
    {
        Task Run(Action action);
        Task<T> Run<T>(Func<T> func);
        Task RunLong(Action action, CancellationToken? token = null);
        Task<T> CreateNotStartedFor<T>(Func<T> func, CancellationToken? cancellationToken = null);
        Task CreateNotStartedFor(Action action, CancellationToken? cancellationToken = null);
        IContinuation Continue(Task task1);
        IContinuation<T> Continue<T>(Task<T> initial);
    }
}