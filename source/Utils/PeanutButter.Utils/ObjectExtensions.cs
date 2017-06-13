using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides a set of convenience extensions on everything
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Runs a deep equality test between two objects, glossing over reference
        /// differences between class-types and comparing only primitive types. Use
        /// this when you'd like to essentially test whether the data in one object
        /// hierachy matches that of another
        /// </summary>
        /// <param name="objSource">Object which is the source of truth</param>
        /// <param name="objCompare">Object to compare with</param>
        /// <param name="ignorePropertiesByName">Params array of properties to ignore by name</param>
        /// <returns></returns>
        public static bool DeepEquals(this object objSource, object objCompare, params string[] ignorePropertiesByName)
        {
            return new DeepEqualityTester(
                objSource,
                objCompare,
                ignorePropertiesByName
            ).AreDeepEqual();
        }

        /// <summary>
        /// Runs a deep equality test between two objects, using the properties on objSource (and children) as
        /// the set of properties to match on
        /// </summary>
        /// <param name="objSource">Source object to perform comparison against</param>
        /// <param name="objCompare">Comparison object to compare</param>
        /// <param name="ignorePropertiesByName">Optional params array of properties to ignore by name</param>
        /// <returns>True if relevant properties are found and match; false otherwise</returns>
        public static bool DeepSubEquals(this object objSource, object objCompare, params string[] ignorePropertiesByName)
        {
            var tester = new DeepEqualityTester(
                objSource,
                objCompare,
                ignorePropertiesByName
            ) {FailOnMissingProperties = false};
            return tester.AreDeepEqual();
        }

        /// <summary>
        /// Runs a deep equality test between two objects, using the properties common to both sides
        /// of the comparison to match on.
        /// </summary>
        /// <param name="objSource">Source object to perform comparison against</param>
        /// <param name="objCompare">Comparison object to compare</param>
        /// <param name="ignorePropertiesByName">Optional params array of properties to ignore by name</param>
        /// <returns>True if relevant properties are found and match; false otherwise. If no common properties are found, returns false; caveat: performing this comparison on two vanilla Object() instances will return true.</returns>
        public static bool DeepIntersectionEquals(this object objSource, object objCompare, params string[] ignorePropertiesByName)
        {
            var tester = new DeepEqualityTester(
                objSource,
                objCompare,
                ignorePropertiesByName
            ) {OnlyTestIntersectingProperties = true};
            return tester.AreDeepEqual();
        }

        /// <summary>
        /// Searches a collection for one or more objects which DeepEquals the provided reference item
        /// </summary>
        /// <param name="collection">Collection of objects to search</param>
        /// <param name="item">Item to find a match for</param>
        /// <param name="ignoreProperties">Optional params array of properties to ignore by name</param>
        /// <typeparam name="T1">Item type of the collection</typeparam>
        /// <typeparam name="T2">Type of the comparison item (can be the same as or different from T1)</typeparam>
        /// <returns>True if one or more matching objects were found; false otherwise</returns>
        public static bool ContainsAtLeastOneDeepEqualTo<T1, T2>(
            this IEnumerable<T1> collection, 
            T2 item,
            params string[] ignoreProperties)
        {
            return collection.Any(i => i.DeepEquals(item, ignoreProperties));
        }

        /// <summary>
        /// Searches a collection for a single object which DeepEquals the provided reference item
        /// </summary>
        /// <param name="collection">Collection of objects to search</param>
        /// <param name="item">Item to find a match for</param>
        /// <param name="ignoreProperties">Optional params array of properties to ignore by name</param>
        /// <typeparam name="T1">Item type of the collection</typeparam>
        /// <typeparam name="T2">Type of the comparison item (can be the same as or different from T1)</typeparam>
        /// <returns>True if one or more matching objects were found; false otherwise</returns>
        public static bool ContainsOneDeepEqualTo<T1, T2>(
            this IEnumerable<T1> collection, 
            T2 item,
            params string[] ignoreProperties)
        {
            return collection.Count(i => i.DeepEquals(item, ignoreProperties)) == 1;
        }

        /// <summary>
        /// Searches a collection for an object which IntersectionEquals the provided reference item
        /// </summary>
        /// <param name="collection">Collection of objects to search</param>
        /// <param name="item">Item to find a match for</param>
        /// <param name="ignoreProperties">Optional params array of properties to ignore by name</param>
        /// <typeparam name="T1">Item type of the collection</typeparam>
        /// <typeparam name="T2">Type of the comparison item (can be the same as or different from T1)</typeparam>
        /// <returns>True if one or more matching objects were found; false otherwise</returns>
        public static bool ContainsOneIntersectionEqualTo<T1, T2>(
            this IEnumerable<T1> collection,
            T2 item,
            params string[] ignoreProperties
        )
        {
            return collection.Any(i => i.DeepIntersectionEquals(item, ignoreProperties));
        }

        /// <summary>
        /// Searches a collection for an object which DeepEquals the provided reference item
        /// </summary>
        /// <param name="collection">Collection of objects to search</param>
        /// <param name="item">Item to find a match for</param>
        /// <param name="ignoreProperties">Optional params array of properties to ignore by name</param>
        /// <typeparam name="T1">Item type of the collection</typeparam>
        /// <typeparam name="T2">Type of the comparison item (can be the same as or different from T1)</typeparam>
        /// <returns>True if exactly one matching object was found; false otherwise</returns>
        public static bool ContainsOnlyOneDeepEqualTo<T1, T2>(
            this IEnumerable<T1> collection, 
            T2 item,
            params string[] ignoreProperties)
        {
            return collection.ContainsOnlyOneMatching(item, 
                (t1, t2) => t1.DeepEquals(t2, ignoreProperties));
        }

        /// <summary>
        /// Searches a collection for an object which IntersectionEquals the provided reference item
        /// </summary>
        /// <param name="collection">Collection of objects to search</param>
        /// <param name="item">Item to find a match for</param>
        /// <param name="ignoreProperties">Optional params array of properties to ignore by name</param>
        /// <typeparam name="T1">Item type of the collection</typeparam>
        /// <typeparam name="T2">Type of the comparison item (can be the same as or different from T1)</typeparam>
        /// <returns>True if exactly one matching object was found; false otherwise</returns>
        public static bool ContainsOnlyOneIntersectionEqualTo<T1, T2>(
            this IEnumerable<T1> collection,
            T2 item,
            params string[] ignoreProperties
        )
        {
            return collection.ContainsOnlyOneMatching(item, 
                (t1, t2) => t1.DeepIntersectionEquals(t2, ignoreProperties));
        }

        /// <summary>
        /// Searches a collection for an object which matches the provided reference item, according
        /// to the provided matcher Func
        /// </summary>
        /// <param name="collection">Collection of objects to search</param>
        /// <param name="item">Item to find a match for</param>
        /// <param name="comparer">Func to use to perform comparison</param>
        /// <typeparam name="T1">Item type of the collection</typeparam>
        /// <typeparam name="T2">Type of the comparison item (can be the same as or different from T1)</typeparam>
        /// <returns>True if exactly one matching object was found; false otherwise</returns>
        public static bool ContainsOnlyOneMatching<T1, T2>(
            this IEnumerable<T1> collection,
            T2 item,
            Func<T1, T2, bool> comparer
        )
        {
            return collection.Aggregate(0, (acc, cur) =>
            {
                if (acc > 1) return acc;
                acc += comparer(cur, item) ? 1 : 0;
                return acc;
            }) == 1;
        }

        /// <summary>
        /// Copies all public primitive property values of intersecting properties from the source object
        /// to the target object, ala poor-man's AutoMapper
        /// </summary>
        /// <param name="src">Source object</param>
        /// <param name="dst">Target object</param>
        public static void CopyPropertiesTo(this object src, object dst)
        {
            src.CopyPropertiesTo(dst, true);
        }


        /// <summary>
        /// Copies all public primitive property values of intersecting properties from the source object
        /// to the target object, ala poor-man's AutoMapper
        /// </summary>
        /// <param name="src">Source object</param>
        /// <param name="dst">Target object</param>
        /// <param name="ignoreProperties">Optional list of properties to ignore by name</param>
        public static void CopyPropertiesTo(this object src, object dst, params string[] ignoreProperties)
        {
            src.CopyPropertiesTo(dst, true, ignoreProperties);
        }

        /// <summary>
        /// Copies all public primitive property values of intersecting properties from the source object
        /// to the target object, ala poor-man's AutoMapper
        /// </summary>
        /// <param name="src">Source object</param>
        /// <param name="dst">Target object</param>
        /// <param name="deep">Flag as to whether or not the process should copy deep (ie, traverse into child objects)</param>
        /// <param name="ignoreProperties"></param>
        public static void CopyPropertiesTo(this object src, object dst, bool deep, params string[] ignoreProperties)
        {
            if (src == null || dst == null) return;
            var srcPropInfos = src.GetType().GetProperties()
                                .Where(pi => !ignoreProperties.Contains(pi.Name));
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
                else if (srcPropInfo.PropertyType.IsArrayOrAssignableFromArray())
                {
                    var underlyingType = srcPropInfo.PropertyType.GetCollectionItemType();
                    if (underlyingType != null)
                    {
                        var specific = _genericMakeArrayCopy.MakeGenericMethod(underlyingType);
                        // ReSharper disable once RedundantExplicitArrayCreation
                        var newValue = specific.Invoke(null, new object[] { srcVal });
                        matchingTarget.SetValue(dst, newValue);
                    }
                }
                else
                {
                    if (srcVal != null)
                    {
                        var targetVal = matchingTarget.GetValue(dst, null);
                        srcVal.CopyPropertiesTo(targetVal);
                    }
                    else
                    {
                        matchingTarget.SetValue(dst, null, null);
                    }
                }
            }
        }

        private static readonly MethodInfo _genericMakeArrayCopy
            = typeof(ObjectExtensions).GetMethod(nameof(MakeArrayCopyOf), 
                BindingFlags.NonPublic | BindingFlags.Static);

