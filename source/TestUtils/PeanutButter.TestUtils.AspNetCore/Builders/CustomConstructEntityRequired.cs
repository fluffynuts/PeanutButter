using System;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Builders;
#else
namespace PeanutButter.TestUtils.AspNetCore.Builders;
#endif

internal class CustomConstructEntityRequired
    : Exception
{
    public CustomConstructEntityRequired(
        Type outerType
    ) : base(
        $"Please implement {outerType.Name}.ConstructEntity"
    )
    {
    }
}