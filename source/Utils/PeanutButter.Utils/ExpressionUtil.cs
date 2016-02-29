using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PeanutButter.Utils
{
    public class ExpressionUtil
    {
        public static string GetMemberPathFor<TSource>(Expression<Func<TSource, object>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
            {
                var unaryExpression = expression.Body as UnaryExpression;
                if (unaryExpression == null) throw new Exception("expression not supported");
                memberExpression = unaryExpression.Operand as MemberExpression;
                if (memberExpression == null)
                {
                    // try for a method
                    var instanceMethodExpression = unaryExpression.Operand as MethodCallExpression;
                    if (instanceMethodExpression != null)
                    {
                        return instanceMethodExpression.Method.Name;
                    }
                    throw new Exception("expression is not a member expression");
                }
            }
            return GetFullPropertyPathNameFrom(memberExpression);
        }

        public static Type GetPropertyTypeFor<TSource>(Expression<Func<TSource, object>> expression)
        {
            var propertyPath = GetMemberPathFor(expression);
            var pathParts = new Stack<string>(propertyPath.Split(new[] {"."}, StringSplitOptions.None));
            var result = typeof(TSource);
            do
            {
                var part = pathParts.Pop();
                var propInfo = result.GetProperty(part);
                result = propInfo.PropertyType;
            } while (pathParts.Any());
            return result;
        }

        public static string GetFullPropertyPathNameFrom(MemberExpression expression)
        {
            var parts = new List<string>(new[] {expression.Member.Name});
            var next = expression.Expression as MemberExpression;
            while (next != null)
            {
                parts.Add(next.Member.Name);
                next = next.Expression as MemberExpression;
            }
            parts.Reverse();
            return string.Join(".", parts);
        }
    }
}
