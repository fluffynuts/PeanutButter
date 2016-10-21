using System.Collections.Generic;
using System.Reflection;

namespace PeanutButter.DuckTyping
{
    internal class MethodInfoComparer: IEqualityComparer<MethodInfo>
    {
        public bool Equals(MethodInfo x, MethodInfo y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Name == y.Name &&
                   x.DeclaringType == y.DeclaringType;
        }

        public int GetHashCode(MethodInfo obj)
        {
            return obj?.GetHashCode() ?? 0;
        }
    }
}