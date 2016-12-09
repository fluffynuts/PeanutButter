using System;
using System.Reflection;

namespace PeanutButter.DuckTyping.Exceptions
{
    /// <summary>
    /// Exception thrown when a ducked type has a different number of parameters
    /// for a ducked method than the ducked interface expects
    /// </summary>
    public class ParameterCountMismatchException: ArgumentException
    {
        /// <summary>
        /// Constructs an instance of the exception
        /// </summary>
        /// <param name="providedParameters">Number of expected parameters</param>
        /// <param name="forMethod">Name of the method with a parameter count mismatch</param>
        public ParameterCountMismatchException(
            int providedParameters,
            MethodInfo forMethod
        ): base ($"{providedParameters} parameters were provided for method {forMethod?.DeclaringType?.Name ?? "(unknown declaring type)"}.{forMethod?.Name ?? "(anonymous method)"} but it requires {forMethod.GetParameters().Length} parameters")
        {
        }
    }
}