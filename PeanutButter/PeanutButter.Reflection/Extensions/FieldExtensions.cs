using System.Reflection;

namespace PeanutButter.Reflection.Extensions
{
    public static class FieldInfoExtensions
    {
        public static MemberAccessibility GetAccessibility(this FieldInfo field)
        {
            MemberAccessibility accessibility = 0;

            if (field.IsPublic) accessibility |= MemberAccessibility.Public;
            if (field.IsPrivate) accessibility |= MemberAccessibility.Private;
            if (field.IsAssembly || field.IsFamilyAndAssembly) accessibility |= MemberAccessibility.Internal;
            if (field.IsFamilyOrAssembly) accessibility |= MemberAccessibility.Protected | MemberAccessibility.Internal;
            if (field.IsFamily) accessibility |= MemberAccessibility.Protected;

            return accessibility;
        }
    }
}
