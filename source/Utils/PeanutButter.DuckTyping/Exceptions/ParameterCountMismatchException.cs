using System;
using System.Reflection;

namespace PeanutButter.DuckTyping.Exceptions
{
    public class ParameterCountMismatchException: ArgumentException
    {
        public ParameterCountMismatchException(
            int providedParameters,
            MethodInfo forMethod
        ): base ($"{providedParameters} parameters were provided for method {forMethod?.DeclaringType?.Name ?? "(unknown declaring type)"}.{forMethod?.Name ?? "(anonymous method)"} but it requires {forMethod.GetParameters().Length} parameters")
        {
        }
    }
}