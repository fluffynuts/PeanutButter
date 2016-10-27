using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PeanutButter.Utils
{
    public static class ObjectExtensions
    {
        private static readonly Type[] PrimitiveTypes;

        static ObjectExtensions()
        {
            PrimitiveTypes = new[] {
                typeof(int),
                typeof(char),
                typeof(byte),
                typeof(long),
                typeof(string),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(bool),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan)
            };
        }

        [Obsolete("AllPropertiesMatch has been deprecated in favour of the more powerful DeepEquals; use DeepSubEquals if your source properties are potentially a subset of the comparison properties")]
        public static bool AllPropertiesMatch(this object objSource, object objCompare, params string[] ignorePropertiesByName)
        {
            return objSource.DeepEquals(objCompare, ignorePropertiesByName);
        }

        public static bool DeepEquals(this object objSource, object objCompare, params string[] ignorePropertiesByName)
        {
            return new DeepEqualityTester(
                objSource,
                objCompare,
                ignorePropertiesByName
            ).AreDeepEqual();
        }

        public static bool DeepSubEquals(this object objSource, object objCompare, params string[] ignorePropertiesByName)
        {
            var tester = new DeepEqualityTester(
                objSource,
                objCompare,
                ignorePropertiesByName
            );
            tester.FailOnMissingProperties = false;
            return tester.AreDeepEqual();
        }


        public static bool ContainsOneLike<T1, T2>(this IEnumerable<T1> collection, T2 value)
        {
            return collection.Any(i => i.DeepEquals(value));
        }

        public static void CopyPropertiesTo(this object src, object dst)
        {
            src.CopyPropertiesTo(dst, true);
        }

        public static void CopyPropertiesTo(this object src, object dst, bool deep)
        {
            if (src == null || dst == null) return;
            var srcPropInfos = src.GetType().GetProperties();
            var dstPropInfos = dst.GetType().GetProperties();

            foreach (var srcPropInfo in srcPropInfos.Where(pi => pi.CanRead))
            {
                var matchingTarget = dstPropInfos.FirstOrDefault(dp => dp.Name == srcPropInfo.Name && 
                                                                       dp.PropertyType == srcPropInfo.PropertyType &&
                                                                       dp.CanWrite);
                if (matchingTarget == null) continue;

                var srcVal = srcPropInfo.GetValue(src, null);
                if (!deep || IsSimpleTypeOrNullableOfSimpleType(srcPropInfo.PropertyType))
                {
                    matchingTarget.SetValue(dst, srcVal, null);
                }
                else
                {
                    if (srcVal != null)
                    {
                        var targetVal = matchingTarget.GetValue(dst,null);
                        srcVal.CopyPropertiesTo(targetVal);
                    }
                    else
                    {
                        matchingTarget.SetValue(dst, null, null);
                    }
                }
            }
        }

        private static bool IsSimpleTypeOrNullableOfSimpleType(Type t)
        {
            return PrimitiveTypes.Any(si => si == t || 
                                          (t.IsGenericType && 
                                          t.GetGenericTypeDefinition() == typeof(Nullable<>) && 
                                          Nullable.GetUnderlyingType(t) == si));
        }

        public static T Get<T>(this object src, string propertyPath)
        {
            var type = src.GetType();
            return ResolvePropertyValueFor<T>(src, propertyPath, type);
        }

        public static T GetOrDefault<T>(this object src, string propertyPath, T defaultValue = default(T))
        {
            try
            {
                return Get<T>(src, propertyPath);
            }
            catch (PropertyNotFoundException)
            {
                return defaultValue;
            }
        }

        public static T[] AsArray<T>(this T input)
        {
            return new[] {input};
        }


        private static T ResolvePropertyValueFor<T>(object src, string propertyPath, Type type)
        {
            var parts = propertyPath.Split('.');
            var valueAsObject = parts.Aggregate(src, GetPropertyValue);
            var valueType = valueAsObject.GetType();
            if (!valueType.IsAssignableTo<T>())
                throw new ArgumentException(
                    "Get<> must be invoked with a type to which the property value could be assigned ("
                    + type.Name + "." + propertyPath + " has type '" + valueType.Name
                    + "', but expected '" + typeof(T).Name + "' or derivative");
            return (T) valueAsObject;
        }


        public static object GetPropertyValue(this object src, string propertyName)
        {
            var type = src.GetType();
            var propInfo = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(pi => pi.Name == propertyName);
            if (propInfo == null)
                throw new PropertyNotFoundException(type, propertyName);
            return propInfo.GetValue(src, null);
        }

        public static void SetPropertyValue(this object propValue, string propertyPath, object newValue)
        {
            var queue = new Queue<string>(propertyPath.Split('.'));
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var type = propValue.GetType();
                var propInfo = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                    .FirstOrDefault(pi => pi.Name == current);
                if (propInfo == null)
                    throw new PropertyNotFoundException(type, current);
                if (queue.Count == 0)
                {
                    propInfo.SetValue(propValue, newValue);
                    return;
                }
                propValue = propInfo.GetValue(propValue);
            }
            throw new PropertyNotFoundException(propValue.GetType(), propertyPath);
        }

        public static T GetPropertyValue<T>(this object src, string propertyName)
        {
            var objectResult = GetPropertyValue(src, propertyName);
            return (T) objectResult;
        }

        public static bool IsAssignableTo<T>(this Type type)
        {
            return type.IsAssignableFrom(typeof (T));
        }
    }
}
