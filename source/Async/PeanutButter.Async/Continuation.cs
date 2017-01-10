using System;
using System.Threading.Tasks;
using PeanutButter.Async.Interfaces;

namespace PeanutButter.Async
{
    public class Continuation<T> : IContinuation<T>, IContinuation
    {
        private readonly Task<T> _taskWithResult;
        private readonly Task _taskWithoutResult;

        public Continuation(Task<T> taskWithResult)
        {
            _taskWithResult = taskWithResult;
        }

        public Continuation(Task taskWithoutResult)
        {
            _taskWithoutResult = taskWithoutResult;
        }

        public Task With(Action<Task> action)
        {
            return Task.Run(() => action(_taskWithResult ?? _taskWithoutResult));
        }

        public Task With(Action<Task<T>> action)
        {
            return Task.Run(() => action(_taskWithResult));
        }

        public Task<TNext> With<TNext>(Func<Task<T>, TNext> func)
        {
            return Task.Run(() => func(_taskWithResult));
        }
    }
}
