using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

// ReSharper disable MemberCanBePrivate.Global
#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Determines the comparison strategy for DeepEquals and friends
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        enum ObjectComparisons
    {
        /// <summary>
        /// Test properties and fields (default behavior for DeepEquals)
        /// </summary>
        PropertiesAndFields,

        /// <summary>
        /// Only test properties (behavior for PropertyAssert)
        /// </summary>
        PropertiesOnly
    }

    /// <summary>
    /// Provides a set of convenience extensions on everything
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class ObjectExtensions
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
        public static bool DeepEquals(
            this object objSource,
            object objCompare,
            params string[] ignorePropertiesByName
        )
        {
            return objSource.DeepEquals(
                objCompare,
                ObjectComparisons.PropertiesAndFields,
                ignorePropertiesByName);
        }

        /// <summary>
        /// Tests if two objects have the same "shape" -- basically Deep Equality testing
        /// without actually testing final property values.
        /// </summary>
        /// <param name="objSource">Source / Master object</param>
        /// <param name="objCompare">Comparison object</param>
        /// <param name="ignorePropertiesByName">Ignore these properties by name</param>
        /// <returns>True if the "shapes" are the same, false otherwise</returns>
        public static bool ShapeEquals(
            this object objSource,
            object objCompare,
            params string[] ignorePropertiesByName
        )
        {
            return PerformShapeEqualityTesting(
                objSource,
                objCompare,
                false,
                ignorePropertiesByName
            );
        }

        /// <summary>
        /// Tests if a second object has at least the "shape" of a primary one. Basically
        /// a DeepSubEquals without testing final property values.
        /// without actually testing final property values.
        /// </summary>
        /// <param name="objSource">Source / Master object</param>
        /// <param name="objCompare">Comparison object</param>
        /// <param name="ignorePropertiesByName">Ignore these properties by name</param>
        /// <returns>True if the comparison object "contains the shape" of the source object, false otherwise</returns>
        public static bool ShapeSubEquals(
            this object objSource,
            object objCompare,
            params string[] ignorePropertiesByName
        )
        {
            return PerformShapeEqualityTesting(
                objSource,
                objCompare,
                true,
                ignorePropertiesByName
            );
        }

        private static bool PerformShapeEqualityTesting(
            object left,
            object right,
            bool allowMissingProperties,
            params string[] ignorePropsByName
        )
        {
            var tester = new DeepEqualityTester(left, right, ignorePropsByName)
            {
                OnlyCompareShape = true,
                IncludeFields = true,
                FailOnMissingProperties = !allowMissingProperties
            };
            return tester.AreDeepEqual();
        }

        /// <summary>
        /// Runs a deep equality test between two objects, glossing over reference
        /// differences between class-types and comparing only primitive types. Use
        /// this when you'd like to essentially test whether the data in one object
        /// hierachy matches that of another
        /// </summary>
        /// <param name="objSource">Object which is the source of truth</param>
        /// <param name="objCompare">Object to compare with</param>
        /// <param name="comparison">Method for comparison</param>
        /// <param name="ignorePropertiesByName">Params array of properties to ignore by name</param>
        /// <returns></returns>
        public static bool DeepEquals(
            this object objSource,
            object objCompare,
            ObjectComparisons comparison,
            params string[] ignorePropertiesByName
        )
        {
            var tester = new DeepEqualityTester(
                objSource,
                objCompare,
                ignorePropertiesByName
            ) { IncludeFields = comparison == ObjectComparisons.PropertiesAndFields };
            return tester.AreDeepEqual();
        }

        /// <summary>
        /// Runs a deep equality test between two objects,
        /// ignoring reference differences wherever possible
        /// and logging failures with the provided action. Properties
        /// can be explided by name with the ignorePropertiesByName params
        /// </summary>
        /// <param name="objSource"></param>
        /// <param name="objCompare"></param>
        /// <param name="failureLogAction"></param>
        /// <param name="ignorePropertiesByName"></param>
        /// <returns></returns>
        public static bool DeepEquals(this object objSource,
            object objCompare,
            Action<string> failureLogAction,
            params string[] ignorePropertiesByName)
        {
            var tester = new DeepEqualityTester(
                objSource, objCompare, ignorePropertiesByName)
            {
                RecordErrors = true
            };
            var result = tester.AreDeepEqual();
            tester.Errors.ForEach(failureLogAction);
            return result;
        }

        /// <summary>
        /// Runs a deep equality test between two objects, using the properties on objSource (and children) as
        /// the set of properties to match on
        /// </summary>
        /// <param name="objSource">Source object to perform comparison against</param>
        /// <param name="objCompare">Comparison object to compare</param>
        /// <param name="ignorePropertiesByName">Optional params array of properties to ignore by name</param>
        /// <returns>True if relevant properties are found and match; false otherwise</returns>
        public static bool DeepSubEquals(
            this object objSource,
            object objCompare,
            params string[] ignorePropertiesByName)
        {
            var tester = new DeepEqualityTester(
                objSource,
                objCompare,
                ignorePropertiesByName
            ) { FailOnMissingProperties = false };
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
        public static bool DeepIntersectionEquals(
            this object objSource,
            object objCompare,
            params string[] ignorePropertiesByName)
        {
            var tester = new DeepEqualityTester(
                objSource,
                objCompare,
                ignorePropertiesByName
            ) { OnlyTestIntersectingProperties = true };
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
            return collection.ContainsOnlyOneMatching(
                item,
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
            return collection.ContainsOnlyOneMatching(
                item,
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
            return collection.Aggregate(
                    0,
                    (acc, cur) =>
                    {
                        if (acc > 1)
                            return acc;
                        acc += comparer(cur, item)
                            ? 1
                            : 0;
                        return acc;
                    }) ==
                1;
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
            if (src == null || dst == null)
            {
                return;
            }

            var srcPropInfos = src.GetType()
                .GetProperties()
                .Where(pi => !ignoreProperties.Contains(pi.Name));
            var dstPropInfos = dst.GetType().GetProperties();

            foreach (var srcPropInfo in srcPropInfos.Where(
                pi => pi.CanRead &&
                    pi.GetIndexParameters().Length == 0))
            {
                var matchingTarget = dstPropInfos.FirstOrDefault(
                    dp => dp.Name == srcPropInfo.Name &&
                        dp.PropertyType == srcPropInfo.PropertyType &&
                        dp.CanWrite);
                if (matchingTarget == null)
                    continue;

                var srcVal = srcPropInfo.GetValue(src);

                _propertySetterStrategies.Aggregate(
                    false,
                    (acc, cur) =>
                        acc || cur(deep, srcPropInfo, matchingTarget, dst, srcVal));
            }
        }

        private static readonly Func<bool, PropertyInfo, PropertyInfo, object, object, bool>[]
            _propertySetterStrategies =
            {
                SetSimpleOrNullableOfSimpleTypeValue,
                SetArrayOrGenericIEnumerableValue,
                SetGenericListValue,
                SetEnumValue,
                DefaultSetValue
            };

        private static bool SetEnumValue(
            bool deep,
            PropertyInfo srcPropertyInfo,
            PropertyInfo dstPropertyInfo,
            object dst,
            object srcValue)
        {
            if (!srcPropertyInfo.PropertyType.IsEnum())
                return false;
            dstPropertyInfo.SetValue(dst, srcValue);

            return true;
        }


        private static bool DefaultSetValue(
            bool deep,
            PropertyInfo srcPropertyInfo,
            PropertyInfo dstPropertyInfo,
            object dst,
            object srcVal
        )
        {
            if (srcVal != null)
            {
                var clone = srcVal.DeepCloneInternal(srcPropertyInfo.PropertyType);
                dstPropertyInfo.SetValue(dst, clone);
                return true;
            }

            dstPropertyInfo.SetValue(dst, null, null);
            return false;
        }

        private static bool SetGenericListValue(
            bool deep,
            PropertyInfo srcPropertyInfo,
            PropertyInfo dstPropertyInfo,
            object dst,
            object srcVal
        )
        {
            if (!srcPropertyInfo.PropertyType.IsGenericOf(typeof(List<>)))
                return false;
            var itemType = srcPropertyInfo.PropertyType.GetCollectionItemType();
            if (itemType == null)
                return false;
            var method = _genericMakeListCopy.MakeGenericMethod(itemType);
            var newValue = method.Invoke(null, new[] { srcVal });
            dstPropertyInfo.SetValue(dst, newValue);
            return true;
        }


        private static bool SetArrayOrGenericIEnumerableValue(
            bool deep,
            PropertyInfo srcPropertyInfo,
            PropertyInfo dstPropertyInfo,
            object dst,
            object srcVal
        )
        {
            if (!srcPropertyInfo.PropertyType.IsArrayOrAssignableFromArray())
                return false;
            var underlyingType = srcPropertyInfo.PropertyType.GetCollectionItemType();
            if (underlyingType == null)
                return false;
            var specific = _genericMakeArrayCopy.MakeGenericMethod(underlyingType);
            // ReSharper disable once RedundantExplicitArrayCreation
            var newValue = specific.Invoke(null, new[] { srcVal });
            dstPropertyInfo.SetValue(dst, newValue);
            return true;
        }


        private static bool SetSimpleOrNullableOfSimpleTypeValue(
            bool deep,
            PropertyInfo srcPropertyInfo,
            PropertyInfo dstPropertyInfo,
            object dst,
            object srcVal
        )
        {
            if (deep && !IsSimpleTypeOrNullableOfSimpleType(srcPropertyInfo.PropertyType))
                return false;
            dstPropertyInfo.SetValue(dst, srcVal);
            return true;
        }

        private static readonly BindingFlags _privateStatic =
            BindingFlags.NonPublic | BindingFlags.Static;

        private static readonly MethodInfo _genericMakeArrayCopy
            = typeof(ObjectExtensions).GetMethod(
                nameof(MakeArrayCopyOf),
                _privateStatic);

        private static readonly MethodInfo _genericMakeListCopy
            = typeof(ObjectExtensions).GetMethod(
                nameof(MakeListCopyOf),
                _privateStatic);

        private static readonly MethodInfo _genericMakeDictionaryCopy
            = typeof(ObjectExtensions).GetMethod(
                nameof(MakeDictionaryCopyOf),
                _privateStatic);

#pragma warning disable S1144 // Unused private types or members should be removed
        // ReSharper disable once UnusedMember.Local
        private static T[] MakeArrayCopyOf<T>(IEnumerable<T> src)
        {
            return MakeListCopyOf(src)?.ToArray();
        }

        private static List<T> MakeListCopyOf<T>(IEnumerable<T> src)
        {
            try
            {
                if (src == null)
                    return null;
                // ReSharper disable once PossibleMultipleEnumeration
                var result = new List<T>();
                // ReSharper disable once PossibleMultipleEnumeration
                result.AddRange(src.Select(item => item.DeepClone()));
                return result;
            }
            catch
            {
                return null;
            }
        }

        private static IDictionary<TKey, TValue> MakeDictionaryCopyOf<TKey, TValue>(
            IDictionary<TKey, TValue> src,
            Type targetType
        )
        {
            var instance = Activator.CreateInstance(targetType) as IDictionary<TKey, TValue>;
            if (instance == null)
                throw new InvalidOperationException($"Activator couldn't create instance of {targetType}");
            src?.ForEach(kvp => instance.Add(new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value)));
            return instance;
        }

#pragma warning restore S1144 // Unused private types or members should be removed

        /// <summary>
        /// Creates a deep clone of the provided item, as far as possible
        /// Works on properties which are:
        ///  * simple values,
        ///  * any complex, non-generic value with a parameterless constructor
        ///  * Collections which are arrays, generic IEnumerable or generic List,
        ///      conforming to the rules above
        /// </summary>
        /// <param name="item">Item to clone</param>
        /// <typeparam name="T">Type of the item to clone</typeparam>
        /// <returns>a new copy of the original item</returns>
        public static T DeepClone<T>(this T item)
        {
            return item == null || item.Equals(default(T))
                ? default(T)
                : (T) item.DeepCloneInternal(item.GetType());
        }

        private static object DeepCloneInternal(
            this object src,
            Type cloneType
        )
        {
            if (src == null)
                return null;
            try
            {
                if (Types.PrimitivesAndImmutables.Contains(cloneType))
                {
                    // FIXME: can we get new instances for Dates and such?
                    return src;
                }

                return _cloneStrategies.Aggregate(
                    null as object,
                    (acc, cur) => acc ?? cur(src, cloneType));
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Unable to clone item of type {cloneType.Name}: {e.Message}");
                return cloneType.DefaultValue();
            }
        }

        private static readonly Func<object, Type, object>[] _cloneStrategies =
        {
            CloneArray,
            CloneList,
            CloneDictionary,
            CloneEnumerable,
            CloneObject
        };

        private static object CloneDictionary(object src, Type cloneType)
        {
            if (!cloneType.TryGetDictionaryKeyAndValueTypes(
                out var keyType,
                out var valueType))
            {
                return null;
            }

            var method = _genericMakeDictionaryCopy.MakeGenericMethod(keyType, valueType);
            return method.Invoke(null, new[] { src, cloneType });
        }

        private static object CloneEnumerable(object src, Type cloneType)
        {
            if (!cloneType.ImplementsEnumerableGenericType())
                return null;
            var itemType = cloneType.GetCollectionItemType();
            var method = _genericMakeArrayCopy.MakeGenericMethod(itemType);
            return method.Invoke(null, new[] { src });
        }

        private static object CloneList(object src, Type cloneType)
        {
            if (!cloneType.IsGenericOf(typeof(List<>)) && !cloneType.IsGenericOf(typeof(IList<>)))
                return null;
            var itemType = cloneType.GetCollectionItemType();
            var method = _genericMakeListCopy.MakeGenericMethod(itemType);
            return method.Invoke(null, new[] { src });
        }

        private static object CloneArray(object src, Type cloneType)
        {
            if (!cloneType.IsArray)
                return null;
            var itemType = cloneType.GetCollectionItemType();
            var method = _genericMakeArrayCopy.MakeGenericMethod(itemType);
            return method.Invoke(null, new[] { src });
        }

        private static object CloneObject(object src, Type cloneType)
        {
            try
            {
                var newInstance = Activator.CreateInstance(cloneType);
                src.CopyPropertiesTo(newInstance);
                return newInstance;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Unable to clone object of type {cloneType.Name}: {e.Message}");
                return cloneType.DefaultValue();
            }
        }

        private static bool IsSimpleTypeOrNullableOfSimpleType(Type t)
        {
            return Types.PrimitivesAndImmutables.Any(
                si => si == t ||
                    (t.IsGenericType() &&
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
        public static T Get<T>(
            this object src,
            string propertyPath)
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
        public static T GetOrDefault<T>(
            this object src,
            string propertyPath)
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
        public static T GetOrDefault<T>(
            this object src,
            string propertyPath,
            T defaultValue)
        {
            try
            {
                return Get<T>(src, propertyPath);
            }
            catch (MemberNotFoundException)
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

        private static T ResolvePropertyValueFor<T>(
            object src,
            string propertyPath,
            Type type)
        {
            var valueAsObject = GetPropertyValue(src, propertyPath);
            if (valueAsObject is null)
            {
                if (type.IsNullableType())
                {
                    return default;
                }

                throw new InvalidOperationException(
                    $"value for {propertyPath} is null but {typeof(T)} is not nullable"
                );
            }

            var valueType = valueAsObject.GetType();
            if (!valueType.IsAssignableTo<T>())
                throw new ArgumentException(
                    "Get<> must be invoked with a type to which the property value could be assigned (" +
                    type.Name +
                    "." +
                    propertyPath +
                    " has type '" +
                    valueType.Name +
                    "', but expected '" +
                    typeof(T).Name +
                    "' or derivative");
            return (T) valueAsObject;
        }


        /// <summary>
        /// Gets a property value by name from an object
        /// </summary>
        /// <param name="src">Source object</param>
        /// <param name="propertyPath">Name of the property to search for</param>
        /// <returns>Value of the property, cast/boxed to object</returns>
        /// <exception cref="MemberNotFoundException">Thrown when the property is not found by name</exception>
        public static object GetPropertyValue(
            this object src,
            string propertyPath)
        {
            var trailingMember = FindPropertyOrField(
                src,
                propertyPath,
                AnyInstanceProperty,
                AnyInstanceField
            );
            if (!trailingMember.Found)
            {
                throw new MemberNotFoundException(src?.GetType(), propertyPath);
            }

            return trailingMember.GetValue();
        }

        private static PropertyOrField[] AnyInstanceProperty(
            Type type)
        {
            var ancestry = type.Ancestry()
                .Reverse()
                .Select((t, idx) => new { type = t, idx })
                .ToDictionary(o => o.type, o => o.idx);
            var result = type.GetProperties(AllOnInstance)
                .Where(pi => !(pi.DeclaringType is null))
                .Select(pi => new { pi, idx = ancestry[pi.DeclaringType] })
                .OrderBy(o => o.pi.Name)
                .ThenBy(o => o.idx)
                .Select(o => o.pi)
                .ImplicitCast<PropertyOrField>()
                .ToArray();
            return result;
        }

        private static PropertyOrField[] AnyInstanceField(
            Type type)
        {
            return type.GetFields(AllOnInstance)
                .ImplicitCast<PropertyOrField>()
                .ToArray();
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
            var method = srcType.GetMethod(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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
        /// <exception cref="MemberNotFoundException">Thrown when the property cannot be found</exception>
        public static void SetPropertyValue(
            this object src,
            string propertyPath,
            object newValue)
        {
            src.SetPropertyOrFieldValue(
                propertyPath,
                newValue);
        }

        private static void SetPropertyOrFieldValue(
            this object src,
            string propertyPath,
            object newValue)
        {
            var trailingMember = FindPropertyOrField(
                src,
                propertyPath,
                AnyInstanceProperty,
                AnyInstanceField);

            if (!trailingMember.Found)
            {
                throw new MemberNotFoundException(src.GetType(), propertyPath);
            }

            trailingMember.SetValue(newValue);
        }

        private class TrailingMember
        {
            public bool Found => Member != null;
            public PropertyOrField Member { get; set; }
            public object Host { get; set; }

            public object GetValue()
            {
                return Member.GetValue(Host);
            }

            public void SetValue(object value)
            {
                Member.SetValue(Host, value);
            }
        }

        private static TrailingMember FindPropertyOrField(
            object src,
            string propertyPath,
            params Func<Type, PropertyOrField[]>[] fetchers)
        {
            var queue = new Queue<string>(propertyPath.Split('.'));
            var result = new TrailingMember();
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var type = src.GetType();
                var memberInfo = fetchers.Aggregate(
                    null as PropertyOrField,
                    (acc, cur) => acc ?? cur(type).FirstOrDefault(mi => mi.Name == current)
                );
                if (memberInfo == null)
                {
                    throw new MemberNotFoundException(type, current);
                }

                if (queue.Count == 0)
                {
                    result.Member = memberInfo;
                    result.Host = src;
                    break;
                }

                src = memberInfo.GetValue(src);
            }

            return result;
        }

        /// <summary>
        /// Attempts to set a property value on an object by property path
        /// </summary>
        /// <param name="src"></param>
        /// <param name="propertyPath"></param>
        /// <param name="newValue"></param>
        /// <typeparam name="T"></typeparam>
        public static void Set<T>(
            this object src,
            string propertyPath,
            T newValue)
        {
            src.SetPropertyValue(propertyPath, newValue);
        }

        private static readonly BindingFlags AllOnInstance =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        /// <summary>
        /// Gets an immediate property value, cast to the specified type
        /// </summary>
        /// <param name="src">Source object</param>
        /// <param name="propertyPath">Immediate property name</param>
        /// <typeparam name="T">Required type</typeparam>
        /// <returns>Value of the property, if it can be found and cast. Will throw otherwise.</returns>
        public static T GetPropertyValue<T>(this object src, string propertyPath)
        {
            var objectResult = GetPropertyValue(src, propertyPath);
            return (T) objectResult;
        }

        /// <summary>
        /// Tests if a type is assignable to another type (inverse of IsAssignableFrom)
        /// </summary>
        /// <param name="type">Type to operate on</param>
        /// <typeparam name="T">Type to check assignment possibility against</typeparam>
        /// <returns>True if objects of type {type} can be assigned to objects of type T</returns>
        public static bool IsAssignableTo<T>(this Type type)
        {
            var targetType = typeof(T);
            return targetType.IsAssignableFrom(type);
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

        /// <summary>
        /// Truncates a decimal value to a required number of places
        /// </summary>
        /// <param name="value">Source decimal value</param>
        /// <param name="places">Number of decimal places required</param>
        /// <returns>A new decimal value which is the original value truncated to the required places</returns>
        public static double TruncateTo(this double value, int places)
        {
            var mul = Math.Pow(10, places);
            return Math.Truncate(value * mul) / mul;
        }

        /// <summary>
        /// Provides a similar api to Javascript's
        /// .toFixed(), except returning a useful decimal!
        /// Note: this is different from .TruncateTo since that will
        /// truncate the value, whereas this will round
        /// </summary>
        /// <param name="value">Source decimal value</param>
        /// <param name="places">Number of decimal places required</param>
        /// <returns></returns>
        public static decimal ToFixed(this decimal value, int places)
        {
            return Math.Round(value, places, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Provides a similar api to Javascript's
        /// .toFixed(), except returning a useful double!
        /// Note: this is different from .TruncateTo since that will
        /// truncate the value, whereas this will round
        /// </summary>
        /// <param name="value">Source double value</param>
        /// <param name="places">Number of double places required</param>
        /// <returns></returns>
        public static double ToFixed(this double value, int places)
        {
            return Math.Round(value, places, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Attempts to convert any object to an IEnumerable&lt;T&gt;
        /// - existing IEnumerables will "just work"
        /// - where possible, types are cast or converted
        ///   - eg an array of strings which are numbers will be converted to ints if required
        /// - also deals with objects which don't implement IEnumerable, but are enumerable
        ///   in a foreach as per C#/.NET compile-time duck-typing, like Regex's MatchCollection
        /// </summary>
        /// <param name="src">Object to operate on</param>
        /// <typeparam name="T">Desired collection element type</typeparam>
        /// <returns></returns>
        public static IEnumerable<T> AsEnumerable<T>(
            this object src)
        {
            if (src == null)
            {
                return new T[0];
            }

            var asEnumerable = src as IEnumerable;
            if (asEnumerable != null)
            {
                return Enumerate<T>(asEnumerable.GetEnumerator());
            }

            var wrapped = new EnumerableWrapper<T>(src);
            if (!wrapped.IsValid)
            {
                return new T[0];
            }

            return wrapped;
        }


        private static IEnumerable<T> Enumerate<T>(IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is T current)
                {
                    yield return current;
                }
                else if (TryChangeType<T>(enumerator.Current, out var converted))
                {
                    yield return converted;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Object of type {enumerator.Current} cannot be converted to {typeof(T)}"
                    );
                }
            }
        }

        /// <summary>
        /// Analogous to TryParse methods, this will attempt to convert a value to
        /// the type T, returning true if it can, and populating the output parameter
        /// </summary>
        /// <param name="input">Value to work on</param>
        /// <param name="output">Output parameter to collect result</param>
        /// <typeparam name="T">Desired type</typeparam>
        /// <returns>True when can ChangeType, false otherwise</returns>
        public static bool TryChangeType<T>(
            this object input,
            out T output)
        {
            if (input is T immediateResult)
            {
                output = immediateResult;
                return true;
            }

            var result = TryChangeType(input, typeof(T), out var outputObj);
            output = (T) outputObj;
            return result;
        }

        /// <summary>
        /// Analogous to TryParse methods, this will attempt to convert a value to
        /// the type requiredType, returning true if it can, and populating the output parameter
        /// </summary>
        /// <param name="input">Value to work on</param>
        /// <param name="requiredType">The required type</param>
        /// <param name="output">Output parameter to collect result</param>
        /// <returns>True when can ChangeType, false otherwise</returns>
        public static bool TryChangeType(
            this object input,
            Type requiredType,
            out object output)
        {
            try
            {
                output = Convert.ChangeType(input, requiredType);
                return true;
            }
            catch
            {
                var method = GenericDefaultOfT.MakeGenericMethod(requiredType);
                output = method.Invoke(null, new object[0]);
                return false;
            }
        }

        private static readonly MethodInfo GenericDefaultOfT = typeof(ObjectExtensions)
            .GetMethod(nameof(Default), BindingFlags.Static | BindingFlags.NonPublic);

        private static T Default<T>()
        {
            return default(T);
        }

        /// <summary>
        /// Tests if the given object is an instance of the type T
        /// - returns false if obj is null
        /// - returns true if T is the exact type of obj
        /// - returns true if T is a base type of obj
        /// - returns true if T is an interface implemented by obj
        /// - returns false otherwise
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsInstanceOf<T>(this object obj)
        {
            var objType = obj?.GetType();
            if (objType is null)
            {
                return false;
            }

            var searchType = typeof(T);

            var search = searchType.IsInterface
                ? objType.GetAllImplementedInterfaces()
                : objType.Ancestry();
            return search.Contains(searchType);
        }

        private static readonly BindingFlags _publicStatic =
            BindingFlags.Public | BindingFlags.Static;

        private static readonly MethodInfo GenericIsInstanceOf
            = typeof(ObjectExtensions)
                .GetMethods(_publicStatic)
                .Single(mi => mi.IsGenericMethod &&
                    mi.Name == nameof(ObjectExtensions.IsInstanceOf));

        /// <summary>
        /// Tests if the given object is an instance of the provided type
        /// - returns false if obj is null
        /// - returns true if the type is the exact type of obj
        /// - returns true if the type is a base type of obj
        /// - returns true if the type is an interface implemented by obj
        /// - returns false otherwise
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IsInstanceOf(
            this object obj,
            Type type
        )
        {
            var method = GenericIsInstanceOf.MakeGenericMethod(
                type
            );
            return (bool)method.Invoke(null, new[] { obj });
        }
    }
}