using System;

#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.Utils;

namespace Imported.PeanutButter.TestUtils.AspNetCore.Builders;
#else
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Builders;
#endif

internal class MissingConstructorRequirementException<T> : Exception
{
    internal MissingConstructorRequirementException(
        string context
    ) : base(
        $"Missing constructor requirement for {typeof(T)}: {(context ?? "(no context)").TrimStart('_').ToPascalCase()}"
    )
    {
    }
}