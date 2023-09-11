using System.Collections.Generic;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils;
#else
namespace PeanutButter.Utils;
#endif
internal class StrictComparer<TKey> : IEqualityComparer<TKey>
{
    public bool Equals(TKey x, TKey y)
    {
        if (x is null && y is null)
        {
            return true;
        }

        return x?.Equals(y) ?? false;
    }

    public int GetHashCode(TKey obj)
    {
        // ReSharper disable once ConstantNullCoalescingCondition
        // ReSharper disable once ConstantConditionalAccessQualifier
        return obj?.GetHashCode() ?? 0;
    }
}