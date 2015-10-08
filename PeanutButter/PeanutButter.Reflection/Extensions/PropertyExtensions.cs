using System;
using System.Reflection;

namespace PeanutButter.Reflection.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static MemberAccessibility GetAccessibility(this PropertyInfo property)
        {
            MemberAccessibility accessibility = 0;

            if (property.CanRead) accessibility = property.GetReadAccessibility();

            if (property.CanWrite)
            {
                MemberAccessibility writeAccessibility = property.GetWriteAccessibility();
                if (accessibility == 0 || writeAccessibility < accessibility) accessibility = writeAccessibility;
            }

            return accessibility;
        }

        public static MemberAccessibility GetReadAccessibility(this PropertyInfo property)
        {
            if (property.CanRead) return property.GetGetMethod(true).GetAccessibility();
            throw new ArgumentException("Property " + property.Name + " is not readable.");
        }

        public static MemberAccessibility GetWriteAccessibility(this PropertyInfo property)
        {
            if (property.CanWrite) return property.GetSetMethod(true).GetAccessibility();
            throw new ArgumentException("Property " + property.Name + " is not writable.");
        }
    }
}
