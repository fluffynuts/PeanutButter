using System;
using System.Linq;
using NUnit.Framework;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.Generic
{
    public class PropertyAssert
    {
        [Obsolete("PropertyAssert.AllPropertiesAreEqual has been obsoleted in favour of PropertyAssert.AreDeepEqual")]
        public static void AllPropertiesAreEqual(object obj1, object obj2, params string[] ignorePropertiesByName)
        {
            PropertyAssert.AreDeepEqual(obj1, obj2, ignorePropertiesByName);
        }

        [Obsolete("PropertyAssert.MatchingPropertiesAreEqual has been obsoleted in favour of PropertyAssert.IntersectionEquals")]
        public static void MatchingPropertiesAreEqual(object obj1, object obj2)
        {
            PropertyAssert.IntersectionEquals(obj1, obj2);
        }

        public static void AreDeepEqual(object obj1, object obj2, params string[] ignorePropertiesByName)
        {
            var tester = new DeepEqualityTester(obj1, obj2, ignorePropertiesByName);
            if (!tester.AreDeepEqual())
                throw new AssertionException(string.Join("\n", tester.Errors));
        }

        public static void IntersectionEquals(object obj1, object obj2)
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
                                        .ToArray();
            var tester = new DeepEqualityTester(obj1, obj2, ignoreProps);
            if (tester.AreDeepEqual())
                return;
            throw new AssertionException(string.Join("\n", tester.Errors));
        }

        public static object ResolveObject(object obj, ref string propName)
        {
            var propParts = propName.Split(new[] { '.' });
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
                throw new Exception(string.Join(string.Empty, "Unable to traverse into property '", propName, "': current object is null"));
            }
            return ResolveObject(propVal, ref propName);
        }

        public static void AreEqual(object obj1, object obj2, string obj1PropName, string obj2PropName = null)
        {
            PerformEqualityAssertionWith(obj1, obj2, obj1PropName, obj2PropName, (o1, o2, info) => Assert.AreEqual(o1, o2, info));
        }

        public static void AreNotEqual<T1, T2>(T1 obj1, T2 obj2, string T1PropertyName, string T2PropertyName = null)
        {
            PerformEqualityAssertionWith(obj1, obj2, T1PropertyName, T2PropertyName, (o1, o2, info) => Assert.AreNotEqual(o1, o2, info));
        }

        private static void PerformEqualityAssertionWith(object obj1, object obj2, string obj1PropName, string obj2PropName, Action<object, object, string> finalAssertion)
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
                Assert.Fail((obj1 == null ? "obj1" : "obj2") + " is null (" + obj1PropName + " => " + obj2PropName + ")");
            }
            var type1 = obj1.GetType();
            var srcPropInfo = type1.GetProperty(obj1PropName);
            Assert.IsNotNull(srcPropInfo, PropNotFoundMessage(type1, obj1PropName));
            var type2 = obj2.GetType();
            var targetPropInfo = type2.GetProperty(obj2PropName);
            Assert.IsNotNull(targetPropInfo, PropNotFoundMessage(type2, obj2PropName));
            Assert.AreEqual(srcPropInfo.PropertyType, targetPropInfo.PropertyType, 
                "Property types for '" + srcPropInfo.Name + "' do not match: " + 
                srcPropInfo.PropertyType.Name + " vs " + targetPropInfo.PropertyType.Name);
            finalAssertion(srcPropInfo.GetValue(obj1, null), targetPropInfo.GetValue(obj2, null), obj1PropName + " => " + obj2PropName);
        }

        private static string PropNotFoundMessage(Type type, string propName)
        {
            return string.Join(string.Empty, "Unable to find property '", propName, "' on type '", type.Name, "'");
        }
    }
}
