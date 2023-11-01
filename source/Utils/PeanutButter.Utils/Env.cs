using System;
using System.Collections.Generic;

namespace
#if BUILD_PEANUTBUTTER_INTERNAL
    Internal.PeanutButter.Utils
#else
    PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides easy access to environment variables with fallback
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class Env
    {
        /// <summary>
        /// Returns the value of the named environment variable
        /// or the fallback if that variable is not set
        /// </summary>
        /// <param name="envVar"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        public static string String(
            string envVar,
            string fallback
        )
        {
            return Environment.GetEnvironmentVariable(envVar)
                ?? fallback;
        }

        /// <summary>
        /// Tries to return an integer value set as an environment
        /// variable
        /// - if the variable is not set, returns the fallback
        /// - if the variable is not an integer, throws
        /// - before returning, validate it's within any specified range
        ///   and, if not, will clip to be within that range
        /// </summary>
        /// <param name="envVar"></param>
        /// <param name="fallback"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int Integer(
            string envVar,
            int fallback,
            int? min = null,
            int? max = null
        )
        {
            if (min is not null && max is not null)
            {
                if (min > max)
                {
                    var swap = max;
                    max = min;
                    min = swap;
                }
            }

            var envValue = Environment.GetEnvironmentVariable(envVar);
            var result = fallback;
            if (envValue is not null)
            {
                if (!int.TryParse(envValue, out result))
                {
                    throw new InvalidEnvironmentVariableValue(
                        envVar,
                        envValue,
                        "not a valid integer value"
                    );
                }
            }

            if (result < min)
            {
                result = min.Value;
            }
            else if (result > max)
            {
                result = max.Value;
            }

            return result;
        }

        /// <summary>
        /// Tries to return a decimal value set as an environment
        /// variable
        /// - if the variable is not set, returns the fallback
        /// - if the variable value is not a decimal, throws
        /// - before returning, validate it's within any specified range
        ///   and, if not, will clip to be within that range
        /// </summary>
        /// <param name="envVar"></param>
        /// <param name="fallback"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static decimal Decimal(
            string envVar,
            decimal fallback,
            decimal? min = null,
            decimal? max = null
        )
        {
            if (min is not null && max is not null)
            {
                if (min > max)
                {
                    var swap = max;
                    max = min;
                    min = swap;
                }
            }

            var envValue = Environment.GetEnvironmentVariable(envVar);
            var result = fallback;
            if (envValue is not null)
            {
                if (!decimal.TryParse(envValue, out result))
                {
                    throw new InvalidEnvironmentVariableValue(
                        envVar,
                        envValue,
                        "not a valid decimal value"
                    );
                }
            }

            if (result < min)
            {
                result = min.Value;
            }
            else if (result > max)
            {
                result = max.Value;
            }

            return result;
        }

        /// <summary>
        /// Tries to return a boolean value set as an environment
        /// variable
        /// - if the variable is not set, returns the fallback
        /// - if the variable value is not a recognised boolean, throws
        ///   - truthy values: true, 1, yes, on
        ///   - falsy values: false, 0, no, off
        /// parsing is case-insensitive
        /// </summary>
        /// <param name="envVar"></param>
        /// <param name="fallback"></param>
        /// <returns></returns>
        public static bool Flag(
            string envVar,
            bool fallback
        )
        {
            var stringValue = Environment.GetEnvironmentVariable(envVar)
                ?? $"{fallback}";
            if (bool.TryParse(stringValue, out var result))
            {
                return result;
            }

            throw new InvalidEnvironmentVariableValue(
                envVar,
                stringValue,
                "not a valid boolean value"
            );
        }

        private static readonly HashSet<string> TruthyValues = new(
            new[]
            {
                "true",
                "1",
                "yes",
                "on"
            },
            StringComparer.OrdinalIgnoreCase
        );

        private static readonly HashSet<string> FalseyyValues = new(
            new[]
            {
                "false",
                "0",
                "no",
                "off"
            },
            StringComparer.OrdinalIgnoreCase
        );
    }
}