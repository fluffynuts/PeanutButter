using System;

namespace PeanutButter.Reflection.Extensions
{
    public static class TypeExtensions
    {
        public static MemberAccessibility GetAccessibility(this Type type)
        {
            MemberAccessibility accessibility = 0;

            if (type.IsNested)
            {
                if (type.IsNestedPublic) accessibility |= MemberAccessibility.Public;
                if (type.IsNestedPrivate) accessibility |= MemberAccessibility.Private;
                if (type.IsNestedAssembly || type.IsNestedFamANDAssem) accessibility |= MemberAccessibility.Internal;

                if (type.IsNestedFamORAssem) accessibility |= MemberAccessibility.Protected | MemberAccessibility.Internal;
                if (type.IsNestedFamily) accessibility |= MemberAccessibility.Protected;
            }
            else
            {
                if (type.IsPublic) accessibility = MemberAccessibility.Public;
                else accessibility = MemberAccessibility.Internal;
            }

            return accessibility;
        }
    }
}
