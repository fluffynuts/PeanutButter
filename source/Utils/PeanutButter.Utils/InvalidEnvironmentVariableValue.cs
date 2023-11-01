using System;

namespace
#if BUILD_PEANUTBUTTER_INTERNAL
    Imported.PeanutButter.Utils
#else
    PeanutButter.Utils
#endif
{
    /// <summary>
    /// 
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class InvalidEnvironmentVariableValue : Exception
    {
        /// <summary>
        /// The problematic environment variable
        /// </summary>
        public string EnvVar { get; }

        /// <summary>
        /// The problematic value
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Why this is a problem
        /// </summary>
        public string Why { get; }

        /// <summary>
        /// Constructs the InvalidEnvironmentVariable instance
        /// </summary>
        /// <param name="envVar"></param>
        /// <param name="value"></param>
        /// <param name="why"></param>
        public InvalidEnvironmentVariableValue(
            string envVar,
            string value,
            string why
        ) : base($"'{value}' is invalid for env var '{envVar}': {why}")
        {
            EnvVar = envVar;
            Value = value;
            Why = why;
        }
    }
}