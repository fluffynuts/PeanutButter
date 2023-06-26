using System;
using System.Threading.Tasks;
// ReSharper disable UnusedMethodReturnValue.Global

namespace PeanutButter.Async.Interfaces
{
    /// <summary>
    /// Provides a continuation point for another task with no result
    /// </summary>
    public interface IContinuation
    {
        /// <summary>
        /// Provides a continuation point for another task with no result
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        Task With(Action<Task> action);
    }

    /// <summary>
    /// Provides a continuation point for another task with a result
    /// </summary>
    public interface IContinuation<T>
    {
        /// <summary>
        /// Provides a continuation point for another task with the a result
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        Task With(Action<Task<T>> action);
        /// <summary>
        /// Provides a continuation point for another task with the a result
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        Task<TNext> With<TNext>(Func<Task<T>, TNext> func);
    }
}