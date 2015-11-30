using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Entity
{
    public static class TypeExtensions
    {
        public static void ShouldHaveRequiredProperty(this Type type, string name)
        {
            type.ShouldHaveProperty(name);
            var propertyInfo = Generic.TypeExtensions.GetPropertyForPath(type, name);
            var requiredAttribute = propertyInfo.GetCustomAttributes(true).OfType<RequiredAttribute>().FirstOrDefault();
            Assert.IsNotNull(requiredAttribute, "Expected property '{0}' to be decorated with [Required]", name);
        }

    }
    public static class ObjectExtensionsForTesting
    {
        public static void ShouldHaveMaxLengthOf<T>(this T item, int expectedMaxLength, Expression<Func<T, object>> expression)
        {
            var propertyPath = ExpressionUtil.GetMemberPathFor(expression);
            var propertyInfo = item.GetPropertyInfoFor(propertyPath);
            var maxLengthAttribute = propertyInfo.GetCustomAttributes<MaxLengthAttribute>().FirstOrDefault();
            var typeofT = typeof(T);
            var errorSuffix = string.Format("property '{0}' on type '{1}'", propertyPath, "' on type '" + typeofT.Name);
            if (maxLengthAttribute == null)
                Assert.Fail("No MaxLength attribute applied to " + errorSuffix);
            Assert.AreEqual(expectedMaxLength, maxLengthAttribute.Length, "Incorrect MaxLength value on " + errorSuffix);
        }

        public static PropertyInfo GetPropertyInfoFor(this object item, string path)
        {
            var parts = path.Split('.');
            var type = item.GetType();
            return parts.Aggregate((PropertyInfo) null, (acc, cur) =>
            {
                var pi = type.GetProperty(cur);
                item = pi.GetValue(item);
                return pi;
            });
        }
    }
}
