using System;
using System.Threading.Tasks;
using PeanutButter.Async.Interfaces;

namespace PeanutButter.Async
{
    public class Continuation<T> : IContinuation<T>, IContinuation
    {
        private readonly Task<T> _ofTaskWithResult;
        private readonly Task _ofTaskWithoutResult;

        public Continuation(Task<T> ofTaskWithResult)
        {
            _ofTaskWithResult = ofTaskWithResult;
        }

        public Continuation(Task ofTaskWithoutResult)
        {
            _ofTaskWithoutResult = ofTaskWithoutResult;
        }

        public Task With(Action<Task> action)
        {
            return Task.Run(() => action(_ofTaskWithResult ?? _ofTaskWithoutResult));
        }

        public Task With(Action<Task<T>> action)
        {
            return Task.Run(() => action(_ofTaskWithResult));
        }

        public Task<TNext> With<TNext>(Func<Task<T>, TNext> func)
        {
            return Task.Run(() => func(_ofTaskWithResult));
        }
    }

}
