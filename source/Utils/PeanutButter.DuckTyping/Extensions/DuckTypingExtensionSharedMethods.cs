using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PeanutButter.DuckTyping.AutoConversion;
using PeanutButter.DuckTyping.Comparers;
using PeanutButter.DuckTyping.Exceptions;
using PeanutButter.DuckTyping.Shimming;

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.DuckTyping.Extensions
{
    internal static class DuckableTypesCache
    {
        private class DuckCacheItem
        {
            public Type FromType { get; }
            public Type ToType { get; }
            public bool Duckable { get; }
            public DuckCacheItem(Type fromType, Type toType, bool duckable)
            {
                ToType = toType;
                FromType = fromType;
                Duckable = duckable;
            }
        }
        private static readonly List<DuckCacheItem> Duckables = new List<DuckCacheItem>();
        private static readonly List<DuckCacheItem> FuzzyDuckables = new List<DuckCacheItem>();
        private static readonly object Lock = new object();

        internal static void CacheDuckable(
            Type from, 
            Type to,
            bool result,
            bool fuzzy)
        {
            lock(Lock)
            {
                var cacheItem = new DuckCacheItem(from, to, result);
                Duckables.Add(cacheItem);
                if (fuzzy)
                    FuzzyDuckables.Add(cacheItem);
            }
        }

        internal static bool CanDuckAs<T>(Type toDuckFrom, bool fuzzy)
        {
            lock(Lock)
            {
                return HaveMatch<T>(fuzzy ? FuzzyDuckables : Duckables, toDuckFrom);
            }
        }

        private static bool HaveMatch<T>(
            IEnumerable<DuckCacheItem> cache,
            Type toDuckFrom
        )
        {
            lock(Lock)
            {
                var check = typeof(T);
                var match = cache.FirstOrDefault(
                    o => o.FromType == toDuckFrom &&
                         o.ToType == check);
                return match?.Duckable ?? false;
            }
        }
    }
    /// <summary>
    /// Provides a set of extension methods to enable duck-typing
    /// </summary>
    public static class DuckTypingExtensionSharedMethods
    {
        internal static readonly MethodInfo GenericFuzzyDuckAsMethod =
            FindGenericDuckMethodFromObjectExtensions(nameof(DuckTypingObjectExtensions.FuzzyDuckAs));

        internal static readonly MethodInfo GenericDuckAsMethod =
            FindGenericDuckMethodFromObjectExtensions(nameof(DuckTypingObjectExtensions.DuckAs));

        private static MethodInfo FindGenericDuckMethodFromObjectExtensions(string name)
        {
            return typeof(DuckTypingObjectExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(mi => mi.Name == name &&
                                      mi.IsGenericMethod &&
                                      IsCorrectSignature(mi));
        }

        private static bool IsCorrectSignature(MethodInfo mi)
        {
            var parameters = mi.GetParameters();
            return parameters.Length == 2 &&
                   parameters[0].ParameterType == typeof(object) &&
                   parameters[1].ParameterType == typeof(bool);
        }

        internal static object UnwrapTargetInvokationFor(Func<object> toRun)
        {
            try
            {
                return toRun();
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
        }

        internal static object NonGenericDuck(
            object src,
            Type toType,
            bool throwOnError,
            MethodInfo genericMethod
        )
        {
            var specific = genericMethod.MakeGenericMethod(toType);
            return specific.Invoke(null, new[] { src, throwOnError });
        }

        internal static T ForceDuckAs<T>(IDictionary<string, object> src, bool allowFuzzy)
        {
            var typeMaker = new TypeMaker();
            var type = allowFuzzy ? typeMaker.MakeTypeImplementing<T>() : typeMaker.MakeFuzzyTypeImplementing<T>();
            return (T)Activator.CreateInstance(type, new object[] { new IDictionary<string, object>[] { src } });
        }

        internal static bool PrivateCanDuckAs<T>(this object src, bool allowFuzzy, bool throwOnError)
        {
            var asDictionary = TryConvertToDictionary(src);
            if (asDictionary != null)
                return asDictionary.CanDuckDictionaryAs<T>(allowFuzzy, throwOnError);
            var type = typeof(T);
            var srcType = src.GetType();
            return DuckableTypesCache.CanDuckAs<T>(srcType, allowFuzzy) ||
                    CacheDuckResult(
                        type.InternalCanDuckAs(srcType, allowFuzzy, throwOnError),
                        allowFuzzy,
                        srcType,
                        type
                    );
        }

        private static bool CacheDuckResult(
            bool calculatedResult, 
            bool allowFuzzy,
            Type from,
            Type to)
        {
            DuckableTypesCache.CacheDuckable(
                from, to, calculatedResult, allowFuzzy
            );
            return calculatedResult;
        }

        private static bool CanDuckDictionaryAs<T>(
            this IDictionary<string, object> src,
            bool allowFuzzy,
            bool throwOnError
        )
        {
            var type = typeof(T);
            return CanDuckDictionaryAs(src, type, allowFuzzy, throwOnError);
        }

        private static bool CanDuckDictionaryAs(
            IDictionary<string, object> src,
            Type type,
            bool allowFuzzy,
            // ReSharper disable once UnusedParameter.Local
            bool throwOnError
        )
        {
            var errors = DictionaryDuckErrorsFor(src, type, allowFuzzy);
            if (throwOnError && errors.Any())
                throw new UnDuckableException(errors);
            return !errors.Any();
        }

        private static List<string> DictionaryDuckErrorsFor(
            IDictionary<string, object> src,
            Type type,
            bool allowFuzzy
        )
        {
            var properties = type
                .GetAllImplementedInterfaces()
                .SelectMany(itype => itype.GetProperties())
                .Distinct(new PropertyInfoComparer());
            src = allowFuzzy ? src.ToCaseInsensitiveDictionary() : src;
            var errors = new List<string>();
            foreach (var prop in properties)
            {
                FindDictionaryDuckErrorFor(prop, src, allowFuzzy, errors);
            }
            return errors;
        }

        private static IDictionary<string, object> TryConvertToDictionary(object src)
        {
            var result = src as IDictionary<string, object>;
            if (result != null)
                return result;
            var dictionaryTypes = src.GetType().GetInterfaces()
                    .Select(GetDictionaryTypes)
                    .FirstOrDefault();

            if (dictionaryTypes == null || dictionaryTypes.KeyType != typeof(string))
                return null;

            var method = GenericConvertDictionaryMethod.MakeGenericMethod(dictionaryTypes.ValueType);
            return (IDictionary<string, object>)method.Invoke(null, new[] { src });
        }

        private static readonly MethodInfo GenericConvertDictionaryMethod =
            typeof(DuckTypingExtensionSharedMethods).GetMethod(nameof(ConvertDictionary),
                BindingFlags.Static | BindingFlags.NonPublic);

        private static IDictionary<string, object> ConvertDictionary<T>(IDictionary<string, T> src)
        {
            return src.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as object);
        }

        private class KeyValueTypes
        {
            public Type KeyType { get; }
            public Type ValueType { get; }

            public KeyValueTypes(Type keyType, Type valueType)
            {
                KeyType = keyType;
                ValueType = valueType;
            }
        }

        private static KeyValueTypes GetDictionaryTypes(Type t)
        {
            if (!t.IsGenericType)
                return null;
            var generic = t.GetGenericTypeDefinition();
            if (generic != typeof(IDictionary<,>))
                return null;
            var addParameters = t.GetMethods().Where(mi => mi.Name == "Add")
                .Select(mi => mi.GetParameters())
                .FirstOrDefault(p => p.Length == 2);
            if (addParameters == null)
                return null;
            return new KeyValueTypes(addParameters[0].ParameterType, addParameters[1].ParameterType);
        }

        private static void FindDictionaryDuckErrorFor(
            PropertyInfo prop,
            IDictionary<string, object> src,
            bool allowFuzzy,
            List<string> errors
        )
        {
            var targetType = prop.PropertyType;
            var finder = new FuzzyKeyFinder();
            object stored;
            var key = allowFuzzy
                        ? finder.FuzzyFindKeyFor(src, prop.Name)
                        : src.Keys.FirstOrDefault(k => k == prop.Name);

            if (key == null || !src.TryGetValue(key, out stored))
            {
                if (prop.PropertyType.IsNullable())
                {
                    return;
                }
                errors.Add($"No value found for {prop.Name} and property is not nullable ({prop.PropertyType.Name})");
                return;
            }
            if (stored == null)
            {
                if (ShimShamBase.GetDefaultValueFor(targetType) != null)
                    errors.Add(
                        $"Stored value for {prop.Name} is null but target type {targetType.Name} does not allow nulls"
                    );
                return;
            }
            var asDictionary = TryConvertToDictionary(stored);
            if (asDictionary != null)
            {
                try
                {
                    CanDuckDictionaryAs(asDictionary, targetType, allowFuzzy, true);
                }
                catch (UnDuckableException ex)
                {
                    errors.Add(
                        $"Property {prop.Name} is dictionary but can't be ducked to {targetType.Name}; examine following errors for more information:");
                    errors.AddRange(ex.Errors.Select(e => $"{prop.Name}: {e}"));
                }
                return;
            }

#pragma warning disable S2219 // Runtime type checking should be simplified
            // ReSharper disable once UseMethodIsInstanceOfType
            var srcType = stored.GetType();
            if (!targetType.IsAssignableFrom(srcType))
            {
                if (allowFuzzy && CanAutoConvert(srcType, targetType))
                    return;
#pragma warning restore S2219 // Runtime type checking should be simplified
                errors.Add(
                    $"{targetType.Name} is not assignable from {srcType.Name}{(allowFuzzy ? " and no converter found" : "")}");
            }
        }

        private static readonly string[] ObjectMethodNames =
            typeof(object).GetMethods().Select(m => m.Name).ToArray();

        internal static bool InternalCanDuckAs(
            this Type type,
            Type toType,
            bool allowFuzzy,
            // ReSharper disable once UnusedParameter.Global
            bool throwIfNotAllowed
        )
        {
            var errors = GetDuckErrorsFor(type, toType, allowFuzzy);
            if (throwIfNotAllowed && errors.Any())
                throw new UnDuckableException(errors);
            return !errors.Any();
        }

        internal static List<string> GetDuckErrorsFor(
            this Type type,
            Type toType,
            bool allowFuzzy
        )
        {
            var expectedProperties = allowFuzzy
                ? type.FindFuzzyProperties()
                : type.FindProperties();
            var expectedPrimitives = expectedProperties.GetPrimitiveProperties(allowFuzzy);
            var srcProperties = allowFuzzy ? toType.FindFuzzyProperties() : toType.FindProperties();
            var srcPrimitives = srcProperties.GetPrimitiveProperties(allowFuzzy);

            var mismatches = srcPrimitives.FindPrimitivePropertyMismatches(expectedPrimitives, allowFuzzy);
            var errors = new List<string>();
            if (mismatches.Any())
            {
                AddPropertyMismatchErrorFor(mismatches, srcPrimitives, expectedPrimitives, allowFuzzy, errors);
            }

            var expectedMethods = allowFuzzy ? type.FindFuzzyMethods() : type.FindMethods();
            if (toType.IsInterface)
                expectedMethods = expectedMethods.Except(ObjectMethodNames);
            var srcMethods = allowFuzzy ? toType.FindFuzzyMethods() : toType.FindMethods();
            if (!srcMethods.IsSuperSetOf(expectedMethods))
                errors.Add("One or more methods could not be ducked");
            return errors;
        }

        private static void AddPropertyMismatchErrorFor(
            Dictionary<string, PropertyInfo> mismatches,
            Dictionary<string, PropertyInfo> srcPrimitives,
            Dictionary<string, PropertyInfo> expectedPrimitives,
            bool allowFuzzy,
            List<string> errors
        )
        {
            var missing = mismatches
                .Where(kvp => !srcPrimitives.ContainsKey(kvp.Key))
                .ToArray();
            if (missing.Any())
            {
                errors.Add($"Missing target {Prop(missing.Length)} {NamesOf(missing)}");
                return;
            }
            var accessMismatches = mismatches.Where(kvp =>
                    !expectedPrimitives[kvp.Key].IsNoMoreRestrictiveThan(srcPrimitives[kvp.Key]))
                .ToArray();
            if (accessMismatches.Any())
            {
                errors.Add(
                    $"Mismatched target accessors for {MakeTargetAccessorMessageFor(accessMismatches, srcPrimitives)}");
                return;
            }
            var typeMismatchError =
                $"Type mismatch for {Prop(mismatches.Count)}: {GetTypeMismatchErrorsFor(mismatches, expectedPrimitives)}";
            if (!allowFuzzy)
            {
                errors.Add(typeMismatchError);
                return;
            }
            if (!HaveConvertersFor(mismatches, expectedPrimitives))
            {
                errors.Add(typeMismatchError + " and one or more could not be auto-converted");
            }
        }

        private static string GetTypeMismatchErrorsFor(
            Dictionary<string, PropertyInfo> mismatches,
            Dictionary<string, PropertyInfo> expectedPrimitives)
        {
            var parts =
                mismatches.Select(
                    kvp =>
                        $"{kvp.Key}: {kvp.Value.PropertyType.Name} -> {expectedPrimitives[kvp.Key].PropertyType.Name}");
            return string.Join(", ", parts);
        }

        private static string MakeTargetAccessorMessageFor(
            IEnumerable<KeyValuePair<string, PropertyInfo>> accessMismatches,
            Dictionary<string, PropertyInfo> expectedPrimitives)
        {
            var legends =
                accessMismatches.Select(
                    kvp => $"{kvp.Key} {GetSetFor(expectedPrimitives[kvp.Key])} -> {GetSetFor(kvp.Value)}");
            return string.Join(", ", legends);
        }

        private static string GetSetFor(PropertyInfo argValue)
        {
            var parts = new List<string>();
            if (argValue.CanRead)
                parts.Add("get");
            if (argValue.CanWrite)
                parts.Add("set");
            return string.Join("/", parts);
        }

        private static string NamesOf(KeyValuePair<string, PropertyInfo>[] missing)
        {
            return string.Join(", ", missing.Select(kvp => kvp.Key));
        }

        private static string Prop(int count)
        {
            return count == 1 ? "property" : "properties";
        }

        private static bool HaveConvertersFor(
            Dictionary<string, PropertyInfo> mismatches,
            Dictionary<string, PropertyInfo> expectedPrimitives)
        {
            foreach (var kvp in mismatches)
            {
                var srcType = kvp.Value.PropertyType;
                var targetType = expectedPrimitives[kvp.Key].PropertyType;
                if (!CanAutoConvert(srcType, targetType))
                    return false;
            }
            return true;
        }

        private static bool CanAutoConvert(Type srcType, Type targetType)
        {
            return ConverterLocator.GetConverter(srcType, targetType) != null;
        }

        private static Dictionary<string, MethodInfo> Except(
            this Dictionary<string, MethodInfo> src,
            IEnumerable<string> others
        )
        {
            return src.Where(kvp => !others.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        internal static T InternalDuckAs<T>(this object src, bool allowFuzzy, bool throwOnError) where T : class
        {
            if (src == null) return null;
            var duckable = PrivateCanDuckAs<T>(src, allowFuzzy, throwOnError);
            if (!duckable) return null;
            var srcAsDict = TryConvertToDictionary(src);
            if (allowFuzzy)
                srcAsDict = srcAsDict?.ToCaseInsensitiveDictionary();

            var duckType = FindOrCreateDuckTypeFor<T>(allowFuzzy);
            var ctorArgs = srcAsDict == null
                    ? new object[] { new object[] { src } }
                    : new object[] { new IDictionary<string, object>[] { srcAsDict } };
            return (T)Activator.CreateInstance(duckType, ctorArgs);
        }

        private static readonly Dictionary<Type, TypePair> DuckTypes
            = new Dictionary<Type, TypePair>();

        private static Type FindOrCreateDuckTypeFor<T>(bool isFuzzy)
        {
            var key = typeof(T);
            return FindOrCreateDuckTypeFor<T>(key, isFuzzy);
        }

        private static Type FindOrCreateDuckTypeFor<T>(Type key, bool isFuzzy)
        {
            lock (DuckTypes)
            {
                if (!DuckTypes.ContainsKey(key))
                {
                    DuckTypes[key] = new TypePair();
                }
                var match = DuckTypes[key];
                return isFuzzy ? GetFuzzyTypeFrom<T>(match) : GetTypeFrom<T>(match);
            }
        }

        private static Type GetTypeFrom<T>(TypePair match)
        {
            return match.Type ?? (match.Type = CreateDuckTypeFor<T>(false));
        }

        private static Type GetFuzzyTypeFrom<T>(TypePair match)
        {
            return match.FuzzyType ?? (match.FuzzyType = CreateDuckTypeFor<T>(true));
        }


        private static Type CreateDuckTypeFor<T>(bool isFuzzy)
        {
            var typeMaker = new TypeMaker();
            return isFuzzy
                ? typeMaker.MakeFuzzyTypeImplementing<T>()
                : typeMaker.MakeTypeImplementing<T>();
        }
    }
}