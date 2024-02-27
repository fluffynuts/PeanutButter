using System;
using System.Collections.Generic;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.Exceptions
#else
namespace PeanutButter.DuckTyping.Exceptions
#endif
{
    /// <summary>
    /// The master exception thrown when a type is not duckable and exceptions
    /// have been enabled
    /// </summary>
#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
    internal
#else
    public
#endif
    class UnDuckableException : Exception
    {
        /// <summary>
        /// Collection of errors encountered whilst attempting the requested ducking operation
        /// </summary>
        public IEnumerable<string> Errors { get; }

        /// <summary>
        /// Constructs an instance of the exception
        /// </summary>
        /// <param name="errors">Errors to store about the duck failure</param>
        public UnDuckableException(params string[] errors)
            : base($"Unable to perform duck operation. Examine errors for more information\n\nErrors:\n- {string.Join("\n- ", errors)}")
        {
            Errors = errors;
        }
    }
}