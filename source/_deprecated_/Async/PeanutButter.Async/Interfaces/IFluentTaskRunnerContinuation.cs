using System;
using System.Threading.Tasks;
// ReSharper disable UnusedMember.Global

namespace PeanutButter.Async.Interfaces
{
    /// <summary>
    /// Provides fluent continuation syntax
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IFluentTaskRunnerContinuation<T>
    {
        /// <summary>
        /// Continue with the provided Action which returns a task
        /// </summary>
        /// <param name="next"></param>
        /// <returns></returns>
        Task ContinueWith(Action<Task> next);
        /// <summary>
        /// Continue with the provided func
        /// </summary>
        /// <param name="next"></param>
        /// <typeparam name="T2"></typeparam>
        /// <returns></returns>
        Task<T2> ContinueWith<T2>(Func<Task<T>, T2> next);
    }
}