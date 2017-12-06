using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global

namespace PeanutButter.RandomGenerators.Tests
{
    public static class VarianceAssert
    {
        public static void IsVariant<TObject, TProperty>(IEnumerable<TObject> collection, string propertyName)
        {
            var type = typeof(TObject);
            var propInfo = type.GetProperty(propertyName);
            if (propInfo == null)
                throw new Exception(string.Join("", "Unable to find property '", propertyName, "' on type '", type.Name));

            IEnumerable<TProperty> values = null;
            try
            {
                values = collection.Select(obj => (TProperty)propInfo.GetValue(obj, null)).ToArray();
            }
            catch (Exception ex)
            {
                Assert.Fail(string.Join("", "Unable to get list of property values for '", propertyName, "' of type '", typeof(TProperty).Name, "' from object of type '", type.Name, "': ", ex.Message));
            }
            IsVariant(values, $"No variance for property '{propertyName}' across {{0}} samples");
        }

        public static void IsVariant<TObject>(
            IEnumerable<TObject> collection,
            string failMessage = "No variance across {0} samples",
            IEqualityComparer<TObject> comparer = null
        )
        {
            if (collection.Count() < 2)
                return;
            if (collection.Distinct(comparer).Count() == 1)
                Assert.Fail(failMessage, collection.Count());
        }
    }
}