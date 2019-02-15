using System.Threading.Tasks;

namespace PeanutButter.Utils
{
    public static class TaskExtensions
    {
        public static T GetResultSync<T>(
            this Task<T> task
        )
        {
            return task
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

    }
}