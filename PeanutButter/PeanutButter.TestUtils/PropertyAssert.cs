using System;
using System.Linq;
using NUnit.Framework;

namespace PeanutButter.TestUtils.Generic
{
    public class PropertyAssert
    {
        public static void AllPropertiesAreEqual(object obj1, object obj2, params string[] ignorePropertiesByName)
        {
            var propInfos1 = obj1.GetType().GetProperties();
            var propInfos2 = obj2.GetType().GetProperties();
            if (propInfos1.Length != propInfos2.Length)
                Assert.Fail("Property counts on objects of types '" + obj1.GetType().Name + "' and '" + obj2.GetType().Name + "' do not match");
            foreach (var propInfo1 in propInfos1)
            {
                if (ignorePropertiesByName.Contains(propInfo1.Name))
                    continue;
                AreEqual(obj1, obj2, propInfo1.Name);
            }
        }

        public static void MatchingPropertiesAreEqual(object obj1, object obj2)
        {
            var propInfos1 = obj1.GetType().GetProperties();
            var propInfos2 = obj2.GetType().GetProperties();
            var matchingProperties = propInfos1
                                        .Where(pi1 => propInfos2.FirstOrDefault(pi2 => pi1.Name == pi2.Name) != null)
                                        .Select(pi => pi.Name);
            foreach (var propName in matchingProperties)
            {
                AreEqual(obj1, obj2, propName);
            }
        }

        public static void AreEqual(object obj1, object obj2, string obj1PropName, string obj2PropName = null)
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
            Assert.AreEqual(srcPropInfo.PropertyType, targetPropInfo.PropertyType, "Property types for '" + srcPropInfo.Name + "' do not match: " + srcPropInfo.PropertyType.Name + " vs " + targetPropInfo.PropertyType.Name);
            Assert.AreEqual(srcPropInfo.GetValue(obj1,null), targetPropInfo.GetValue(obj2, null), obj1PropName + " => " + obj2PropName);
        }

        public static object ResolveObject(object obj, ref string propName)
        {
            var propParts = propName.Split(new[] { '.' });
            if (propParts.Length == 1 || obj == null)
            {
                return obj;
            }
            propName = String.Join(".", propParts.Skip(1));
            var objType = obj.GetType();
            var propInfo = objType.GetProperty(propParts[0]);
            if (propInfo == null)
            {
                throw new Exception(String.Join("", new[] { "Unable to resolve property '", propName, 
                    "': can't find immediate property '", propParts[0], "' on object of type '", objType.Name, "'" }));
            }
            var propVal = propInfo.GetValue(obj, null);
            if (propVal == null && propName.IndexOf(".") > -1)
            {
                throw new Exception(String.Join("", new[] { "Unable to traverse into property '", propName, "': current object is null" }));
            }
            return ResolveObject(propVal, ref propName);
        }

        public static void AreNotEqual<T1, T2>(T1 obj1, T2 obj2, string T1PropertyName, string T2PropertyName = null)
        {
            if (T2PropertyName == null)
            {
                T2PropertyName = T1PropertyName;
            }
            var type1 = obj1.GetType();
            var srcPropInfo = type1.GetProperty(T1PropertyName);
            Assert.IsNotNull(srcPropInfo, PropNotFoundMessage(type1, T1PropertyName));
            var type2 = obj2.GetType();
            var targetPropInfo = type2.GetProperty(T2PropertyName);
            Assert.IsNotNull(targetPropInfo, PropNotFoundMessage(type2, T2PropertyName));
            Assert.AreNotEqual(srcPropInfo.GetValue(obj1, null), targetPropInfo.GetValue(obj2, null), T1PropertyName + " => " + T2PropertyName);
        }

        private static string PropNotFoundMessage(Type type, string propName)
        {
            return String.Join("", new[] { "Unable to find property '", propName, "' on type '", type.Name, "'" });
        }
    }
}
