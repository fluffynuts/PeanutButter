using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PeanutButter.RandomGenerators.Tests
{
    public static class VarianceAssert
    {
        public static void IsVariant<TObject, TProperty>(IEnumerable<TObject> collection, string propertyName)
        {
            var type = typeof(TObject);
            var propInfo = type.GetProperty(propertyName);
            if (propInfo == null)
                throw new Exception(string.Join("", new[] { "Unable to find property '", propertyName, "' on type '", type.Name }));

            IEnumerable<TProperty> values = null;
            try
            {
                values = collection.Select(obj => (TProperty)propInfo.GetValue(obj, null)).ToArray();
            }
            catch (Exception ex)
            {
                Assert.Fail(string.Join("", new[] { "Unable to get list of property values for '", propertyName, "' of type '", typeof(TProperty).Name
                    , "' from object of type '", type.Name, "': ", ex.Message }));
            }
            var totalCount = values.Count();
            foreach (var value in values)
            {
                if (values.Count(v => v.Equals(value)) == totalCount)
                {
                    Assert.Fail(string.Join("", new[] { "No variance for property '", propertyName, "' across ", totalCount.ToString(), " samples" }));
                }
            }
        }
    }
}