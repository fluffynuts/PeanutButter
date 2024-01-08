using System;
using System.Linq;
using System.Reflection;
using PeanutButter.TestUtils.Generic.NUnitAbstractions;
using PeanutButter.Utils;

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.TestUtils.Generic
{
    /// <summary>
    /// Provides a wrapper around DeepEqualityTester and NUnit for property assertions
    /// </summary>
    public static class PropertyAssert
    {
        /// <summary>
        /// Assert that all the properties for two objects match, deep-tested. Essentially tests
        /// that all primitive properties are equal and complex ones match shape
        /// </summary>
        /// <param name="obj1">Primary object</param>
        /// <param name="obj2">Object to compare with</param>
        /// <param name="ignorePropertiesByName">Properties to ignore, by name</param>
        public static void AreDeepEqual(object obj1, object obj2, params string[] ignorePropertiesByName)
        {
            var tester = new DeepEqualityTester(obj1, obj2, ignorePropertiesByName)
            {
                IncludeFields = false,
                RecordErrors = true
            };
            // this is PropertyAssert!
            if (!tester.AreDeepEqual())
            {
                Assertions.Throw(string.Join("\n", tester.Errors));
            }
        }

        /// <summary>
        /// Assert that common (intersecting) the properties for two objects match, deep-tested. Essentially tests
        /// that all primitive properties are equal and complex ones match shape
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <param name="ignorePropertiesByName"></param>
        public static void AreIntersectionEqual(object obj1, object obj2, params string[] ignorePropertiesByName)
        {
            var propInfos1 = obj1.GetType().GetProperties();
            var propInfos2 = obj2.GetType().GetProperties();
            var matchingProperties = propInfos1
                .Where(pi1 => propInfos2.FirstOrDefault(pi2 => pi1.Name == pi2.Name) != null)
                .Select(pi => pi.Name);
            var ignoreProps = propInfos1.Select(pi => pi.Name)
                .Union(propInfos2.Select(pi => pi.Name))
                .Distinct()
                .Except(matchingProperties)
                .Union(ignorePropertiesByName)
                .ToArray();
            var tester = new DeepEqualityTester(obj1, obj2, ignoreProps) {RecordErrors = true};
            if (tester.AreDeepEqual())
            {
                return;
            }

            Assertions.Throw(string.Join("\n", tester.Errors));
        }

        internal static object ResolveObject(object obj, ref string propName)
        {
            var propParts = propName.Split('.');
            if (propParts.Length == 1 || obj == null)
            {
                return obj;
            }
            propName = string.Join(".", propParts.Skip(1));
            var objType = obj.GetType();
            var propInfo = objType.GetProperty(propParts[0]);
            if (propInfo == null)
            {
                throw new Exception(string.Join(string.Empty,
                    "Unable to resolve property '",
                    propName,
                    "': can't find immediate property '",
                    propParts[0],
                    "' on object of type '",
                    objType.Name, "'"));
            }
            var propVal = propInfo.GetValue(obj, null);
            if (propVal == null && propName.IndexOf(".", StringComparison.Ordinal) > -1)
            {
                throw new Exception(string.Join(string.Empty, "Unable to traverse into property '", propName,
                    "': current object is null"));
            }
            return ResolveObject(propVal, ref propName);
        }

        /// <summary>
        /// Asserts that two objects have the same value for two specified properties, by
        /// name. If the second name is omitted, it is assumed to test the same-named property on both
        /// </summary>
        /// <param name="obj1">Primary object to test</param>
        /// <param name="obj2">Comparison object to test</param>
        /// <param name="obj1PropertyName">Property to test on both objects, unless obj2PropertyName is specified</param>
        /// <param name="obj2PropertyName">Specifies the name of the property to test on obj2</param>
        public static void AreEqual(
            object obj1,
            object obj2,
            string obj1PropertyName,
            string obj2PropertyName = null)
        {
            PerformEqualityAssertionWith(
                obj1,
                obj2,
                obj1PropertyName,
                obj2PropertyName,
                Assert.AreEqual);
        }

        /// <summary>
        /// Asserts that two objects have different values for two specified properties, by
        /// name. If the second name is omitted, it is assumed to test the same-named property on both
        /// </summary>
        /// <param name="obj1">Primary object to test</param>
        /// <param name="obj2">Comparison object to test</param>
        /// <param name="obj1PropertyName">Property to test on both objects, unless obj2PropertyName is specified</param>
        /// <param name="obj2PropertyName">Specifies the name of the property to test on obj2</param>
        public static void AreNotEqual<T1, T2>(
            T1 obj1,
            T2 obj2,
            string obj1PropertyName,
            string obj2PropertyName = null)
        {
            PerformEqualityAssertionWith(
                obj1,
                obj2,
                obj1PropertyName,
                obj2PropertyName,
                Assert.AreNotEqual);
        }

        private static void PerformEqualityAssertionWith(object obj1, object obj2, string obj1PropName,
            string obj2PropName, Action<object, object, string> finalAssertion)
        {
            if (obj2PropName == null)
            {
                obj2PropName = obj1PropName;
            }
            obj1 = ResolveObject(obj1, ref obj1PropName);
            obj2 = ResolveObject(obj2, ref obj2PropName);
            if (obj1 == null || obj2 == null)
            {
                if (obj1 == obj2)
                {
                    Assert.Fail("Both objects are null (" + obj1PropName + " => " + obj2PropName + ")");
                }
                Assert.Fail(
                    (obj1 == null ? "obj1" : "obj2") + " is null (" + obj1PropName + " => " + obj2PropName + ")");
                // ReSharper disable once HeuristicUnreachableCode
                return; // prevent warning about nulls
            }
            var type1 = obj1.GetType();
            var srcPropInfo = type1.GetProperty(obj1PropName);
            Assert.IsNotNull(srcPropInfo, PropNotFoundMessage(type1, obj1PropName));
            var type2 = obj2.GetType();
            var targetPropInfo = type2.GetProperty(obj2PropName);
            Assert.IsNotNull(targetPropInfo, PropNotFoundMessage(type2, obj2PropName));
            // ReSharper disable once PossibleNullReferenceException
            if (!targetPropInfo.PropertyType
                // ReSharper disable once PossibleNullReferenceException
                .MatchesOrIsNullableOf(srcPropInfo.PropertyType))
            {
                Assert.Fail(CreateTypeMismatchMessageFor(srcPropInfo, targetPropInfo));
            }
            finalAssertion(srcPropInfo.GetValue(obj1, null), targetPropInfo.GetValue(obj2, null),
                obj1PropName + " => " + obj2PropName);
        }

        private static string CreateTypeMismatchMessageFor(
            PropertyInfo srcPropInfo,
            PropertyInfo targetPropInfo
        )
        {
            return string.Join(string.Empty,
                "Property types for '",
                srcPropInfo.Name,
                "' do not match: ",
                srcPropInfo.PropertyType.Name,
                " vs ",
                targetPropInfo.PropertyType.Name);
        }

        private static string PropNotFoundMessage(Type type, string propName)
        {
            return string.Join(string.Empty, "Unable to find property '", propName, "' on type '", type.Name, "'");
        }
    }
}