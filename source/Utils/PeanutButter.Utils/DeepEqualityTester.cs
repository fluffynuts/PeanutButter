using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
#if BUILD_PEANUTBUTTER_INTERNAL
using static Imported.PeanutButter.Utils.PyLike;
#else
using static PeanutButter.Utils.PyLike;
#endif

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

// ReSharper disable PossibleMultipleEnumeration

// ReSharper disable MemberCanBePrivate.Global

namespace
#if BUILD_PEANUTBUTTER_INTERNAL
Imported.PeanutButter.Utils
#else
PeanutButter.Utils
#endif
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
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        class DeepEqualityTester
    {
        private readonly object _objSource;
        private readonly object _objCompare;
        private readonly string[] _ignorePropertiesByName;
        private Dictionary<object, object> _pendingComparisons;

        /// <summary>
        /// Describes available methods for comparing enum values
        /// </summary>
        public enum EnumComparisonStrategies
        {
            /// <summary>
            /// Compare enum values by name (default)
            /// </summary>
            ByName,
            /// <summary>
            /// Compare enum values by object equality (.Equals())
            /// </summary>
            ByObjectEquals,
            /// <summary>
            /// Compare enum values by integer value
            /// </summary>
            ByIntegerValue
        }

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
        
        /// <summary>
        /// Include full object dumps when storing errors about property mismatches
        /// </summary>
        public bool VerbosePropertyMismatchErrors { get; set; } = true;

        /// <summary>
        /// When comparing enum values, forget their type and only compare
        /// their integer values
        /// </summary>
        public EnumComparisonStrategies EnumComparisonStrategy { get; set; } = EnumComparisonStrategies.ByName;

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
            {
                return true;
            }

            if (objSource == null || objCompare == null)
            {
                return false;
            }

            var sourceType = objSource.GetType();
            var compareType = objCompare.GetType();
            if (AreBothEnumTypes(sourceType, compareType))
            {
                return CompareEnums(objSource, objCompare);
            }

            if (AreBothSimpleOrNullableOfSimpleTypes(sourceType, compareType))
            {
                return AreSimpleEqual(sourceType, objSource, compareType, objCompare);
            }

            if (CanDetermineItemTypeForBoth(sourceType, compareType))
            {
                return DeepCollectionCompare(
                    sourceType,
                    objSource,
                    compareType,
                    objCompare
                );
            }

            return TryCompareWithCustomComparer(objSource, objCompare) ??
                   DeepCompare(
                       sourceType,
                       objSource,
                       compareType,
                       objCompare
                   );
        }

        private bool CompareEnums(object objSource, object objCompare)
        {
            switch (EnumComparisonStrategy)
            {
                case EnumComparisonStrategies.ByObjectEquals:
                    return objSource.Equals(objCompare);
                case EnumComparisonStrategies.ByIntegerValue:
                    return (int)objSource == (int)objCompare;
                case EnumComparisonStrategies.ByName:
                    return objSource.ToString() == objCompare.ToString();
                default:
                    throw new ArgumentOutOfRangeException($"{EnumComparisonStrategy}");
            }
        }

        private bool AreBothEnumTypes(Type sourceType, Type compareType)
        {
            return sourceType.IsEnum &&
                compareType.IsEnum;
        }

        private bool AreSimpleEqual(
            Type sourceType,
            object objSource,
            Type compareType,
            object objCompare)
        {
            // naive simple equality tester:
            //  if the types match, use .Equals, otherwise attempt upcasting to decimal
            //  so that, eg: (long)2 == (int)2
            if (sourceType == compareType)
                return OnlyCompareShape || PerformSameTypeEquals(objSource, objCompare);
            var sourceAsDecimal = TryConvertToDecimal(objSource);
            var compareAsDecimal = TryConvertToDecimal(objCompare);
            if (sourceAsDecimal == null || compareAsDecimal == null)
                return false;
            return OnlyCompareShape || PerformDecimalEquals(sourceAsDecimal.Value, compareAsDecimal.Value);
        }

        private bool PerformDecimalEquals(
            decimal left,
            decimal right)
        {
            var customResult = TryCompareWithCustomComparer(left, right);
            return customResult ?? left.Equals(right);
        }

        private bool PerformSameTypeEquals(
            object left,
            object right)
        {
            var customResult = TryCompareWithCustomComparer(left, right);
            if (customResult.HasValue)
                return customResult.Value;
            var result = left.Equals(right);
            if (IgnoreDateTimeKind())
                return result;
            if (!result)
                return false;

            if (left is DateTime leftDate &&
                right is DateTime rightDate)
            {
                return leftDate.Kind == rightDate.Kind;
            }

            return true;
        }

        private bool? TryCompareWithCustomComparer(
            object left,
            object right
        )
        {
            var method = TryCompareWithCustomComparerGenericMethod.MakeGenericMethod(left.GetType());
            try
            {
                return (bool?) method.Invoke(this, new[] { left, right });
            }
            catch
            {
                return null;
            }
        }

        private static readonly MethodInfo TryCompareWithCustomComparerGenericMethod
            = typeof(DeepEqualityTester).GetMethod(
                nameof(TryCompareWithCustomComparerGeneric),
                BindingFlags.Instance | BindingFlags.NonPublic
            );

        private bool? TryCompareWithCustomComparerGeneric<T>(
            T left,
            T right
        )
        {
            var comparer = _customComparers.OfType<IEqualityComparer<T>>().FirstOrDefault();
            return comparer?.Equals(left, right);
        }

        private bool? _ignoreDateTimeKind;

        private bool IgnoreDateTimeKind()
        {
            _ignoreDateTimeKind = _ignoreDateTimeKind ?? (CheckEnvironmentForIgnoreDateTimeKind());
            return _ignoreDateTimeKind ?? false;
        }

        private static readonly string[] Positives =
        {
            "true",
            "yes",
            "1"
        };

        private bool? CheckEnvironmentForIgnoreDateTimeKind()
        {
            var envVar = Environment.GetEnvironmentVariable("DEEP_EQUALITY_IGNORES_DATETIME_KIND");
            return Positives.Contains(envVar?.ToLower(CultureInfo.InvariantCulture));
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

        private bool DeepCollectionCompare(
            Type sourceType,
            object objSource,
            Type compareType,
            object objCompare)
        {
            var sourceItemType = GetItemTypeFor(sourceType);
            var compareItemType = GetItemTypeFor(compareType);
            var method = DeepCollectionCompareGenericMethod.MakeGenericMethod(
                sourceItemType,
                compareItemType
            );
            return (bool) method.Invoke(this, new[] { objSource, objCompare });
        }

        private static Type GetItemTypeFor(Type collectionType)
        {
            return collectionType.TryGetEnumerableItemType();
        }

        private static readonly MethodInfo DeepCollectionCompareGenericMethod =
#if NETSTANDARD
            typeof(DeepEqualityTester)
                .GetRuntimeMethods()
                .FirstOrDefault(mi => mi.Name == nameof(DeepCollectionCompareGeneric));
#else
            typeof(DeepEqualityTester).GetMethod(
                nameof(DeepCollectionCompareGeneric),
                BindingFlags.NonPublic | BindingFlags.Instance
            );
#endif

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
                    (acc, cur) => acc &&
                                  DeepCompareAtIndex(
                                      index++,
                                      cur.Item1,
                                      cur.Item2
                                  ));
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
                   Types.PrimitivesAndImmutables.Any(
                       si => si == t ||
#if NETSTANDARD
                             (t.IsConstructedGenericType &&
#else
                                                           (t.IsGenericType &&
#endif
                              t.GetGenericTypeDefinition() == typeof(Nullable<>) &&
                              Nullable.GetUnderlyingType(t) == si));
        }


        private static bool CanPerformSimpleTypeMatchFor(Type srcPropType)
        {
            return Types.PrimitivesAndImmutables.Any(st => st == srcPropType);
        }


        private PropertyOrField FindMatchingPropertyInfoFor(
            PropertyOrField srcPropInfo,
            IEnumerable<PropertyOrField> compareProperties)
        {
            var comparePropInfo = compareProperties.FirstOrDefault(pi => pi.Name == srcPropInfo.Name);
            if (comparePropInfo == null)
            {
                if (FailOnMissingProperties)
                    AddError("Unable to find comparison property with name: '" + srcPropInfo.Name + "'");
                return null;
            }

            var compareType = comparePropInfo.Type;
            var srcType = srcPropInfo.Type;
            if (TypesAreComparable(srcType, compareType))
                return comparePropInfo;
            var srcIsEnumerable = srcType.ImplementsEnumerableGenericType();
            var comparisonIsEnumerable = compareType.ImplementsEnumerableGenericType();
            if (srcIsEnumerable && comparisonIsEnumerable)
                return comparePropInfo;

            AddErrorForMismatch(srcPropInfo, comparePropInfo, srcIsEnumerable || comparisonIsEnumerable);
            return null;
        }

        private void AddErrorForMismatch(
            PropertyOrField srcPropInfo,
            PropertyOrField compareInfo,
            bool eitherAreEnumerable
        )
        {
            if (eitherAreEnumerable)
            {
                AddError(
                    $@"Source property '{srcPropInfo.Name}' has type '{srcPropInfo.Type.Name}' but comparison property has type '{compareInfo.Type.Name}' and can't find common enumerability"
                );
            }
            else
            {
                AddError(
                    $@"Source property '{srcPropInfo.Name}' has type '{srcPropInfo.Type.Name}' but comparison property has type '{compareInfo.Type.Name}'"
                );
            }
        }

        private bool TypesAreComparable(Type srcType, Type compareType)
        {
            return _comparableStrategies.Aggregate(
                false,
                (acc, cur) => acc || cur(this, srcType, compareType)
            );
        }

        private readonly Func<DeepEqualityTester, Type, Type, bool>[] _comparableStrategies =
        {
            TypesAreIdentical,
            TypesAreCloseEnough,
            TypesAreBothEnums, // defer this to a later choice
            TypesHaveSimilarImmediateShape
        };

        private static bool TypesAreBothEnums(
            DeepEqualityTester arg1, 
            Type arg2, 
            Type arg3)
        {
            return arg2.IsEnum &&
                arg3.IsEnum;
        }

        private static bool TypesHaveSimilarImmediateShape(
            DeepEqualityTester tester,
            Type srcType,
            Type compareType
        )
        {
            if (CanPerformSimpleTypeMatchFor(srcType) || CanPerformSimpleTypeMatchFor(compareType))
                return false;
            var srcProps = tester.GetPropertiesAndFieldsOf(srcType);
            var compareProps = tester.GetPropertiesAndFieldsOf(compareType);
            if (!tester.FailOnMissingProperties)
                compareProps = compareProps
                               .Where(cp => srcProps.Any(sp => sp.Name == cp.Name))
                               .ToArray();
            return compareProps.Any() && compareProps.All(cp => srcProps.Any(sp => sp.Name == cp.Name));
        }

        // TODO: add a flag & even fuzzier matching to allow, for instance,
        //    comparison between int and decimal, when enabled by the consumer
        //    -> should not be default behavior
        private static readonly Tuple<Type, Type>[] LooselyComparableTypes =
        {
            Tuple.Create(typeof(int), typeof(long)),
            Tuple.Create(typeof(int), typeof(short)),
            Tuple.Create(typeof(long), typeof(short)),
            Tuple.Create(typeof(long), typeof(float)),
            Tuple.Create(typeof(float), typeof(double)),
            Tuple.Create(typeof(float), typeof(decimal)),
            Tuple.Create(typeof(double), typeof(decimal))
        };

        private static bool TypesAreCloseEnough(
            DeepEqualityTester tester,
            Type srcType,
            Type compareType
        )
        {
            return LooselyComparableTypes.Any(
                pair =>
                    (pair.Item1 == srcType && pair.Item2 == compareType) ||
                    (pair.Item2 == srcType && pair.Item1 == compareType)
            );
        }

        private static bool TypesAreIdentical(
            DeepEqualityTester tester,
            Type srcType,
            Type compareType
        )
        {
            return srcType == compareType;
        }

        private bool CanDetermineItemTypeForBoth(Type t1, Type t2)
        {
            return t1.TryGetEnumerableItemType() != null &&
                   t2.TryGetEnumerableItemType() != null;
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
            AddError(
                string.Join(
                    "\n",
                    "Property count mismatch",
                    $"Source has {srcPropInfos.Length} properties:",
                    $"{DumpPropertyInfo(srcPropInfos)}",
                    $"\nComparison has {compareProps.Length} properties:",
                    $"{DumpPropertyInfo(compareProps)}"
                ));
            return false;
        }

        private PropertyOrField[] GetPropertiesAndFieldsOf(Type sourceType)
        {
            var props = sourceType
#if NETSTANDARD
                        .GetRuntimeProperties().Where(pi => pi.GetAccessors().Any(a => a.IsPublic)).ToArray()
#else
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
#endif
                        .Encapsulate()
                        .Where(pi => !_ignorePropertiesByName.Contains(pi.Name))
                        .ToArray();
            if (IncludeFields)
            {
                var fields = sourceType
#if NETSTANDARD
                             .GetRuntimeFields().Where(fi => fi.IsPublic)
                             .ToArray() //.Where(fi => fi.IsPublic).ToArray()
#else
                    .GetFields(BindingFlags.Public | BindingFlags.Instance)
#endif
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
                   string.Join(
                       DUMP_DELIMITER,
                       propInfos.Select(pi => $"{pi.Type} {pi.Name}")
                   );
        }

        private PropertyOrField[] GetIntersectingPropertyInfos(
            IEnumerable<PropertyOrField> left,
            IEnumerable<PropertyOrField> right
        )
        {
            var result = left.Where(
                                 s =>
                                     FindMatchingPropertyInfoFor(s, right) !=
                                     null // right.Any(c => c.Name == s.Name && c.Type == s.Type)
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
            var finalResult = srcPropInfos.Aggregate(
                true,
                (result, srcProp) =>
                {
                    if (!result)
                        return false;
                    var compareProp = FindMatchingPropertyInfoFor(srcProp, comparePropInfos);
                    didAnyComparison = didAnyComparison || compareProp != null;
                    return (compareProp == null &&
                            !FailOnMissingProperties) ||
                           (compareProp != null &&
                            PropertyValuesMatchFor(objSource, objCompare, srcProp, compareProp));
                });
            return (srcPropInfos.IsEmpty() || didAnyComparison) && finalResult;
        }

        private bool IsPending(object objSource, object objCompare)
        {
            if (_pendingComparisons.TryGetValue(objSource, out var gotValue))
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
            var result = CanPerformSimpleTypeMatchFor(srcProp.Type)
                ? AreDeepEqualInternal(srcValue, compareValue)
                : MatchPropertiesOrCollection(srcValue, compareValue, srcProp, compareProp);
            if (!result)
            {
                var error = new[]
                {
                    $"{(srcProp.MemberType == PropertyOrFieldTypes.Property ? "Property" : "Field")}",
                    $"value mismatch for {srcProp.Name}: {srcValue.Stringify()} vs {compareValue.Stringify()}"
                };
                if (VerbosePropertyMismatchErrors)
                {
                    error = error.And($"when comparing {objSource.Stringify()} and {objCompare.Stringify()}");
                }

                AddError(
                    error.JoinWith(" ")
                );
            }

            return result;
        }

        private bool TryWrapEnumerable(
            object value,
            out object wrapped,
            out Type wrappedType)
        {
            wrapped = null;
            wrappedType = null;
            if (value == null)
            {
                return false;
            }
            
            var attempt = new EnumerableWrapper<object>(value);
            if (attempt.IsValid)
            {
                wrapped = attempt;
                wrappedType = typeof(EnumerableWrapper<object>);
            }
            return attempt.IsValid;
        }

        private bool MatchPropertiesOrCollection(
            object srcValue,
            object compareValue,
            PropertyOrField srcProp,
            PropertyOrField compareProp
        )
        {
            TryResolveEnumerable(
                ref srcValue,
                srcProp,
                out var srcEnumerableInterface);
            TryResolveEnumerable(
                ref compareValue,
                compareProp,
                out var compareEnumerableInterface);

            if (srcEnumerableInterface == null && 
                compareEnumerableInterface == null)
            {
                return AreDeepEqualInternal(srcValue, compareValue);
            }

            if (srcEnumerableInterface == null || 
                compareEnumerableInterface == null)
            {
                return false;
            }

            return OnlyCompareShape ||
                   CollectionsMatch(
                       srcValue,
                       srcEnumerableInterface,
                       compareValue,
                       compareEnumerableInterface
                   );
        }

        private void TryResolveEnumerable(
            ref object value,
            PropertyOrField prop,
            out Type resolvedType)
        {
            var enumerableInterface = TryGetEnumerableInterfaceFor(prop);
            if (enumerableInterface == null &&
                TryWrapEnumerable(value, out var resolved, out resolvedType))
            {
                value = resolved;
                return;
            }
            resolvedType = enumerableInterface;
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
#if NETSTANDARD
            var genericMethod = GetType()
                                .GetRuntimeMethods()
                                .Single(mi => mi.Name == nameof(TestCollectionsMatch));
#else
            var genericMethod = GetType()
                .GetMethod(nameof(TestCollectionsMatch),
                    BindingFlags.Instance | BindingFlags.NonPublic);
#endif
            if (genericMethod == null)
                throw new InvalidOperationException(
                    $"No '{nameof(TestCollectionsMatch)}' method found on {GetType().PrettyName()}"
                );
            var typedMethod = genericMethod.MakeGenericMethod(t1, t2);
            return (bool) typedMethod.Invoke(this, new[] { srcValue, compareValue });
        }


#pragma warning disable S1144 // Unused private types or members should be removed
        // ReSharper disable once UnusedMember.Local
        private bool TestCollectionsMatch<T1, T2>(
            IEnumerable<T1> collection1,
            IEnumerable<T2> collection2
        )
        {
            if (collection1 == null && collection2 == null)
                return true;
            if (collection1 == null || collection2 == null)
                return false;
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
                AddError($"Unable to find match for: {seek.Stringify()}");
            }

            return haveMatch;
        }

        private bool AreDeepEqualDetached(object left, object right)
        {
            var tester = new DeepEqualityTester(left, right, _ignorePropertiesByName)
            {
                EnumComparisonStrategy = EnumComparisonStrategy
            };
            tester.UseCustomComparers(_customComparers);
            return tester.AreDeepEqual(_pendingComparisons);
        }

        private void UseCustomComparers(List<object> customComparers)
        {
            _customComparers.AddRange(customComparers);
        }

#pragma warning restore S1144 // Unused private types or members should be removed

        private static Type TryGetEnumerableInterfaceFor(PropertyOrField prop)
        {
            return prop.Type
                       .TryGetEnumerableInterface();
        }

        private readonly List<object> _customComparers = new List<object>();

        /// <summary>
        /// Adds a custom comparer for the type T
        /// </summary>
        /// <param name="comparer"></param>
        /// <typeparam name="T"></typeparam>
        public void AddCustomComparer<T>(IEqualityComparer<T> comparer)
        {
            _customComparers.Add(comparer);
        }

        /// <summary>
        /// Adds a custom comparer to use for the specified type.
        /// Custom comparers must implement IComparer&lt;T&gt; where T
        /// becomes the type selection to use for when the comparer
        /// is invoked
        /// </summary>
        /// <param name="comparer"></param>
        public void AddCustomComparer(object comparer)
        {
            _customComparers.Add(comparer);
        }
    }
}