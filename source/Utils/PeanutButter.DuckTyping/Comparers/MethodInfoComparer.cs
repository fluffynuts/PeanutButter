using System.Collections.Generic;
using System.Reflection;

namespace PeanutButter.DuckTyping.Comparers
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