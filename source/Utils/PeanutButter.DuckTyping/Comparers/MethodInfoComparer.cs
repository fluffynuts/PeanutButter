using System.Collections.Generic;
using System.Reflection;
// ReSharper disable ConstantConditionalAccessQualifier
// ReSharper disable ConstantNullCoalescingCondition

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.Comparers
#else
namespace PeanutButter.DuckTyping.Comparers
#endif
{
    internal class MethodInfoComparer: IEqualityComparer<MethodInfo>
    {
        public bool Equals(MethodInfo x, MethodInfo y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Name == y.Name;
        }

        public int GetHashCode(MethodInfo obj)
        {
            return obj?.Name.GetHashCode() ?? 0;
        }
    }
}