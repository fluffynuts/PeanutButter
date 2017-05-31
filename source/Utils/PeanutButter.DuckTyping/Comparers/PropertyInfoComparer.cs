using System.Collections.Generic;
using System.Reflection;

namespace PeanutButter.DuckTyping.Comparers
{
    internal class PropertyInfoComparer: IEqualityComparer<PropertyInfo>
    {
        public bool Equals(PropertyInfo x, PropertyInfo y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            return x.Name == y.Name;
        }

        public int GetHashCode(PropertyInfo obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}