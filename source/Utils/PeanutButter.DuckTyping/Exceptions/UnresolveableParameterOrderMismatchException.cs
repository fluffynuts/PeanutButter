using System;
using System.Linq;
using System.Reflection;

namespace PeanutButter.DuckTyping.Exceptions
{
    /// <summary>
    /// Exception thrown when fuzzy ducking is enabled but the duck
    /// library cannot resolve the order of parameters to pass into the
    /// underlying type, usually because there are repeated types amongst the arguments
    /// (ie, two integers or similar)
    /// </summary>
    public class UnresolveableParameterOrderMismatchException: ArgumentException
    {
        /// <summary>
        /// Constructs an instance of the exception
        /// </summary>
        /// <param name="methodParameterTypes">Collection of the parameter types supported by the underlying method, ordered as the underlying method has them ordered</param>
        /// <param name="methodInfo">MethodInfo object describing the problematic method</param>
        public UnresolveableParameterOrderMismatchException(
            Type[] methodParameterTypes,
            MethodInfo methodInfo
        ): base($"Attempt to {methodInfo?.DeclaringType?.Name ?? "(unknown declaring type)"}.{methodInfo?.Name ?? "(anonymous method)"} with arguments out of sequence and target method has repeated argument types, making a re-order anyone's guess (parameters are of types: {string.Join(",", methodParameterTypes.Select(t => t.Name))})")
        {
        }
    }
}