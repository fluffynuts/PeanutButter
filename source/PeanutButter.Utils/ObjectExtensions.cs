using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace PeanutButter.Utils
{
    public static class ObjectExtensions
    {
        private static Type[] _simpleTypes;

        static ObjectExtensions()
        {
            _simpleTypes = new[] {
                typeof(int),
                typeof(char),
                typeof(byte),
                typeof(long),
                typeof(string),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(bool),
                typeof(DateTime)
            };
        }

        public static bool AllPropertiesMatch(this object objSource, object objCompare, params string[] ignorePropertiesByName)
        {
            if (objSource == null && objCompare == null) return true;
            if (objSource == null || objCompare == null) return false;
            var srcPropInfos = objSource.GetType()
                                        .GetProperties()
                                        .Where(pi => !ignorePropertiesByName.Contains(pi.Name));
            var comparePropInfos = objCompare.GetType().GetProperties();
            return srcPropInfos.Aggregate(true, (result, srcProp) =>
            {
                if (!result) return false;
                var compareProp = FindMatchingPropertyInfoFor(comparePropInfos, srcProp);
                return compareProp != null &&
                         PropertyValuesMatchFor(objSource, objCompare, srcProp, compareProp);
            });
        }

        private static bool PropertyValuesMatchFor(object objSource, object objCompare, PropertyInfo srcProp, PropertyInfo compareProp)
        {
            var srcValue = srcProp.GetValue(objSource, null);
            var compareValue = compareProp.GetValue(objCompare, null);
            return CanPerformSimpleTypeMatchFor(srcProp)
                ? SimpleObjectsMatch(srcProp.Name, srcValue, compareValue)
                : srcValue.AllPropertiesMatch(compareValue);
        }

        private static bool CanPerformSimpleTypeMatchFor(PropertyInfo srcProp)
        {
            return _simpleTypes.Any(st => st == srcProp.PropertyType);
        }

        private static bool SimpleObjectsMatch(string propertyName, object srcValue, object compareValue)
        {
            var srcString = StringOf(srcValue);
            var compareString = StringOf(compareValue);
            if (srcString != compareString)
            {
                Debug.WriteLine(propertyName + " value mismatch: (" + (srcString ?? "NULL") + ") vs (" + (compareString ?? "NULL") + ")");
                return false;
            }
            return true;
        }

        private static PropertyInfo FindMatchingPropertyInfoFor(IEnumerable<PropertyInfo> properties, PropertyInfo srcProp)
        {
            var comparePropInfo = properties.FirstOrDefault(pi => pi.Name == srcProp.Name);
            if (comparePropInfo == null)
            {
                Debug.WriteLine("Unable to find comparison property with name: '" + srcProp.Name + "'");
                return null;
            }
            if (comparePropInfo.PropertyType != srcProp.PropertyType)
            {
                Debug.WriteLine("Source property has type '" + srcProp.PropertyType.Name + "' but comparison property has type '" + comparePropInfo.PropertyType.Name + "'");
                return null;
            }
            return comparePropInfo;
        }

        public static void CopyPropertiesTo(this object src, object dst, bool deep = true)
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
            return _simpleTypes.Any(si => si == t || 
                                          (t.IsGenericType && 
                                          t.GetGenericTypeDefinition() == typeof(Nullable<>) && 
                                          Nullable.GetUnderlyingType(t) == si));
        }

        private static string StringOf(object srcValue)
        {
            return srcValue == null ? "[null]" : srcValue.ToString();
        }

        public static T Get<T>(this object src, string propertyName, T defaultValue = default(T))
        {
            var type = src.GetType();
            var parts = propertyName.Split('.');
            try
            {
                return ResolvePropertyValueFor<T>(src, propertyName, parts, type);
            }
            catch (PropertyNotFoundException)
            {
                return defaultValue;
            }
        }

        private static T ResolvePropertyValueFor<T>(object src, string propertyName, string[] parts, Type type)
        {
            var valueAsObject = parts.Aggregate(src, GetPropertyValue);
            var valueType = valueAsObject.GetType();
            if (!valueType.IsAssignableTo<T>())
                throw new ArgumentException(
                    "Get<> must be invoked with a type to which the property value could be assigned ("
                    + type.Name + "." + propertyName + " has type '" + valueType.Name
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

        public static T GetPropertyValue<T>(this object src, string propertyName)
        {
            var objectResult = GetPropertyValue(src, propertyName);
            return (T) objectResult;
        }

        public static bool IsAssignableTo<T>(this Type type)
        {
            return type.IsAssignableFrom(typeof (T));
        }

        public static void SetPropertyValue(this object obj, string propertyName, object newValue)
        {
            var type = obj.GetType();   
            var propInfo = type.GetProperty(propertyName);
            if (propInfo == null)
                throw new ArgumentException($"{type.Name} has no public property called {propertyName}");
            if (!propInfo.CanWrite)
                throw new ArgumentException($"{propertyName} on {type.Name} is read-only");
            propInfo.SetValue(obj, newValue, null);
        }
    }
}
