using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using PeanutButter.TestUtils.Generic;

namespace PeanutButter.TestUtils.Entity
{
    public static class ObjectExtensionsForTesting
    {
        public static void ShouldHaveMaxLengthOf<T>(this T item, int expectedMaxLength, Expression<Func<T, object>> expression)
        {
            var propData = item.PropertyData(expression);
            var maxLengthAttribute = propData.CustomAttribute<MaxLengthAttribute>();
            var errorSuffix = ErrorSuffixFor<T>(propData.PropertyPath);
            if (maxLengthAttribute == null)
                Assert.Fail("No MaxLength attribute applied to " + errorSuffix);
            Assert.AreEqual(expectedMaxLength, maxLengthAttribute.Length, "Incorrect MaxLength value on " + errorSuffix);
        }

        public static void ShouldBeRequired<T>(this T item, Expression<Func<T, object>> expression)
        {
            var propData = item.PropertyData(expression);
            var requiredAttribute = propData.CustomAttribute<RequiredAttribute>();
            if (requiredAttribute == null)
                Assert.Fail("No Required attribute applied to " + ErrorSuffixFor<T>(propData.PropertyPath));
        }

        public static void ShouldNotBeDatabaseGenerated<T>(this T item, Expression<Func<T, object>> expression)
        {
            var propData = item.PropertyData(expression);
            var attrib = propData.CustomAttribute<DatabaseGeneratedAttribute>();
            Assert.IsNotNull(attrib, "No DatabaseGeneratedAttribute applied to " + ErrorSuffixFor<T>(propData.PropertyPath));
            Assert.AreEqual(DatabaseGeneratedOption.None, attrib.DatabaseGeneratedOption, 
                "Expected [DatabaseGenerated(DatabaseGeneratedOption.None)] on " + ErrorSuffixFor<T>(propData.PropertyPath));
        }

        public static void ShouldHaveForeignKey<T>(this T item, string keyName, Expression<Func<T, object>> expression)
        {
            var propData = item.PropertyData(expression);
            var attrib = propData.CustomAttribute<ForeignKeyAttribute>();
            Assert.IsNotNull(attrib, "No ForeignKey attribute applied to " + ErrorSuffixFor<T>(propData.PropertyPath));
            Assert.AreEqual(keyName, attrib.Name, "Incorrect ForeignKey value");
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