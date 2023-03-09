using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides a static Time method which times running
    /// the provided action the specified number of times
    /// </summary>
    public static class Benchmark
    {
        /// <summary>
        /// Times the provided action, run the provided number
        /// of iterations, and then prints out the run time to
        /// stdout
        /// </summary>
        /// <param name="label"></param>
        /// <param name="toRun"></param>
        /// <param name="iterations"></param>
        public static void PrintTime(
            string label,
            Action toRun,
            int iterations
        )
        {
            var runTime = Time(toRun, iterations);
            Console.WriteLine($"{label}: {runTime}");
        }

        /// <summary>
        /// Times the provided action, run the provided number
        /// of iterations, and then prints out the run time to
        /// stdout
        /// </summary>
        /// <param name="label"></param>
        /// <param name="toRun"></param>
        /// <param name="iterations"></param>
        public static async Task PrintTimeAsync(
            string label,
            Func<Task> toRun,
            int iterations
        )
        {
            var runTime = await TimeAsync(toRun, iterations);
            Console.WriteLine($"{label}: {runTime}");
        }

        /// <summary>
        /// Time a single iteration of the provided action
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static TimeSpan Time(Action action)
        {
            return Time(action, 1);
        }

        /// <summary>
        /// Time an action, run the specified number of times,
        /// and return the total time taken.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="iterations"></param>
        /// <returns></returns>
        public static TimeSpan Time(Action action, int iterations)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (var i = 0; i < iterations; i++)
            {
                action();
            }

            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        /// <summary>
        /// Time a single iteration of the provided async action
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task<TimeSpan> TimeAsync(Func<Task> action)
        {
            return await TimeAsync(action, 1);
        }

        /// <summary>
        /// Time an async action, run the specified number of times,
        /// and return the total time taken.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="iterations"></param>
        /// <returns></returns>
        public static async Task<TimeSpan> TimeAsync(Func<Task> action, int iterations)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (var i = 0; i < iterations; i++)
            {
                await action();
            }
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
    }
}