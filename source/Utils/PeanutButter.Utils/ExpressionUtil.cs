using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UsePatternMatching

namespace PeanutButter.Utils
{
    /// <summary>
    /// Utility class to assist with dealing with expressions
    /// </summary>
    public static class ExpressionUtil
    {
        /// <summary>
        /// Calculates the member path of an Expression and returns it as a dotted string
        /// </summary>
        /// <param name="expression">Subject Expression to operate on</param>
        /// <typeparam name="TSource">Type of the source object for the expression</typeparam>
        /// <returns>
        /// (Dotted) string representing the path in the expression
        /// - for example, the expression o => o.Foo.Bar
        ///     will return "Foo.Bar"
        /// </returns>
        /// <exception cref="Exception">
        /// Exceptions are thrown when the Expression cannot be grokked. The Exception message
        /// will explain why.
        /// </exception>
        public static string GetMemberPathFor<TSource>(Expression<Func<TSource, object>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression != null)
                return GetFullPropertyPathNameFrom(memberExpression);
            var unaryExpression = expression.Body as UnaryExpression;
            if (unaryExpression == null) throw new Exception("expression not supported");
            memberExpression = unaryExpression.Operand as MemberExpression;
            if (memberExpression != null)
                return GetFullPropertyPathNameFrom(memberExpression);
            // try for a method
            var instanceMethodExpression = unaryExpression.Operand as MethodCallExpression;
            if (instanceMethodExpression != null)
            {
                return instanceMethodExpression.Method.Name;
            }

            throw new Exception("expression is not a member expression");
        }

        /// <summary>
        /// Gets the type of the property referred to by an expression
        /// </summary>
        /// <param name="expression">Subject expression to operate on</param>
        /// <typeparam name="TSource">Type of the source object of the expression</typeparam>
        /// <returns>
        /// The type of the property referred to in the expression. For example,
        /// o => o.Name would probably return string
        /// </returns>
        public static Type GetPropertyTypeFor<TSource>(Expression<Func<TSource, object>> expression)
        {
            var propertyPath = GetMemberPathFor(expression);
            var pathParts = new Stack<string>(propertyPath.Split(new[] {"."}, StringSplitOptions.None));
            var result = typeof(TSource);
            do
            {
                var part = pathParts.Pop();
                var propInfo = result.GetProperty(part);
                if (propInfo == null)
                    throw new InvalidOperationException($"Unable to find property {part} on {result}");
                result = propInfo.PropertyType;
            } while (pathParts.Any());

            return result;
        }

        /// <summary>
        /// Calculates the member path of an Expression and returns it as a dotted string
        /// </summary>
        /// <param name="expression">Subject Expression to operate on</param>
        /// <returns>
        /// (Dotted) string representing the path in the expression
        /// - for example, the expression o => o.Foo.Bar
        ///     will return "Foo.Bar"
        /// </returns>
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