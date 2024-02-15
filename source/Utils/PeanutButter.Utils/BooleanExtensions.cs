#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils;
#else
namespace PeanutButter.Utils;
#endif
/// <summary>
/// Provides extensions for boolean values
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
public
#endif
    static class BooleanExtensions
{
    /// <summary>
    /// Negates the given boolean in-place: if
    /// you do flag.Negate(), the value held in flag
    /// should be negated and returned.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool Negate(
        this ref bool value
    )
    {
        value = !value;
        return value;
    }
}