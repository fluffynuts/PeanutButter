using System;
using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global

namespace PeanutButter.Async.Interfaces
{
    public interface IFluentTaskRunnerContinuation<T>
    {
        Task ContinueWith(Action<Task> next);
        Task<T2> ContinueWith<T2>(Func<Task<T>, T2> next);
    }
}