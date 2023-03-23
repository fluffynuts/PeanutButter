using System.Threading.Tasks;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Extension methods for tasks
    /// Suggest: Use Async.RunSync, which does more cleverness.
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class TaskExtensions
    {
        /// <summary>
        /// Runs a task which returns a result synchronously, returning that result
        /// </summary>
        /// <param name="task"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetResultSync<T>(
            this Task<T> task
        )
        {
            return task
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Waits on a void-result task for completion
        /// </summary>
        /// <param name="task"></param>
        public static void WaitSync(
            this Task task)
        {
            task.ConfigureAwait(false);
            task.Wait();
        }
    }
}