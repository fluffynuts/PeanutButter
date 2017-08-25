using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static PeanutButter.Utils.PyLike;

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

        /// <summary>
        /// Flag: include fields in deep equality testing (false by default)
        /// </summary>
        public bool IncludeFields { get; set; }

        /// <summary>
        /// Toggle only testing the shape of the objects provided.
        /// </summary>
        public bool OnlyCompareShape { get; set; }

        private List<string> _errors;

        /// <summary>
        /// Constructs a new DeepEqualityTester for a source object and compare object
        /// with an optional params array of properties to ignore by name, all the way down
        /// </summary>
        /// <param name="objSource">Source / master object</param>
        /// <param name="objCompare">Object to compare with</param>
        /// <param name="ignorePropertiesByName">Params array of properties to ignore by name</param>
        public DeepEqualityTester(
            object objSource,
            object objCompare,
            params string[] ignorePropertiesByName
        )
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

        internal bool AreDeepEqual(Dictionary<object, object> pendingComparisons)
        {
            _pendingComparisons = pendingComparisons;
            var result = AreDeepEqualInternal(_objSource, _objCompare);
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
            if (!RecordErrors)
                return;
            _errors.Add(message);
        }

        private static bool AreBothSimpleOrNullableOfSimpleTypes(Type t1, Type t2)
        {
            return IsSimpleTypeOrNullableOfSimpleType(t1) &&
                   IsSimpleTypeOrNullableOfSimpleType(t2);
        }

        private bool AreDeepEqualInternal(
            object objSource,
            object objCompare
        )
        {
            if (objSource == null && objCompare == null)
                return true;
            if (objSource == null || objCompare == null)
                return false;

            var sourceType = objSource.GetType();
            var compareType = objCompare.GetType();
            if (AreBothSimpleOrNullableOfSimpleTypes(sourceType, compareType))
            {
                return AreSimpleEqual(sourceType, objSource, compareType, objCompare);
            }
            // TODO: see if this can be done a bit better
            if (AreBothEnumerable(sourceType, compareType) &&
                BothHaveGenericTypeParameters(sourceType, compareType))
            {
                return DeepCollectionCompare(
                    sourceType,
                    objSource,
                    compareType,
                    objCompare
                );
            }
            return DeepCompare(
                sourceType,
                objSource,
                compareType,
                objCompare
            );
        }

        private bool AreSimpleEqual(Type sourceType, object objSource, Type compareType, object objCompare)
        {
            // naive simple equality tester:
            //  if the types match, use .Equals, otherwise attempt upcasting to decimal
            //  so that, eg: (long)2 == (int)2
            if (sourceType == compareType)
                return OnlyCompareShape || objSource.Equals(objCompare);
            var sourceAsDecimal = TryConvertToDecimal(objSource);
            var compareAsDecimal = TryConvertToDecimal(objCompare);
            if (sourceAsDecimal == null || compareAsDecimal == null)
                return false;
            return OnlyCompareShape || sourceAsDecimal.Equals(compareAsDecimal);
        }

        private decimal? TryConvertToDecimal(object obj)
        {
            try
            {
                return Convert.ToDecimal(obj);
            }
            catch
            {
                return null;
            }
        }


        private bool BothHaveGenericTypeParameters(Type sourceType, Type compareType)
        {
            return
                sourceType.GenericTypeArguments.Length > 0 &&
                compareType.GenericTypeArguments.Length > 0;
        }

        private bool DeepCollectionCompare(
            Type sourceType, object objSource,
            Type compareType, object objCompare)
        {
            var sourceItemType = GetItemTypeFor(sourceType);
            var compareItemType = GetItemTypeFor(compareType);
            var method = DeepCollectionCompareGenericMethod.MakeGenericMethod(
                sourceItemType, compareItemType
            );
            return (bool) method.Invoke(this, new[] {objSource, objCompare});
        }

        private static Type GetItemTypeFor(Type collectionType)
        {
            return collectionType.GenericTypeArguments[0];
        }

        private static readonly MethodInfo DeepCollectionCompareGenericMethod =
            typeof(DeepEqualityTester).GetMethod(
                nameof(DeepCollectionCompareGeneric),
                BindingFlags.NonPublic | BindingFlags.Instance
            );

        // ReSharper disable once UnusedMember.Local
        private bool DeepCollectionCompareGeneric<T1, T2>(
            IEnumerable<T1> source,
            IEnumerable<T2> compare
        )
        {
            var sourceCount = source.Count();
            var compareCount = compare.Count();
            if (sourceCount != compareCount)
            {
                AddError($"Collection sizes do not match: {sourceCount} vs {compareCount}");
                return false;
            }
            var index = 0;
            return Zip(source, compare)
                .Aggregate(
                    true,
                    (acc, cur) =>
                    {
                        return acc &&
                               DeepCompareAtIndex(
                                   index++,
                                   cur.Item1,
                                   cur.Item2
                               );
                    });
        }

        private bool DeepCompareAtIndex(
            int index,
            object source,
            object target
        )
        {
            var result = AreDeepEqualInternal(
                source,
                target
            );
            if (!result)
            {
                AddError($"Collection comparison fails at index {index}");
            }
            return result;
        }

        private void ClearPendingOperations()
        {
            _pendingComparisons = new Dictionary<object, object>();
        }

        private static bool IsSimpleTypeOrNullableOfSimpleType(Type t)
        {
            return t != null &&
                   Types.PrimitivesAndImmutables.Any(si => si == t ||
                                                           (t.IsGenericType &&
                                                            t.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                                                            Nullable.GetUnderlyingType(t) == si));
        }


        private bool CanPerformSimpleTypeMatchFor(PropertyOrField srcProp)
        {
            return Types.PrimitivesAndImmutables.Any(st => st == srcProp.Type);
        }


        private PropertyOrField FindMatchingPropertyInfoFor(
            IEnumerable<PropertyOrField> properties,
            PropertyOrField srcPropInfo)
        {
            var comparePropInfo = properties.FirstOrDefault(pi => pi.Name == srcPropInfo.Name);
            if (comparePropInfo == null)
            {
                AddError("Unable to find comparison property with name: '" + srcPropInfo.Name + "'");
                return null;
            }
            var compareType = comparePropInfo.Type;
            var srcType = srcPropInfo.Type;
            if (compareType != srcType &&
                !AreBothEnumerable(compareType, srcType))
            {
                AddError(
                    $"Source property '{srcPropInfo.Name}' has type '{srcType.Name}' but comparison property has type '{compareType.Name}' and can't find common enumerable ground"
                );
                return null;
            }
            return comparePropInfo;
        }

        private bool AreBothEnumerable(Type t1, Type t2)
        {
            // TODO: should we examine the duck-typed enumerable interface (ie, GetEnumerator())?
            return t1.ImplementsEnumerableGenericType() &&
                   t2.ImplementsEnumerableGenericType();
        }

        private bool DeepCompare(
            Type sourceType,
            object objSource,
            Type compareType,
            object objCompare
        )
        {
            if (IsPending(objSource, objCompare))
                return true; // let other comparisons continue
            var srcProps = GetPropertiesAndFieldsOf(sourceType);
            var compareProps = GetPropertiesAndFieldsOf(compareType);

            var srcPropInfos = OnlyTestIntersectingProperties
                ? GetIntersectingPropertyInfos(srcProps, compareProps)
                : srcProps.ToArray();
            if (OnlyTestIntersectingProperties)
            {
                srcPropInfos = srcPropInfos.Where(
                        s => compareProps.Any(c => c.Name == s.Name && c.Type == s.Type)
                    )
                    .ToArray();
                if (srcPropInfos.IsEmpty())
                {
                    AddError("No intersecting properties found");
                    return false;
                }
            }
            if (!FailOnMissingProperties ||
                OnlyTestIntersectingProperties ||
                srcPropInfos.Length == compareProps.Length)
                return CompareWith(objSource, objCompare, srcPropInfos, compareProps);
            AddError(string.Join("\n",
                "Property count mismatch",
                $"Source has {srcPropInfos.Count()} properties:",
                $"{DumpPropertyInfo(srcPropInfos)}",
                $"\nComparison has {compareProps.Count()} properties:",
                $"{DumpPropertyInfo(compareProps)}"
            ));
            return false;
        }

        private PropertyOrField[] GetPropertiesAndFieldsOf(Type sourceType)
        {
            var props = sourceType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Encapsulate()
                .Where(pi => !_ignorePropertiesByName.Contains(pi.Name))
                .ToArray();
            if (IncludeFields)
            {
                var fields = sourceType
                    .GetFields(BindingFlags.Public | BindingFlags.Instance)
                    .Encapsulate()
                    .Where(o => !_ignorePropertiesByName.Contains(o.Name));
                props = props.And(fields.ToArray());
            }
            return props;
        }

        private const string DUMP_DELIMITER = "\n* ";

        private string DumpPropertyInfo(PropertyOrField[] propInfos)
        {
            return DUMP_DELIMITER +
                   string.Join(DUMP_DELIMITER,
                       propInfos.Select(pi => $"{pi.Type} {pi.Name}")
                   );
        }

        private PropertyOrField[] GetIntersectingPropertyInfos(
            IEnumerable<PropertyOrField> left,
            IEnumerable<PropertyOrField> right
        )
        {
            var result = left.Where(
                    s => right.Any(c => c.Name == s.Name && c.Type == s.Type)
                )
                .ToArray();
            if (result.IsEmpty())
                AddError("No intersecting properties found");
            return result;
        }

        private bool CompareWith(
            object objSource,
            object objCompare,
            PropertyOrField[] srcPropInfos,
            PropertyOrField[] comparePropInfos)
        {
            var didAnyComparison = false;
            var finalResult = srcPropInfos.Aggregate(true, (result, srcProp) =>
            {
                if (!result)
                    return false;
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

        private bool PropertyValuesMatchFor(
            object objSource,
            object objCompare,
            PropertyOrField srcProp,
            PropertyOrField compareProp)
        {
            var srcValue = srcProp.GetValue(objSource);
            var compareValue = compareProp.GetValue(objCompare);
            var result = CanPerformSimpleTypeMatchFor(srcProp)
                ? AreDeepEqualInternal(srcValue, compareValue)
                : MatchPropertiesOrCollection(srcValue, compareValue, srcProp, compareProp);
            if (!result)
            {
                AddError(
                    $"Property value mismatch for {srcProp.Name}: {Stringifier.Stringify(objSource)} vs {Stringifier.Stringify(objCompare)}"
                );
            }
            return result;
        }

        private bool MatchPropertiesOrCollection(
            object srcValue, 
            object compareValue,
            PropertyOrField srcProp,
            PropertyOrField compareProp
        )
        {
            var srcEnumerableInterface = TryGetEnumerableInterfaceFor(srcProp);
            var compareEnumerableInterface = TryGetEnumerableInterfaceFor(compareProp);
            if (srcEnumerableInterface == null && compareEnumerableInterface == null)
                return AreDeepEqualInternal(srcValue, compareValue);
            if (srcEnumerableInterface == null || compareEnumerableInterface == null)
                return false;
            return OnlyCompareShape || CollectionsMatch(
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
                .GetMethod(nameof(TestCollectionsMatch),
                    BindingFlags.Instance | BindingFlags.NonPublic);
            var typedMethod = genericMethod.MakeGenericMethod(t1, t2);
            return (bool) typedMethod.Invoke(this, new[] {srcValue, compareValue});
        }


#pragma warning disable S1144 // Unused private types or members should be removed
        // ReSharper disable once UnusedMember.Local
        private bool TestCollectionsMatch<T1, T2>(
            IEnumerable<T1> collection1,
            IEnumerable<T2> collection2
        )
        {
            if (collection1 == null && collection2 == null) return true;
            if (collection1 == null || collection2 == null) return false;
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
            var haveMatch = collection.Any(i => AreDeepEqualDetached(i, seek));
            if (!haveMatch)
            {
                AddError($"Unable to find match for: {Stringifier.Stringify(seek)}");
            }
            return haveMatch;
        }

        private bool AreDeepEqualDetached(object left, object right)
        {
            var tester = new DeepEqualityTester(left, right, _ignorePropertiesByName);
            return tester.AreDeepEqual(_pendingComparisons);
        }

#pragma warning restore S1144 // Unused private types or members should be removed

        private static Type TryGetEnumerableInterfaceFor(PropertyOrField prop)
        {
            return prop.Type
                .TryGetEnumerableInterface();
        }
    }
}