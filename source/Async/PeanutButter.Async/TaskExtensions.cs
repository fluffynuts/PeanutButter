using System.Threading.Tasks;
using PeanutButter.Async.Interfaces;

namespace PeanutButter.Async
{
    public static class TaskExtensions
    {
        public static IFluentTaskRunnerContinuation<object> Using(this Task task, ITaskRunner taskRunner)
        {
            return new FluentTaskRunnerContinuation<object>(task, taskRunner);
        }

        public static IFluentTaskRunnerContinuation<T> Using<T>(this Task<T> task, ITaskRunner taskRunner)
        {
            return new FluentTaskRunnerContinuation<T>(task, taskRunner);
        }
    }
}
