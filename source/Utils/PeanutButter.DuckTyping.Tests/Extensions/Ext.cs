using NUnit.Framework.Constraints;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace PeanutButter.DuckTyping.Tests.Extensions
{
  public static class Ext
    {
        public static ContainsConstraint Containing(
            this ResolvableConstraintExpression expr,
            string expected
        )
        {
            return expr.Contain(expected);
        }
    }

}
