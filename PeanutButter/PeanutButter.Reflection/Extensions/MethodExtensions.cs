using System.Reflection;

namespace PeanutButter.Reflection.Extensions
{
    public static class MethodInfoExtensions
    {
        public static MemberAccessibility GetAccessibility(this MethodBase method)
        {
            MemberAccessibility accessibility = 0;

            if (method.IsPublic) accessibility |= MemberAccessibility.Public;
            if (method.IsPrivate) accessibility |= MemberAccessibility.Private;
            if (method.IsAssembly || method.IsFamilyAndAssembly) accessibility |= MemberAccessibility.Internal;
            if (method.IsFamilyOrAssembly) accessibility |= MemberAccessibility.Protected | MemberAccessibility.Internal;
            if (method.IsFamily) accessibility |= MemberAccessibility.Protected;

            return accessibility;
        }
    }
}
