using System;
using System.Collections.Generic;

namespace PeanutButter.DuckTyping.Exceptions
{
    public class UnDuckableException : Exception
    {
        public IEnumerable<string> Errors { get; }

        public UnDuckableException(IEnumerable<string> errors)
            : base("Unable to perform duck operation. Examine errors for more information")
        {
            Errors = errors;
        }
    }
}