using System;

namespace PeanutButter.Reflection
{
    [Flags]
    public enum MemberAccessibility
    {
        Private = 1,
        Protected = 2,
        Internal = 4,
        ProtectedInternal = Protected | Internal,
        Public = 8
    }
}
