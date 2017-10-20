using System.Collections.Generic;

namespace PeanutButter.Utils
{
    internal class StrictComparer<TKey> : IEqualityComparer<TKey>
    {
        public bool Equals(TKey x, TKey y)
        {
            if (x == null && y == null)
                return true;
            return x?.Equals(y) ?? false;
        }

        public int GetHashCode(TKey obj)
        {
            return obj.GetHashCode();
        }
    }
}