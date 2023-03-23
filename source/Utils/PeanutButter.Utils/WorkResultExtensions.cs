using System;
using System.Collections.Generic;
using System.Linq;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides convenience extensions for work results
    /// </summary>
    // ReSharper disable once UnusedType.Global
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
    static class WorkResultExtensions
    {
        /// <summary>
        /// Provides a convenience method into retrieving all
        /// result values from a collection of WorkResults.
        /// If any exceptions are encountered, an AggregateException will be thrown
        /// </summary>
        public static IEnumerable<T> Results<T>(
            this IEnumerable<WorkResult<T>> workResults
        )
        {
            return workResults.Results(ErrorStrategies.Throw);
        }

        /// <summary>
        /// Provides a convenience method into retrieving all
        /// result values from a collection of WorkResults.
        /// If any exceptions are encountered and not suppressed,
        /// an AggregateException will be thrown
        /// </summary>
        public static IEnumerable<T> Results<T>(
            this IEnumerable<WorkResult<T>> workResults,
            ErrorStrategies errorStrategy
        )
        {
            var errors = new List<Exception>();
            var results = new List<T>();
            foreach (var result in workResults)
            {
                if (result.Exception is not null)
                {
                    errors.Add(result.Exception);
                }
                else
                {
                    results.Add(result.Result);
                }
            }

            if (errorStrategy == ErrorStrategies.Throw && errors.Any())
            {
                throw new AggregateException(
                    errors
                );
            }

            return results.ToArray();
        }

        /// <summary>
        /// Retrieve just the errors from a collection of WorkResults
        /// </summary>
        /// <param name="workResults"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Exception[] Errors<T>(
            this IEnumerable<WorkResult<T>> workResults
        )
        {
            return workResults.Select(o => o?.Exception)
                .Where(e => e is not null)
                .ToArray();
        }
    }
}