#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.Utils;
#else
using PeanutButter.Utils;
#endif

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.RandomGenerators;
#else
namespace PeanutButter.RandomGenerators;
#endif

/// <summary>
/// Specifies one or more properties, by name, to ignore
///    when attempting .WithRandomProps on an entity
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class NoRandomize : RandomizerAttribute
{
    /// <inheritdoc />
    public NoRandomize(
        string propertyName,
        params string[] otherProperties
    ) : base(
        propertyName,
        otherProperties
    )
    {
    }

    /// <inheritdoc />
    public override void SetRandomValue(PropertyOrField propInfo, ref object target)
    {
        /* intentionally left blank */
    }
}