using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides a mechanism to test deep-equality of two objects with
    /// an optional list of properties to ignore by name. Deep equality
    /// testing bypasses reference-checking of container objects and compares
    /// primitive propertyt values. Use this to test whether or not two
    /// objects essentially contain the same data. More conveniently,
    /// use the following extension methods:
    /// - DeepEquals -> performs default deep equality testing
    /// - DeepSubEquals -> tests if one object matches another, for all the properties that the first has in common with the second
    /// - DeepIntersectionEquals -> tests deep equality only on properties which can be matched by name and type
    /// </summary>
    public class DeepEqualityTester
    {
        private readonly object _objSource;
        private readonly object _objCompare;
        private readonly string[] _ignorePropertiesByName;
        private Dictionary<object, object> _pendingComparisons;

        /// <summary>
        /// Toggle whether or not to record equality errors
        /// </summary>
        public bool RecordErrors { get; set; }
        /// <summary>
        /// Toggle whether or not equality testing fails when properties found
        /// on the first object are not found on the corresponding other object
        /// </summary>
        public bool FailOnMissingProperties { get; set; }
        /// <summary>
        /// Toggle whether or not to only test properties found on both objects
        /// </summary>
        public bool OnlyTestIntersectingProperties { get; set; }
        /// <summary>
        /// Provides a list of errors for diagnosing inequality, if RecordErrors has been
        /// set to true
        /// </summary>
        public IEnumerable<string> Errors => _errors.ToArray();

        private List<string> _errors;

        /// <summary>
        /// Constructs a new DeepEqualityTester for a source object and compare object
        /// with an optional params array of properties to ignore by name, all the way down
        /// </summary>
        /// <param name="objSource">Source / master object</param>
        /// <param name="objCompare">Object to compare with</param>
        /// <param name="ignorePropertiesByName">Params array of properties to ignore by name</param>
        public DeepEqualityTester(object objSource, object objCompare, params string[] ignorePropertiesByName)
        {
            _objSource = objSource;
            _objCompare = objCompare;
            _ignorePropertiesByName = ignorePropertiesByName;
            FailOnMissingProperties = true;
            ClearErrors();
        }

        private void ClearErrors()
        {
            _errors = new List<string>();
        }

        /// <summary>
        /// Calculates if the two objects provided during construction are DeepEqual
        /// according to the properties set. Will always re-calculate, so if one of the
        /// provided objects changes, this will always return the current value.
        /// </summary>
        /// <returns>True if the two objects are found to match; false otherwise.</returns>
        public bool AreDeepEqual()
        {
            ClearPendingOperations();
            var result = AreDeepEqualInternal(_objSource, _objCompare);
            RecordPrimitiveErrorIfRequiredFor(result);
            ClearPendingOperations();
            return result;
        }

        private void RecordPrimitiveErrorIfRequiredFor(bool result)
        {
            if (!result &&
                RecordErrors &&
                IsSimpleTypeOrNullableOfSimpleType(_objSource?.GetType()) &&
                IsSimpleTypeOrNullableOfSimpleType(_objCompare?.GetType()))
                AddError("Primitive values differ");
        }

        private void AddError(string message)
        {
            _errors.Add(message);
        }

        private bool AreDeepEqualInternal(
            object objSource,
            object objCompare
        )
        {
            if (objSource == null && objCompare == null) return true;
            if (objSource == null || objCompare == null) return false;
            var sourceType = objSource.GetType();
            var compareType = objCompare.GetType();
            if (IsSimpleTypeOrNullableOfSimpleType(sourceType) &&
                IsSimpleTypeOrNullableOfSimpleType(compareType))
            {
                return objSource.Equals(objCompare);
            }
            return DeepCompare(
                sourceType,
                objSource,
                compareType,
                objCompare
            );

        }

        private void ClearPendingOperations()
        {
            _pendingComparisons = new Dictionary<object, object>();
        }

        private bool IsSimpleTypeOrNullableOfSimpleType(Type t)
        {
            return t != null &&
                    Types.Primitives.Any(si => si == t ||
                                            (t.IsGenericType &&
                                             t.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                                             Nullable.GetUnderlyingType(t) == si));
        }


        private bool CanPerformSimpleTypeMatchFor(PropertyInfo srcProp)
        {
            return Types.Primitives.Any(st => st == srcProp.PropertyType);
        }


        private PropertyInfo FindMatchingPropertyInfoFor(IEnumerable<PropertyInfo> properties, PropertyInfo srcPropInfo)
        {
            var comparePropInfo = properties.FirstOrDefault(pi => pi.Name == srcPropInfo.Name);
            if (comparePropInfo == null)
            {
                AddError("Unable to find comparison property with name: '" + srcPropInfo.Name + "'");
                return null;
            }
            var compareType = comparePropInfo.PropertyType;
            var srcType = srcPropInfo.PropertyType;
            if (compareType != srcType &&
                !AreBothEnumerable(compareType, srcType))
            {
                AddError($"Source property '{srcPropInfo.Name}' has type '{srcType.Name}' but comparison property has type '{compareType.Name}' and can't find common enumerable ground");
                return null;
            }
            return comparePropInfo;
        }

        private bool AreBothEnumerable(Type t1, Type t2)
        {
            return t1.ImplementsEnumerableGenericType() && t2.ImplementsEnumerableGenericType();
        }

        private bool DeepCompare(
            Type sourceType,
            object objSource,
            Type compareType,
            object objCompare
        )
        {
            if (IsPending(objSource, objCompare))
                return true;    // let other comparisons continue
            var srcProps = sourceType
                .GetProperties()
                .Where(pi => !_ignorePropertiesByName.Contains(pi.Name));
            var comparePropInfos = compareType.GetProperties()
                .Where(pi => !_ignorePropertiesByName.Contains(pi.Name))
                .ToArray();
            var srcPropInfos = OnlyTestIntersectingProperties
                                ? GetIntersectingPropertyInfos(srcProps, comparePropInfos)
                                : srcProps.ToArray();
            if (OnlyTestIntersectingProperties)
            {
                srcPropInfos = srcPropInfos.Where(
                    s => comparePropInfos.Any(c => c.Name == s.Name && c.PropertyType == s.PropertyType)
                ).ToArray();
                if (srcPropInfos.IsEmpty())
                {
                    if (RecordErrors)
                        AddError("No intersecting properties found");
                }
                if (srcPropInfos.IsEmpty())
                    return false;
            }
            if (!FailOnMissingProperties || OnlyTestIntersectingProperties || srcPropInfos.Length == comparePropInfos.Length)
                return CompareWith(objSource, objCompare, srcPropInfos, comparePropInfos);
            if (RecordErrors)
                AddError("Property count mismatch");
            return false;
        }

        private PropertyInfo[] GetIntersectingPropertyInfos(
            IEnumerable<PropertyInfo> left,
            IEnumerable<PropertyInfo> right
        )
        {
            var result = left.Where(
                    s => right.Any(c => c.Name == s.Name && c.PropertyType == s.PropertyType)
                ).ToArray();
            if (result.IsEmpty() && RecordErrors)
                AddError("No intersecting properties found");
            return result;
        }

        private bool CompareWith(object objSource, object objCompare, PropertyInfo[] srcPropInfos, PropertyInfo[] comparePropInfos)
        {
            var didAnyComparison = false;
            var finalResult = srcPropInfos.Aggregate(true, (result, srcProp) =>
            {
                if (!result) return false;
                var compareProp = FindMatchingPropertyInfoFor(comparePropInfos, srcProp);
                didAnyComparison = didAnyComparison || compareProp != null;
                return compareProp != null &&
                       PropertyValuesMatchFor(objSource, objCompare, srcProp, compareProp);
            });
            return (srcPropInfos.IsEmpty() || didAnyComparison) && finalResult;
        }

        private bool IsPending(object objSource, object objCompare)
        {
            object gotValue;
            if (_pendingComparisons.TryGetValue(objSource, out gotValue))
                return ReferenceEquals(gotValue, objCompare);
            if (_pendingComparisons.TryGetValue(objCompare, out gotValue))
                return ReferenceEquals(gotValue, objSource);
            _pendingComparisons.Add(objSource, objCompare);
            return false;
        }

        private bool PropertyValuesMatchFor(object objSource, object objCompare, PropertyInfo srcProp, PropertyInfo compareProp)
        {
            var srcValue = srcProp.GetValue(objSource);
            var compareValue = compareProp.GetValue(objCompare, null);
            var result = CanPerformSimpleTypeMatchFor(srcProp)
                ? AreDeepEqualInternal(srcValue, compareValue)
                : MatchPropertiesOrCollection(srcValue, compareValue);
            if (!result && RecordErrors)
            {
                AddError($"Property value mismatch for {srcProp.Name}: {Stringify(objSource)} vs {Stringify(objCompare)}");
            }
            return result;
        }

        private string Stringify(object obj)
        {
            if (obj == null) return "(null)";
            try
            {
                var props = obj.GetType().GetProperties();
                return string.Join(
                    "\n  ",
                    "{",
                    "  " + props.Aggregate(new List<string>(), (acc, cur) =>
                    {
                        var propValue = cur.GetValue(obj);
                        acc.Add(string.Join("", cur.Name, ": ",  propValue?.ToString() ?? "(null)"));
                        return acc;
                    }).JoinWith("\n    "),
                    "}");
            }
            catch
            {
                return obj.ToString();
            }
        }

        private bool MatchPropertiesOrCollection(object srcValue, object compareValue)
        {
            var srcEnumerableInterface = TryGetEnumerableInterfaceFor(srcValue);
            var compareEnumerableInterface = TryGetEnumerableInterfaceFor(compareValue);
            if (srcEnumerableInterface == null && compareEnumerableInterface == null)
                return AreDeepEqualInternal(srcValue, compareValue);
            if (srcEnumerableInterface == null || compareEnumerableInterface == null)
                return false;
            return CollectionsMatch(
                srcValue,
                srcEnumerableInterface,
                compareValue,
                compareEnumerableInterface
            );
        }

        private bool CollectionsMatch(
            object srcValue,
            Type srcEnumerableInterface,
            object compareValue,
            Type compareEnumerableInterface
        )
        {
            var t1 = srcEnumerableInterface.GenericTypeArguments[0];
            var t2 = compareEnumerableInterface.GenericTypeArguments[0];
            var genericMethod = GetType()
                                .GetMethod("TestCollectionsMatch", BindingFlags.Instance | BindingFlags.NonPublic);
            var typedMethod = genericMethod.MakeGenericMethod(t1, t2);
            return (bool)typedMethod.Invoke(this, new[] { srcValue, compareValue });
        }


#pragma warning disable S1144 // Unused private types or members should be removed
        // ReSharper disable once UnusedMember.Local
        private bool TestCollectionsMatch<T1, T2>(
            IEnumerable<T1> collection1,
            IEnumerable<T2> collection2
        )
        {
            var enumerable = collection1 as T1[] ?? collection1.ToArray();
            var second = collection2 as T2[] ?? collection2.ToArray();
            if (enumerable.Length != second.Length)
                return false;
            var collection1ContainsCollection2 = AllMembersOfFirstCollectionAreFoundInSecond(enumerable, second);
            var collection2ContainsCollection1 = AllMembersOfFirstCollectionAreFoundInSecond(second, enumerable);
            return collection1ContainsCollection2 && collection2ContainsCollection1;
        }

        private bool AllMembersOfFirstCollectionAreFoundInSecond<T1, T2>(IEnumerable<T1> first, IEnumerable<T2> second)
        {
            return first.Aggregate(true, (acc, cur) => acc && ContainsOneLike(second, cur));
        }

        private bool ContainsOneLike<T1, T2>(IEnumerable<T2> collection, T1 seek)
        {
            return collection.Any(i => AreDeepEqualInternal(i, seek));
        }
#pragma warning restore S1144 // Unused private types or members should be removed

        private static Type TryGetEnumerableInterfaceFor(object srcValue)
        {
            return srcValue
                ?.GetType()
                .TryGetEnumerableInterface();
        }
    }
}