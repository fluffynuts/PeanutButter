using System;
using System.Threading.Tasks;
using PeanutButter.Async.Interfaces;

namespace PeanutButter.Async
{
    /// <inheritdoc cref="IContinuation"/> />
    public class Continuation<T> : IContinuation<T>, IContinuation
    {
        private readonly Task<T> _taskWithResult;
        private readonly Task _taskWithoutResult;

        /// <summary>
        /// Creates the continuation around the task
        /// </summary>
        /// <param name="taskWithResult"></param>
        public Continuation(Task<T> taskWithResult)
        {
            _taskWithResult = taskWithResult;
        }

        /// <summary>
        /// Creates the continuation around the task
        /// </summary>
        /// <param name="taskWithoutResult"></param>
        public Continuation(Task taskWithoutResult)
        {
            _taskWithoutResult = taskWithoutResult;
        }

        /// <summary>
        /// Continues the continuation around the action
        /// </summary>
        /// <param name="action"></param>
        public Task With(Action<Task> action)
        {
            return Task.Run(() => action(_taskWithResult ?? _taskWithoutResult));
        }

        /// <summary>
        /// Continues the continuation around the action
        /// </summary>
        /// <param name="action"></param>
        public Task With(Action<Task<T>> action)
        {
            return Task.Run(() => action(_taskWithResult));
        }

        /// <summary>
        /// Continues the continuation around the func
        /// </summary>
        /// <param name="func"></param>
        public Task<TNext> With<TNext>(Func<Task<T>, TNext> func)
        {
            return Task.Run(() => func(_taskWithResult));
        }
    }
}
