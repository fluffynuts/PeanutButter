using System;
using System.Threading.Tasks;
// ReSharper disable UnusedMethodReturnValue.Global

namespace PeanutButter.Async.Interfaces
{
    public interface IContinuation
    {
        Task With(Action<Task> action);
    }

    public interface IContinuation<T>
    {
        Task With(Action<Task<T>> action);
        Task<TNext> With<TNext>(Func<Task<T>, TNext> func);
    }
}