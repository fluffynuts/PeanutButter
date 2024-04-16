#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.Utils;

namespace Imported.PeanutButter.RandomGenerators;
#else
using PeanutButter.Utils;

namespace PeanutButter.RandomGenerators;
#endif

/// <summary>
/// Requires that the named property is randomized to a non-zero value
/// </summary>
public class RequireNonZero : RandomizerAttribute
{
    /// <inheritdoc />
    public RequireNonZero(string propertyName, params string[] moreProps)
        : base(propertyName, moreProps)
    {
    }

    /// <inheritdoc />
    public override void SetRandomValue(
        PropertyOrField propInfo,
        ref object target
    )
    {
        propInfo.SetValue(target, RandomValueGen.GetRandomInt(1));
    }
}