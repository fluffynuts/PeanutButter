using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.Utils.Dictionaries;
#else
using PeanutButter.Utils.Dictionaries;
#endif

// ReSharper disable MemberCanBePrivate.Global
#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils;
#else
namespace PeanutButter.Utils;
#endif

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
    /// hierarchy matches that of another
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
            ignorePropertiesByName
        );
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
    /// hierarchy matches that of another
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
        )
        {
            IncludeFields = comparison == ObjectComparisons.PropertiesAndFields
        };
        return tester.AreDeepEqual();
    }

    /// <summary>
    /// Runs a deep equality test between two objects,
    /// ignoring reference differences wherever possible
    /// and logging failures with the provided action. Properties
    /// can be excluded by name with the ignorePropertiesByName params
    /// </summary>
    /// <param name="objSource"></param>
    /// <param name="objCompare"></param>
    /// <param name="failureLogAction"></param>
    /// <param name="ignorePropertiesByName"></param>
    /// <returns></returns>
    public static bool DeepEquals(
        this object objSource,
        object objCompare,
        Action<string> failureLogAction,
        params string[] ignorePropertiesByName
    )
    {
        var tester = new DeepEqualityTester(
            objSource,
            objCompare,
            ignorePropertiesByName
        )
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
        params string[] ignorePropertiesByName
    )
    {
        var tester = new DeepEqualityTester(
            objSource,
            objCompare,
            ignorePropertiesByName
        )
        {
            FailOnMissingProperties = false
        };
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
        params string[] ignorePropertiesByName
    )
    {
        var tester = new DeepEqualityTester(
            objSource,
            objCompare,
            ignorePropertiesByName
        )
        {
            OnlyTestIntersectingProperties = true
        };
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
        params string[] ignoreProperties
    )
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
        params string[] ignoreProperties
    )
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
        params string[] ignoreProperties
    )
    {
        return collection.ContainsOnlyOneMatching(
            item,
            (t1, t2) => t1.DeepEquals(t2, ignoreProperties)
        );
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
            (t1, t2) => t1.DeepIntersectionEquals(t2, ignoreProperties)
        );
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
                }
            ) ==
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
        if (src is null || dst is null)
        {
            return;
        }

        var srcType = src.GetType();
        if (!PropertyCache.TryGetValue(srcType, out var props))
        {
            props = srcType.GetProperties();
            PropertyCache.TryAdd(srcType, props);
        }

        var srcPropInfos = props
            .Where(pi => !ignoreProperties.Contains(pi.Name));
        var dstPropInfos = dst.GetType().GetProperties()
            .Where(p => p.CanWrite)
            .ToArray();

        var targetPropertyCache = dstPropInfos.ToDictionary(
            pi => Tuple.Create(pi.Name, pi.PropertyType),
            pi => pi
        );

        foreach (var srcPropInfo in srcPropInfos.Where(
                     pi => pi.CanRead &&
                         pi.GetIndexParameters().Length == 0
                 ))
        {
            if (!targetPropertyCache.TryGetValue(
                    Tuple.Create(
                        srcPropInfo.Name,
                        srcPropInfo.PropertyType
                    ),
                    out var matchingTarget
                ))
            {
                continue;
            }

            var srcVal = srcPropInfo.GetValue(src);

            PropertySetterStrategies.Aggregate(
                false,
                (acc, cur) =>
                    acc || cur(deep, srcPropInfo, matchingTarget, dst, srcVal)
            );
        }
    }

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    private static readonly Func<bool, PropertyInfo, PropertyInfo, object, object, bool>[]
        PropertySetterStrategies =
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
        object srcValue
    )
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
        var method = GenericMakeListCopy.MakeGenericMethod(itemType);
        var newValue = method.Invoke(
            null,
            new[]
            {
                srcVal
            }
        );
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
        var specific = GenericMakeArrayCopy.MakeGenericMethod(underlyingType);
        // ReSharper disable once RedundantExplicitArrayCreation
        var newValue = specific.Invoke(
            null,
            new[]
            {
                srcVal
            }
        );
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

    private const BindingFlags PRIVATE_STATIC =
        BindingFlags.NonPublic | BindingFlags.Static;

    private static readonly MethodInfo GenericMakeArrayCopy
        = typeof(ObjectExtensions).GetMethod(
            nameof(MakeArrayCopyOf),
            PRIVATE_STATIC
        );

    private static readonly MethodInfo GenericMakeListCopy
        = typeof(ObjectExtensions).GetMethod(
            nameof(MakeListCopyOf),
            PRIVATE_STATIC
        );

    private static readonly MethodInfo GenericMakeDictionaryCopy
        = typeof(ObjectExtensions).GetMethod(
            nameof(MakeDictionaryCopyOf),
            PRIVATE_STATIC
        );

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
        return item is null || item.Equals(default(T))
            ? default(T)
            : (T)item.DeepCloneInternal(item.GetType());
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

            return CloneStrategies.Aggregate(
                null as object,
                (acc, cur) => acc ?? cur(src, cloneType)
            );
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Unable to clone item of type {cloneType.Name}: {e.Message}");
            return cloneType.DefaultValue();
        }
    }

    private static readonly Func<object, Type, object>[] CloneStrategies =
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
                out var valueType
            ))
        {
            return null;
        }

        var key = Tuple.Create(keyType, valueType);
        if (!DictionaryCopyMethodCache.TryGetValue(key, out var method))
        {
            method = GenericMakeDictionaryCopy.MakeGenericMethod(keyType, valueType);
            DictionaryCopyMethodCache.TryAdd(key, method);
        }

        return method.Invoke(
            null,
            new[]
            {
                src,
                cloneType
            }
        );
    }

    private static readonly ConcurrentDictionary<Tuple<Type, Type>, MethodInfo> DictionaryCopyMethodCache = new();

    private static object CloneEnumerable(object src, Type cloneType)
    {
        if (!cloneType.ImplementsEnumerableGenericType())
            return null;
        var itemType = cloneType.GetCollectionItemType();
        var method = FindGenericMethodFor(
            itemType,
            GenericMakeArrayCopy,
            ArrayCopyMethodCache
        );
        return method.Invoke(
            null,
            new[]
            {
                src
            }
        );
    }

    private static object CloneList(object src, Type cloneType)
    {
        if (!cloneType.IsGenericOf(typeof(List<>)) && !cloneType.IsGenericOf(typeof(IList<>)))
            return null;
        var itemType = cloneType.GetCollectionItemType();
        var method = FindGenericMethodFor(
            itemType,
            GenericMakeListCopy,
            ListCopyMethodCache
        );
        return method.Invoke(
            null,
            new[]
            {
                src
            }
        );
    }

    private static object CloneArray(object src, Type cloneType)
    {
        if (!cloneType.IsArray)
            return null;
        var itemType = cloneType.GetCollectionItemType();
        var method = FindGenericMethodFor(
            itemType,
            GenericMakeArrayCopy,
            ArrayCopyMethodCache
        );
        return method.Invoke(
            null,
            new[]
            {
                src
            }
        );
    }

    private static readonly ConcurrentDictionary<Type, MethodInfo> ListCopyMethodCache = new();
    private static readonly ConcurrentDictionary<Type, MethodInfo> ArrayCopyMethodCache = new();

    private static MethodInfo FindGenericMethodFor(
        Type type,
        MethodInfo genericMethod,
        ConcurrentDictionary<Type, MethodInfo> cache
    )
    {
        if (cache.TryGetValue(type, out var cached))
        {
            return cached;
        }

        var result = genericMethod.MakeGenericMethod(type);
        cache.TryAdd(type, result);
        return result;
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
                    Nullable.GetUnderlyingType(t) == si)
        );
    }

    /// <summary>
    /// Gets the value of a property on an object, specified by the property path, of the given Type
    /// </summary>
    /// <param name="src">Object to search for the required property</param>
    /// <param name="propertyPath">Path to the property: may be a property name or a dotted path down an object hierarchy, eg: Company.Name</param>
    /// <typeparam name="T">Expected type of the property value</typeparam>
    /// <returns></returns>
    public static T Get<T>(
        this object src,
        string propertyPath
    )
    {
        var type = src.GetType();
        return ResolvePropertyValueFor<T>(src, propertyPath, type, out _);
    }

    /// <summary>
    /// Provides a non-generic interface to Get&lt;T&gt;
    /// </summary>
    /// <param name="src"></param>
    /// <param name="propertyPath"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object Get(
        this object src,
        string propertyPath,
        Type type
    )
    {
        var result = ResolvePropertyValueFor<object>(src, propertyPath, type, out var typeWasConverted);
        return typeWasConverted || result?.GetType() == type
            ? result
            // result could be assignable, but the requester has specifically
            // asked for a type
            : Convert.ChangeType(result, type);
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
        string propertyPath
    )
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
        T defaultValue
    )
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
    [Obsolete(
        "This method was poorly-named and will be removed at some point. Please rather use .InArray() to avoid confusion with the IEnumerable<T> extension AsArray"
    )]
    public static T[] AsArray<T>(this T input)
    {
        return new[]
        {
            input
        };
    }

    /// <summary>
    /// Fluency extension to wrap a single item in an array, eg:
    /// new SomeBusinessObject().InArray().Union(SomeOtherCollection);
    /// </summary>
    /// <param name="input">The item to wrap</param>
    /// <typeparam name="T">The type of the object</typeparam>
    /// <returns>A single-element array containing the input object</returns>
    public static T[] InArray<T>(this T input)
    {
        return new[]
        {
            input
        };
    }

    /// <summary>
    /// Fluency extension to wrap a single item in a list, eg:
    /// new SomeBusinessObject().InList().Union(SomeOtherCollection);
    /// </summary>
    /// <param name="input">The item to wrap</param>
    /// <typeparam name="T">The type of the object</typeparam>
    /// <returns>A single-element list containing the input object</returns>
    public static List<T> InList<T>(this T input)
    {
        return new List<T>()
        {
            input
        };
    }

    private static T ResolvePropertyValueFor<T>(
        object src,
        string propertyPath,
        Type type,
        out bool typeWasConverted
    )
    {
        var valueAsObject = GetPropertyValue(src, propertyPath);
        return RetrieveTypedValue<T>(type, valueAsObject, propertyPath, out typeWasConverted);
    }

    private static T RetrieveTypedValue<T>(
        Type type,
        object valueAsObject,
        string propertyPath,
        out bool typeWasConverted
    )
    {
        if (valueAsObject is null)
        {
            if (type.IsNullableType())
            {
                typeWasConverted = false;
                return default;
            }

            throw new InvalidOperationException(
                $"value for {propertyPath} is null but {typeof(T)} is not nullable"
            );
        }

        var valueType = valueAsObject.GetType();
        if (!valueType.IsAssignableTo<T>())
        {
            if (type == typeof(string))
            {
                typeWasConverted = true;
                return (T)(object)valueAsObject.ToString();
            }

            try
            {
                typeWasConverted = true;
                return (T)Convert.ChangeType(valueAsObject, typeof(T));
            }
            catch
            {
                throw new ArgumentException(
                    "Get<> must be invoked with a type to which the property value could be assigned (" +
                    type.Name +
                    "." +
                    propertyPath +
                    " has type '" +
                    valueType.Name +
                    "', but expected '" +
                    typeof(T).Name +
                    "' or derivative",
                    nameof(T)
                );
            }
        }

        typeWasConverted = false;
        return (T)valueAsObject;
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
        string propertyPath
    )
    {
        if (src is null)
        {
            throw new InvalidOperationException(
                "Cannot retrieve any property value off of null"
            );
        }

        var trailingMember = FindPropertyOrField(
            src,
            propertyPath,
            AnyInstanceProperty,
            AnyInstanceField
        );
        if (!trailingMember.Found)
        {
            throw new MemberNotFoundException(src.GetType(), propertyPath);
        }

        return trailingMember.GetValue();
    }

    private static PropertyOrField[] AnyInstanceProperty(
        Type type
    )
    {
        var ancestry = CreateAncestryLookupFor(type);
        return type.GetProperties(AllOnInstance)
            .Where(pi => !(pi.DeclaringType is null))
            .Select(
                pi => new
                {
                    pi,
                    idx = ancestry[pi.DeclaringType]
                }
            )
            .OrderBy(o => o.pi.Name)
            .ThenBy(o => o.idx)
            .Select(o => o.pi)
            .ImplicitCast<PropertyOrField>()
            .ToArray();
    }

    private static PropertyOrField[] AnyInstanceField(
        Type type
    )
    {
        var ancestry = CreateAncestryLookupFor(type);
        var fields = new List<PropertyOrField>();
        foreach (var implementation in ancestry.OrderBy(o => o.Value))
        {
            fields.AddRange(
                implementation.Key.GetFields(AllOnInstance)
                    .ImplicitCast<PropertyOrField>()
            );
        }

        return fields.ToArray();
    }

    private static Dictionary<Type, int> CreateAncestryLookupFor(
        Type type
    )
    {
        return type.Ancestry()
            .Reverse()
            .Select(
                (t, idx) => new
                {
                    type = t,
                    idx
                }
            )
            .ToDictionary(o => o.type, o => o.idx);
    }

    /// <summary>
    /// Invokes a method on an object, if available; otherwise explodes
    /// </summary>
    /// <param name="src">Object to invoke the method on</param>
    /// <param name="methodName">Method to invoke, by name</param>
    /// <param name="args">Any parameters to give to the method</param>
    /// <returns>return value of the method</returns>
    public static object InvokeMethodWithResult(
        this object src,
        string methodName,
        params object[] args
    )
    {
        var srcType = src.GetType();
        var method = srcType.GetMethod(
            methodName,
            INSTANCE_PUBLIC_OR_PRIVATE
        );
        return method is null
            ? throw new InvalidOperationException($"Can't find method {methodName} on {srcType.Name}")
            : method.Invoke(src, args);
    }

    private const BindingFlags INSTANCE_PUBLIC_OR_PRIVATE =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

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
        object newValue
    )
    {
        src.SetPropertyOrFieldValue(
            propertyPath,
            newValue
        );
    }

    private static void SetPropertyOrFieldValue(
        this object src,
        string propertyPath,
        object newValue
    )
    {
        var trailingMember = FindPropertyOrField(
            src,
            propertyPath,
            AnyInstanceProperty,
            AnyInstanceField
        );

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
        public object Index { get; set; }
        public bool IsIndexed { get; set; }

        public object GetValue()
        {
            return IsIndexed
                ? Member.GetValueAt(Host, Index)
                : Member.GetValue(Host);
        }

        public void SetValue(object value)
        {
            if (IsIndexed)
            {
                throw new NotImplementedException();
            }
            else
            {
                Member.SetValue(Host, value);
            }
        }
    }

    private static TrailingMember FindPropertyOrField(
        object src,
        string propertyPath,
        params Func<Type, PropertyOrField[]>[] fetchers
    )
    {
        return FindPropertyOrField(
            src,
            propertyPath,
            throwIfNotFound: true,
            fetchers
        );
    }

    private static TrailingMember FindPropertyOrField(
        object src,
        string propertyPath,
        bool throwIfNotFound,
        params Func<Type, PropertyOrField[]>[] fetchers
    )
    {
        if (src is null)
        {
            return Fail(null, propertyPath);
        }

        var queue = new Queue<string>(propertyPath.Split('.'));
        var result = new TrailingMember();
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var isIndexed = ParseIndexes(current, out var name, out var indexes);
            if (isIndexed && indexes.Length > 1)
            {
                throw new NotSupportedException("Multi-dimensional indexing is not supported");
            }

            var index = indexes?.FirstOrDefault();

            var type = src.IsRuntimeType()
                ? src as Type
                : src.GetType();
            var memberInfo = fetchers.Aggregate(
                null as PropertyOrField,
                (acc, cur) => acc ?? cur(type).FirstOrDefault(mi => mi.Name == name)
            );
            if (memberInfo == null)
            {
                return Fail(type, name);
            }

            if (queue.Count == 0)
            {
                result.Member = memberInfo;
                result.Host = src;
                result.IsIndexed = isIndexed;
                result.Index = index;
                break;
            }

            src = isIndexed
                ? memberInfo.GetValueAt(src, index)
                : memberInfo.GetValue(src);
        }

        return result;

        TrailingMember Fail(Type type, string name)
        {
            return throwIfNotFound
                ? throw new MemberNotFoundException(type, name)
                : new TrailingMember();
        }
    }

    private static bool ParseIndexes(
        string path,
        out string name,
        out object[] indexes
    )
    {
        var parts = path.Split('[', ']').Where(p => p.Length > 0)
            .ToArray();
        name = parts[0];
        if (parts.Length == 1)
        {
            indexes = default;
            return false;
        }

        indexes = parts.Skip(1).Select(
            s => int.TryParse(s, out var asInt)
                ? asInt
                : s as object
        ).ToArray();
        return true;
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
        T newValue
    )
    {
        src.SetPropertyValue(propertyPath, newValue);
    }

    /// <summary>
    /// attempts to set a property value on an object by path and value
    /// - will not throw on error, but will return false instead
    /// </summary>
    /// <param name="src"></param>
    /// <param name="propertyPath"></param>
    /// <param name="newValue"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool TrySet<T>(
        this object src,
        string propertyPath,
        T newValue
    )
    {
        try
        {
            src.Set(propertyPath, newValue);
            return true;
        }
        catch
        {
            return false;
        }
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
        this object src
    )
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
        out T output
    )
    {
        if (input is T immediateResult)
        {
            output = immediateResult;
            return true;
        }

        var result = TryChangeType(input, typeof(T), out var outputObj);
        output = (T)outputObj;
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
        out object output
    )
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

    private const BindingFlags PUBLIC_STATIC = BindingFlags.Public | BindingFlags.Static;

    private static readonly MethodInfo GenericIsInstanceOf
        = typeof(ObjectExtensions)
            .GetMethods(PUBLIC_STATIC)
            .Single(
                mi => mi.IsGenericMethod &&
                    mi.Name == nameof(ObjectExtensions.IsInstanceOf)
            );

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
    /// <returns></returns>
    public static bool IsInstanceOf(
        this object obj,
        Type type
    )
    {
        var method = GenericIsInstanceOf.MakeGenericMethod(
            type
        );
        return (bool)method.Invoke(
            null,
            new[]
            {
                obj
            }
        );
    }

    /// <summary>
    /// Determines whether or not an arbitrary object is a RuntimeType
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsRuntimeType(this object obj)
    {
        var objType = obj?.GetType();
        return objType is not null &&
            objType.Name == "RuntimeType" &&
            objType.Namespace == "System";
    }

    /// <summary>
    /// Tests if the object has the requested property by path
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="prop"></param>
    /// <returns></returns>
    public static bool HasProperty(
        this object obj,
        string prop
    )
    {
        var trailingMember = FindPropertyOrFieldForHasProperty(
            obj,
            prop
        );
        return trailingMember.Found;
    }

    /// <summary>
    /// Tests whether the object has the named property with the type T
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="prop"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool HasProperty<T>(
        this object obj,
        string prop
    )
    {
        return obj.HasProperty(prop, typeof(T));
    }

    /// <summary>
    /// Tests if the object has the named property with the expected type
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="prop"></param>
    /// <param name="propertyType"></param>
    /// <returns></returns>
    public static bool HasProperty(
        this object obj,
        string prop,
        Type propertyType
    )
    {
        var member = FindPropertyOrFieldForHasProperty(
            obj,
            prop
        );
        return member.Found && member.Member.Type == propertyType;
    }

    private static TrailingMember FindPropertyOrFieldForHasProperty(
        object obj,
        string prop
    )
    {
        return FindPropertyOrField(
            obj,
            prop,
            throwIfNotFound: false,
            AnyInstanceProperty
        );
    }

    /// <summary>
    /// Attempt to retrieve the named property with the given type
    /// off of the provided object
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="propertyPath"></param>
    /// <param name="result"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool TryGet<T>(
        this object obj,
        string propertyPath,
        out T result
    )
    {
        var member = FindPropertyOrField(
            obj,
            propertyPath,
            throwIfNotFound: false,
            AnyInstanceProperty,
            AnyInstanceField
        );
        var outputType = typeof(T);
        var memberType = member.Member?.Type;
        var canRetrieveValue =
            member.Found &&
            (
                memberType == outputType ||
                outputType.IsAssignableFrom(memberType) ||
                (memberType.IsNumericType() && outputType.IsNumericType())
            );
        if (!canRetrieveValue)
        {
            result = default;
            return false;
        }

        var objectResult = member.Member!.GetValue(obj);
        try
        {
            result = RetrieveTypedValue<T>(
                typeof(T),
                objectResult,
                propertyPath,
                out _
            );
            return true;
        }
        catch
        {
            result = default;
            return false;
        }
    }

    /// <summary>
    /// Tests if two objects are the same reference in memory
    /// (ie, short-hand for object.ReferenceEquals(original, test);
    /// </summary>
    /// <param name="original"></param>
    /// <param name="test"></param>
    /// <returns></returns>
    public static bool Is(
        this object original,
        object test
    )
    {
        return ReferenceEquals(original, test);
    }

    /// <summary>
    /// Attempts to provide a dictionary representation for the provided
    /// object. If the provided object already implements IDictionary&lt;TKey, TValue&gt;
    /// then you'll get the same instance back - be careful to clone it if you don't
    /// want to mutate the original
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static IDictionary<TKey, TValue> AsDictionary<TKey, TValue>(
        this object obj
    )
    {
        return obj switch
        {
            null => new Dictionary<TKey, TValue>(),
            IDictionary<TKey, TValue> typed => typed,
            IDictionary dict => dict.ToDictionary<TKey, TValue>(),
            _ => TryMakeDictionaryOutOf<TKey, TValue>(obj)
        };
    }

    private static IDictionary<TKey, TValue> TryMakeDictionaryOutOf<TKey, TValue>(
        object obj
    )
    {
        var type = obj.GetType();
        if (type.IsPrimitiveOrImmutable())
        {
            throw new NotSupportedException(
                $"Cannot convert object of type {type} to a dictionary"
            );
        }

        if (typeof(TKey) == typeof(string) &&
            typeof(TValue) == typeof(object))
        {
            return new DictionaryWrappingObject(obj)
                as IDictionary<TKey, TValue>;
        }

        throw new NotSupportedException(
            "Arbitrary objects may only be represented by Dictionary<string, object>"
        );
    }

    /// <summary>
    /// Produces a string-string dictionary from any value,
    /// given that value and two funcs:
    /// - one to retrieve all keys on the object
    /// - one to resolve a key to a value for the object
    /// </summary>
    /// <param name="value"></param>
    /// <param name="keyFetcher"></param>
    /// <param name="valueFetcher"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IDictionary<string, string> AsDictionary<T>(
        this T value,
        Func<T, IEnumerable<string>> keyFetcher,
        Func<T, string, string> valueFetcher
    )
    {
        return keyFetcher(value).Select(
            k => new KeyValuePair<string, string>(k, valueFetcher(value, k))
        ).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

}