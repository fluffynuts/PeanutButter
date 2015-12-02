using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Entity
{
    public static class ObjectExtensionsForTesting
    {
        public static void ShouldHaveMaxLengthOf<T>(this T item, int expectedMaxLength, Expression<Func<T, object>> expression)
        {
            var propertyPath = ExpressionUtil.GetMemberPathFor(expression);
            var propertyInfo = item.GetPropertyInfoFor(propertyPath);
            var maxLengthAttribute = propertyInfo.GetCustomAttributes<MaxLengthAttribute>().FirstOrDefault();
            var errorSuffix = ErrorSuffixFor<T>(propertyPath);
            if (maxLengthAttribute == null)
                Assert.Fail("No MaxLength attribute applied to " + errorSuffix);
            Assert.AreEqual(expectedMaxLength, maxLengthAttribute.Length, "Incorrect MaxLength value on " + errorSuffix);
        }

        public static void ShouldBeRequired<T>(this T item, Expression<Func<T, object>> expression)
        {
            var propertyPath = ExpressionUtil.GetMemberPathFor(expression);
            var propertyInfo = item.GetPropertyInfoFor(propertyPath);
            var requiredAttribute = propertyInfo.GetCustomAttributes<RequiredAttribute>().FirstOrDefault();
            if (requiredAttribute == null)
                Assert.Fail("No Required attribute applied to " + ErrorSuffixFor<T>(propertyPath));
        }

        public static void ShouldNotBeDatabaseGenerated<T>(this T item, Expression<Func<T, object>> expression)
        {
            var propertyPath = ExpressionUtil.GetMemberPathFor(expression);
            var propertyInfo = item.GetPropertyInfoFor(propertyPath);
            var attrib = propertyInfo.GetCustomAttributes<DatabaseGeneratedAttribute>().FirstOrDefault();
            Assert.IsNotNull(attrib, "No DatabaseGeneratedAttribute applied to " + ErrorSuffixFor<T>(propertyPath));
            Assert.AreEqual(DatabaseGeneratedOption.None, attrib.DatabaseGeneratedOption, "Expected [DatabaseGenerated(DatabaseGeneratedOption.None)] on " + ErrorSuffixFor<T>(propertyPath));
        }

        private static string ErrorSuffixFor<T>(string propertyPath)
        {
            return string.Format("property '{0}' on type '{1}'", propertyPath, typeof(T).PrettyName());
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