using System.Collections.Generic;
using System.Reflection;

namespace PeanutButter.DuckTyping
{
    internal class PropertyInfoComparer: IEqualityComparer<PropertyInfo>
    {
        public bool Equals(PropertyInfo x, PropertyInfo y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Name == y.Name &&
                   x.DeclaringType == y.DeclaringType;
        }

        public int GetHashCode(PropertyInfo obj)
        {
            return obj?.GetHashCode() ?? 0;
        }
    }
}