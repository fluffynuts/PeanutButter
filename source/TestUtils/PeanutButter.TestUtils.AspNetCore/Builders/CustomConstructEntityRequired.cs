using System;

namespace PeanutButter.TestUtils.AspNetCore.Builders;

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