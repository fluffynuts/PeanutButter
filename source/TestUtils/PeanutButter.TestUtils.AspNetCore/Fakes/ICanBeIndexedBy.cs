using Microsoft.Extensions.Primitives;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
#else
namespace PeanutButter.TestUtils.AspNetCore.Fakes;
#endif

/// <summary>
/// Describes a type which has
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    interface ICanBeIndexedBy<in T>
{
    /// <summary>
    /// Indexes into the store
    /// </summary>
    /// <param name="key"></param>
    StringValues this[T key] { get; set; }
}