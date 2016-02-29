using System;
using System.Collections.Generic;

namespace PeanutButter.Utils
{
    public static class TypeExtensions
    {
        public static Type[] Ancestry(this Type type)
        {
            var heirachy = new List<Type>();
            do
            {
                heirachy.Add(type);
            } while ((type = type.BaseType) != null);
            heirachy.Reverse();
            return heirachy.ToArray();
        }
    }
}