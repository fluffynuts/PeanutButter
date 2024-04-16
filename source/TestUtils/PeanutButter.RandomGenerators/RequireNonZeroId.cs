#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.RandomGenerators;
#else
namespace PeanutButter.RandomGenerators;
#endif

/// <summary>
/// Requires that the field named "Id" be non-zero
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class RequireNonZeroId : RequireNonZero
{
    /// <inheritdoc />
    public RequireNonZeroId()
        : base("Id")
    {
    }
}