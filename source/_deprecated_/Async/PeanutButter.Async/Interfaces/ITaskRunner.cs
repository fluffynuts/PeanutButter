using System;
using System.Threading;
using System.Threading.Tasks;

namespace PeanutButter.Async.Interfaces
{
    /// <summary>
    /// Provides a wrapper around task running
    /// </summary>
    public interface ITaskRunner
    {
        /// <summary>
        /// Runs an action async
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        Task Run(Action action);
        
        /// <summary>
        /// Runs a func async and returns the task
        /// </summary>
        /// <param name="func"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> Run<T>(Func<T> func);
        
        /// <summary>
        /// Runs the action as LongRunning
        /// </summary>
        /// <param name="action"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task RunLong(Action action, CancellationToken? token = null);
        
        /// <summary>
        /// Creates a suspended task for the provided func
        /// </summary>
        /// <param name="func"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<T> CreateNotStartedFor<T>(Func<T> func, CancellationToken? cancellationToken = null);
        
        /// <summary>
        /// Creates a suspended task for the provided action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task CreateNotStartedFor(Action action, CancellationToken? cancellationToken = null);
        
        /// <summary>
        /// Continues the provided task
        /// </summary>
        /// <param name="task1"></param>
        /// <returns></returns>
        IContinuation Continue(Task task1);
        
        /// <summary>
        /// Continues the provided task
        /// </summary>
        /// <param name="initial"></param>
        /// <returns></returns>
        IContinuation<T> Continue<T>(Task<T> initial);
    }
}