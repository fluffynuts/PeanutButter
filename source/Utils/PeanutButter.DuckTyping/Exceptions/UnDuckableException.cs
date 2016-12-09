using System;
using System.Collections.Generic;

namespace PeanutButter.DuckTyping.Exceptions
{
    /// <summary>
    /// The master exception thrown when a type is not duckable and exceptions
    /// have been enabled
    /// </summary>
    public class UnDuckableException : Exception
    {
        /// <summary>
        /// Collection of errors encountered whilst attempting the requested ducking operation
        /// </summary>
        public IEnumerable<string> Errors { get; }

        /// <summary>
        /// Constructs an instance of the exception
        /// </summary>
        /// <param name="errors">Errors to store about the duck failure</param>
        public UnDuckableException(IEnumerable<string> errors)
            : base("Unable to perform duck operation. Examine errors for more information")
        {
            Errors = errors;
        }
    }
}