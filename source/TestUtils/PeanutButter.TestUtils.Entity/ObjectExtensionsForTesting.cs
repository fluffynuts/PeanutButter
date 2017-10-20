using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using PeanutButter.TempDb.LocalDb;
using PeanutButter.Utils;

// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local

namespace PeanutButter.TestUtils.Entity
{
    public static class ObjectExtensionsForTesting
    {
        public static void ShouldHaveMaxLengthOf<T>(this T item, int expectedMaxLength,
            Expression<Func<T, object>> expression)
        {
            var propData = item.PropertyDataFor(expression);
            var maxLengthAttribute = propData.CustomAttribute<MaxLengthAttribute>();
            var stringLengthAttribute = propData.CustomAttribute<StringLengthAttribute>();
            var errorSuffix = ErrorSuffixFor<T>(propData.PropertyPath);
            if (maxLengthAttribute == null &&
                stringLengthAttribute == null)
                Assert.Fail("No MaxLength or StringLength attribute applied to " + errorSuffix);
            var specifiedMaxLength = GetMaxLengthFrom(maxLengthAttribute, stringLengthAttribute, propData);
            Assert.AreEqual(expectedMaxLength, specifiedMaxLength, "Incorrect MaxLength value on " + errorSuffix);
        }

        public static void ShouldBeRequired<T>(this T item, Expression<Func<T, object>> expression)
        {
            var propData = item.PropertyDataFor(expression);
            var requiredAttribute = propData.CustomAttribute<RequiredAttribute>();
            if (requiredAttribute == null)
                Assert.Fail("No Required attribute applied to " + ErrorSuffixFor<T>(propData.PropertyPath));
        }

        public static void ShouldNotBeDatabaseGenerated<T>(this T item, Expression<Func<T, object>> expression)
        {
            var propData = item.PropertyDataFor(expression);
            var attrib = propData.CustomAttribute<DatabaseGeneratedAttribute>();
            AssertHasDatabaseGeneratedattribute(attrib, propData);
            AssertHasExpectedDatabaseGeneratedOption(DatabaseGeneratedOption.None, attrib, propData);
        }

        public static void ShouldBeIdentity<T>(this T item, Expression<Func<T, object>> expression)
        {
            var propData = item.PropertyDataFor(expression);
            var attrib = propData.CustomAttribute<DatabaseGeneratedAttribute>();
            AssertHasDatabaseGeneratedattribute(attrib, propData);
            AssertHasExpectedDatabaseGeneratedOption(DatabaseGeneratedOption.Identity, attrib, propData);
        }

        public static void ShouldHaveForeignKey<T>(this T item, string keyName, Expression<Func<T, object>> expression)
        {
            var propData = item.PropertyDataFor(expression);
            var attrib = propData.CustomAttribute<ForeignKeyAttribute>();
            Assert.IsNotNull(attrib, "No ForeignKey attribute applied to " + ErrorSuffixFor<T>(propData.PropertyPath));
            Assert.AreEqual(keyName, attrib.Name, "Incorrect ForeignKey value");
        }

        public static PropertyInfo GetPropertyInfoFor(this object item, string path)
        {
            var parts = path.Split('.');
            var type = item.GetType();
            return parts.Aggregate((PropertyInfo) null,
                (acc, cur) =>
                {
                    var pi = type.GetProperty(cur);
                    if (pi == null)
                        throw new InvalidOperationException($"Unable to find property {cur} on {item?.GetType()}");
                    item = pi.GetValue(item);
                    return pi;
                });
        }

        public static void ShouldHaveIDbSetFor<TEntity>(this Type contextType)
        {
            var propInfo = contextType.GetProperties()
                .FirstOrDefault(IsIDbSetFor<TEntity>);
            if (propInfo == null)
                Assert.Fail($"Expected context of type {contextType.PrettyName()} to have IDbSet<{typeof(TEntity)}>");
        }

        public static void ShouldHaveNullInitializer<T>(this T context) where T : DbContext
        {
            using (new TempDBLocalDb())
            {
                /* intentionally left blank */
            }
        }

        private static readonly Type _dbSetGenericType = typeof(IDbSet<>);

        private static bool IsIDbSetFor<TEntity>(PropertyInfo propInfo)
        {
            var searchType = _dbSetGenericType.MakeGenericType(typeof(TEntity));
            return searchType.IsAssignableFrom(propInfo.PropertyType);
        }

        private static int GetMaxLengthFrom<T>(MaxLengthAttribute maxLengthAttribute,
            StringLengthAttribute stringLengthAttribute,
            PropertyData<T> propData)
        {
            if (stringLengthAttribute == null)
                return maxLengthAttribute.Length;
            if (maxLengthAttribute == null)
                return stringLengthAttribute.MaximumLength;
            if (stringLengthAttribute.MaximumLength == maxLengthAttribute.Length)
                return stringLengthAttribute.MaximumLength;
            Assert.Fail(string.Join(string.Empty,
                "MaxLength and StringLength are both specified for '",
                propData.PropertyPath,
                "' on type '",
                propData.ParentType.PrettyName(),
                "' but disagree on the maximum length: ",
                maxLengthAttribute.Length,
                " vs ",
                stringLengthAttribute.MaximumLength
            ));
            throw new Exception("Should not get here as we should have Assert.Failed");
        }

        private static string ErrorSuffixFor<T>(string propertyPath)
        {
            return $"property '{propertyPath}' on type '{typeof(T).PrettyName()}'";
        }

        private static void AssertHasExpectedDatabaseGeneratedOption<T>(DatabaseGeneratedOption databaseGeneratedOption,
            DatabaseGeneratedAttribute attrib, PropertyData<T> propData)
        {
            Assert.AreEqual(databaseGeneratedOption,
                attrib.DatabaseGeneratedOption,
                new[]
                {
                    "Expected [DatabaseGenerated(DatabaseGeneratedOption.",
                    databaseGeneratedOption.ToString(),
                    ")] on ",
                    ErrorSuffixFor<T>(propData.PropertyPath)
                }.JoinWith(""));
        }

        private static void AssertHasDatabaseGeneratedattribute<T>(
            DatabaseGeneratedAttribute attrib,
            PropertyData<T> propData
        )
        {
            Assert.IsNotNull(attrib,
                "No DatabaseGeneratedAttribute applied to " + ErrorSuffixFor<T>(propData.PropertyPath));
        }
    }
}