using System;
using System.Diagnostics;

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
        /// Time an action, run the specified number of times,
        /// and return the time it took.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="iterations"></param>
        /// <returns></returns>
        public static TimeSpan Time(Action action, int iterations)
        {
            return TimeWithStopwatch(action, iterations).Elapsed;
        }

        private static Stopwatch TimeWithStopwatch(
            Action action,
            int iterations
        )
        {
            var tStopwatch = new Stopwatch();
            tStopwatch.Start();
            for (var i = 0; i < iterations; i++)
            {
                action();
            }

            tStopwatch.Stop();
            return tStopwatch;
        }
    }
}