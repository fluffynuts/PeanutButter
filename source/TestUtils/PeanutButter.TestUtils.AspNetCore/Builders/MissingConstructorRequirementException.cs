using System;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Builders;

internal class MissingConstructorRequirementException<T> : Exception
{
    internal MissingConstructorRequirementException(
        string context
    ) : base(
        $"Missing constructor requirement for {typeof(T)}: {(context ?? "(no context)").TrimStart('_').ToPascalCase()}")
    {
    }
}