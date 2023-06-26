using System.Threading.Tasks;
using PeanutButter.Async.Interfaces;

namespace PeanutButter.Async
{
    /// <summary>
    /// Convenience extensions
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Uses the provided task runner for the task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="taskRunner"></param>
        /// <returns></returns>
        public static IFluentTaskRunnerContinuation<object> Using(
            this Task task,
            ITaskRunner taskRunner)
        {
            return new FluentTaskRunnerContinuation<object>(task, taskRunner);
        }

        /// <summary>
        /// Uses the provided task runner for the task
        /// </summary>
        /// <param name="task"></param>
        /// <param name="taskRunner"></param>
        /// <returns></returns>
        public static IFluentTaskRunnerContinuation<T> Using<T>(
            this Task<T> task,
            ITaskRunner taskRunner)
        {
            return new FluentTaskRunnerContinuation<T>(task, taskRunner);
        }
    }
}