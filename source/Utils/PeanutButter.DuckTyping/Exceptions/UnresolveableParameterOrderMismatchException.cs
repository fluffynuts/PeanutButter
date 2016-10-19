using System;
using System.Linq;
using System.Reflection;

namespace PeanutButter.DuckTyping.Exceptions
{
    public class UnresolveableParameterOrderMismatchException: ArgumentException
    {
        public UnresolveableParameterOrderMismatchException(
            Type[] methodParameterTypes,
            MethodInfo methodInfo
        ): base($"Attempt to {methodInfo?.DeclaringType?.Name ?? "(unknown declaring type)"}.{methodInfo?.Name ?? "(anonymous method)"} with arguments out of sequence and target method has repeated argument types, making a re-order anyone's guess (parameters are of types: {string.Join(",", methodParameterTypes.Select(t => t.Name))})")
        {
        }
    }
}