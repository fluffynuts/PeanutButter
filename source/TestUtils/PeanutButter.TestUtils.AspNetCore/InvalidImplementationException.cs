using System;

namespace PeanutButter.TestUtils.AspNetCore
{
    public class InvalidImplementationException : Exception
    {
        public InvalidImplementationException(
            object implementation,
            string context
        ) : base(GenerateMessageFor(implementation, context))
        {
        }

        private static string GenerateMessageFor(
            object implementation,
            string context
        )
        {
            return implementation is null
                ? $"implementation is null for {context}"
                : $"invalid implementation {implementation} for {context}";
        }
    }
}