#pragma warning disable S1144 // Unused private types or members should be removed
        // ReSharper disable once UnusedMember.Local
        private static T[] MakeArrayCopyOf<T>(IEnumerable<T> src)
        {
            try
            {
                // ReSharper disable once PossibleMultipleEnumeration
                var result = new T[src?.Count() ?? 0];
                if (src != null)
                {
                    var idx = 0;
                    // ReSharper disable once PossibleMultipleEnumeration
                    foreach (var item in src)
                    {
                        result[idx++] = item;
                    }
                }
                return result;
            }
            catch
            {
                return null;
            }
        }
#pragma warning restore S1144 // Unused private types or members should be removed

        private static bool IsSimpleTypeOrNullableOfSimpleType(Type t)
        {
            return Types.Primitives.Any(si => si == t ||
                                          (t.IsGenericType &&
                                          t.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                                          Nullable.GetUnderlyingType(t) == si));
        }

        /// <summary>
        /// Gets the value of a property on an object, specified by the property path, of the given Type
        /// </summary>
        /// <param name="src">Object to search for the required property</param>
        /// <param name="propertyPath">Path to the property: may be a property name or a dotted path down an object heirachy, eg: Company.Name</param>
        /// <typeparam name="T">Expected type of the property value</typeparam>
        /// <returns></returns>
        public static T Get<T>(this object src, string propertyPath)
        {
            var type = src.GetType();
            return ResolvePropertyValueFor<T>(src, propertyPath, type);
        }

        /// <summary>
        /// Gets the value of a property on an object, specified by the property path, of the given Type
        /// or returns a default value when that property cannot be found by path and/or type
        /// </summary>
        /// <param name="src"></param>
        /// <param name="propertyPath"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetOrDefault<T>(this object src, string propertyPath)
        {
            return src.GetOrDefault(propertyPath, default(T));
        }

        /// <summary>
        /// Gets the value of a property on an object, specified by the property path, of the given Type
        /// or returns a default value when that property cannot be found by path and/or type
        /// </summary>
        /// <param name="src"></param>
        /// <param name="propertyPath"></param>
        /// <param name="defaultValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetOrDefault<T>(this object src, string propertyPath, T defaultValue)
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

        /// <summary>
        /// Fluency extension to wrap a single item in an array, eg:
        /// new SomeBusinessObject().AsArray().Union(SomeOtherCollection);
        /// </summary>
        /// <param name="input">The item to wrap</param>
        /// <typeparam name="T">The type of the object</typeparam>
        /// <returns>A single-element array containing the input object</returns>
        public static T[] AsArray<T>(this T input)
        {
            return new[] { input };
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
            return (T)valueAsObject;
        }


        /// <summary>
        /// Gets a property value by name from an object
        /// </summary>
        /// <param name="src">Source object</param>
        /// <param name="propertyName">Name of the property to search for</param>
        /// <returns>Value of the property, cast/boxed to object</returns>
        /// <exception cref="PropertyNotFoundException">Thrown when the property is not found by name</exception>
        public static object GetPropertyValue(this object src, string propertyName)
        {
            var type = src.GetType();
            var propInfo = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault(pi => pi.Name == propertyName);
            if (propInfo == null)
                throw new PropertyNotFoundException(type, propertyName);
            return propInfo.GetValue(src, null);
        }

        /// <summary>
        /// Invokes a method on an object, if available; otherwise 'splodes
        /// </summary>
        /// <param name="src">Object to invoke the method on</param>
        /// <param name="methodName">Method to invoke, by name</param>
        /// <param name="args">Any parameters to give to the method</param>
        /// <returns>return value of the method</returns>
        public static object InvokeMethodWithResult(this object src, string methodName, params object[] args)
        {
            var srcType = src.GetType();
            var method = srcType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
                throw new InvalidOperationException($"Can't find method {methodName} on {srcType.Name}");
            return method.Invoke(src, args);
        }

        /// <summary>
        /// Attempts to set a property value on an object by property path
        /// </summary>
        /// <param name="src">Source object to set property on</param>
        /// <param name="propertyPath">Path into the property: could be an immediate property name or something like "Company.Name"</param>
        /// <param name="newValue">New value to attempt to set the property to</param>
        /// <exception cref="PropertyNotFoundException">Thrown when the property cannot be found</exception>
        public static void SetPropertyValue(this object src, string propertyPath, object newValue)
        {
            var queue = new Queue<string>(propertyPath.Split('.'));
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var type = src.GetType();
                var propInfo = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                    .FirstOrDefault(pi => pi.Name == current);
                if (propInfo == null)
                    throw new PropertyNotFoundException(type, current);
                if (queue.Count == 0)
                {
                    propInfo.SetValue(src, newValue);
                    return;
                }
                src = propInfo.GetValue(src);
            }
            throw new PropertyNotFoundException(src.GetType(), propertyPath);
        }

        /// <summary>
        /// Gets an immediate property value, cast to the specified type
        /// </summary>
        /// <param name="src">Source object</param>
        /// <param name="propertyName">Immediate property name</param>
        /// <typeparam name="T">Required type</typeparam>
        /// <returns>Value of the property, if it can be found and cast. Will throw otherwise.</returns>
        public static T GetPropertyValue<T>(this object src, string propertyName)
        {
            var objectResult = GetPropertyValue(src, propertyName);
            return (T)objectResult;
        }

        /// <summary>
        /// Tests if a type is assignable to another type (inverse of IsAssignableFrom)
        /// </summary>
        /// <param name="type">Type to operate on</param>
        /// <typeparam name="T">Type to check assignment possibility against</typeparam>
        /// <returns>True if objects of type {type} can be assigned to objects of type T</returns>
        public static bool IsAssignableTo<T>(this Type type)
        {
            return type.IsAssignableFrom(typeof(T));
        }

        /// <summary>
        /// Truncates a decimal value to a required number of places
        /// </summary>
        /// <param name="value">Source decimal value</param>
        /// <param name="places">Number of decimal places required</param>
        /// <returns>A new decimal value which is the original value truncated to the required places</returns>
        public static decimal TruncateTo(this decimal value, int places)
        {
            var mul = new decimal(Math.Pow(10, places));
            return Math.Truncate(value * mul) / mul;
        }
    }
}
