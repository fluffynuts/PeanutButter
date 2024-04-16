using System;
using System.Linq;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.RandomGenerators;
#else
namespace PeanutButter.RandomGenerators;
#endif

/// <summary>
/// Require a unique Id on the generated entity
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class RequireUniqueId : RequireUnique
{
    /// <inheritdoc />
    public RequireUniqueId() : base("Id")
    {
    }

    /// <inheritdoc />
    public override void Init(Type entityType)
    {
        var propName = PropertyNames.Single();
        PropertyType = entityType.GetProperty(propName)
            ?.PropertyType ?? throw new InvalidOperationException(
            $"Unable to find property {propName} on {entityType}"
        );
        base.Init(entityType);
    }
}