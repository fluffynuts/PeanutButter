using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using PeanutButter.DuckTyping.AutoConversion.Converters;
using PeanutButter.DuckTyping.Comparers;
using PeanutButter.DuckTyping.Shimming;

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.DuckTyping.Extensions
{
    internal static class DuckTypingHelperExtensions
    {
        private static readonly Dictionary<Type, PropertyInfoContainer> PropertyCache =
            new Dictionary<Type, PropertyInfoContainer>();

        private static readonly Dictionary<Type, MethodInfoContainer> MethodCache =
            new Dictionary<Type, MethodInfoContainer>();

        private static readonly IPropertyInfoFetcher DefaultPropertyInfoFetcher = new DefaultPropertyInfoFetcher();

        internal static Dictionary<string, PropertyInfo> FindProperties(this Type type)
        {
            return type.FindProperties(DefaultPropertyInfoFetcher);
        }

        internal static Dictionary<string, PropertyInfo> FindProperties(
            this Type type,
            IPropertyInfoFetcher fetcher)
        {
            lock (PropertyCache)
            {
                CachePropertiesIfRequired(type, fetcher);
                return PropertyCache[type].PropertyInfos;
            }
        }

        private static void CachePropertiesIfRequired(Type type, IPropertyInfoFetcher fetcher)
        {
            if (!PropertyCache.ContainsKey(type))
            {
                PropertyCache[type] = GetPropertiesFor(type, fetcher);
            }
        }

        internal static Dictionary<string, PropertyInfo> FindFuzzyProperties(this Type type)
        {
            return FindFuzzyProperties(type, DefaultPropertyInfoFetcher);
        }

        internal static Dictionary<string, PropertyInfo> FindFuzzyProperties(this Type type,
            IPropertyInfoFetcher fetcher)
        {
            lock (PropertyCache)
            {
                CachePropertiesIfRequired(type, fetcher);
                return PropertyCache[type].FuzzyPropertyInfos;
            }
        }

        private const BindingFlags SEEK_FLAGS = 
            BindingFlags.Instance | 
            BindingFlags.Public | 
            BindingFlags.FlattenHierarchy;

        private static PropertyInfoContainer GetPropertiesFor(Type type, IPropertyInfoFetcher fetcher)
        {
            var immediateProperties = fetcher.GetProperties(type, SEEK_FLAGS);
            var interfaceProperties = type.GetAllImplementedInterfaces()
                .Select(itype => fetcher.GetProperties(itype, SEEK_FLAGS))
                .SelectMany(p => p);
            var all = immediateProperties.Union(interfaceProperties).ToArray();
            return new PropertyInfoContainer(all);
        }

        internal static Dictionary<string, MethodInfo> FindMethods(this Type type)
        {
            lock (MethodCache)
            {
                CacheMethodInfosIfRequired(type);
                return MethodCache[type].MethodInfos;
            }
        }

        internal static Dictionary<string, MethodInfo> FindFuzzyMethods(
            this Type type
        )
        {
            lock (MethodCache)
            {
                CacheMethodInfosIfRequired(type);
                return MethodCache[type].FuzzyMethodInfos;
            }
        }

        internal static Type[] GetAllImplementedInterfaces(this Type interfaceType)
        {
            var result = new List<Type> {interfaceType};
            foreach (var type in interfaceType.GetInterfaces())
            {
                result.AddRange(type.GetAllImplementedInterfaces());
            }
            return result.ToArray();
        }


        private static void CacheMethodInfosIfRequired(Type type)
        {
            if (!MethodCache.ContainsKey(type))
            {
                MethodCache[type] = GetMethodsFor(type);
            }
        }

        private static MethodInfoContainer GetMethodsFor(Type type)
        {
            return new MethodInfoContainer(
                type.GetMethods(SEEK_FLAGS)
                    .Where(mi => !mi.IsSpecial())
                    .ToArray()
            );
        }

        internal static IEnumerable<KeyValuePair<string, PropertyInfo>> FindAccessMismatches(
            this Dictionary<string, PropertyInfo> authoritative,
            Dictionary<string, PropertyInfo> test
        )
        {
            return test.Where(t =>
                authoritative.TryGetValue(t.Key, out var authoritativePropInfo) &&
                t.Value.IsMoreRestrictiveThan(authoritativePropInfo));
        }

        internal static Dictionary<string, PropertyInfo> FindPrimitivePropertyMismatches(
            this Dictionary<string, PropertyInfo> src,
            Dictionary<string, PropertyInfo> other,
            bool allowFuzzy
        )
        {
            return other.Where(kvp => !src.HasNonComplexPropertyMatching(kvp.Value))
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value,
                    allowFuzzy
                        ? Comparers.Comparers.FuzzyComparer
                        : Comparers.Comparers.NonFuzzyComparer);
        }

        internal static bool IsSuperSetOf(
            this Dictionary<string, MethodInfo> src,
            Dictionary<string, MethodInfo> other)
        {
            return other.All(kvp => src.HasMethodMatching(kvp.Value));
        }


        static readonly HashSet<Type> TreatAsPrimitives = new HashSet<Type>(new[]
        {
            typeof(string),
            typeof(Guid),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan)
        });

        internal static Dictionary<string, PropertyInfo> GetPrimitiveProperties(
            this Dictionary<string, PropertyInfo> props,
            bool allowFuzzy
        )
        {
            // this will cause oddness with structs. Will have to do for now
            return props.Where(kvp => kvp.Value.PropertyType.ShouldTreatAsPrimitive())
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value,
                    allowFuzzy
                        ? Comparers.Comparers.FuzzyComparer
                        : Comparers.Comparers.NonFuzzyComparer);
        }

        internal static bool ShouldTreatAsPrimitive(this Type type)
        {
            return type.IsPrimitive || // types .net thinks are primitive
                   type.IsValueType || // includes enums, structs, https://msdn.microsoft.com/en-us/library/s1ax56ch.aspx
                   type.IsArray ||
                   TreatAsPrimitives.Contains(type); // catch cases like strings and Date(/Time) containers
        }

        internal static bool HasNonComplexPropertyMatching(
            this Dictionary<string, PropertyInfo> haystack,
            PropertyInfo needle
        )
        {
            if (!haystack.TryGetValue(needle.Name, out var matchByName))
                return false;
            if (!matchByName.PropertyType.ShouldTreatAsPrimitive())
                return false;
            if (needle.IsReadOnly() &&
                matchByName.CanRead &&
                needle.PropertyType.IsAssignableFrom(matchByName.PropertyType))
                return true;
            return MatchesTypeOrCanEnumConvert(needle, matchByName) &&
                   matchByName.IsNoMoreRestrictiveThan(needle);
        }

        private static bool MatchesTypeOrCanEnumConvert(PropertyInfo needle, PropertyInfo matchByName)
        {
            return matchByName.PropertyType == needle.PropertyType ||
                   EnumConverter.CanPerhapsConvertBetween(matchByName.PropertyType, needle.PropertyType);
        }

        internal static bool IsReadOnly(this PropertyInfo propInfo)
        {
            return propInfo.CanRead && !propInfo.CanWrite;
        }

        internal static bool IsNoMoreRestrictiveThan(
            this PropertyInfo src,
            PropertyInfo target
        )
        {
            var atLeastAsReadable = !target.CanRead || src.CanRead;
            var atLeastAsWritable = !target.CanWrite || src.CanWrite;
            return atLeastAsWritable && atLeastAsReadable;
        }

        internal static bool IsMoreRestrictiveThan(
            this PropertyInfo src,
            PropertyInfo target
        )
        {
            var isLessReadable = target.CanRead && !src.CanRead;
            var isLessWritable = target.CanWrite && !src.CanWrite;
            return isLessWritable || isLessReadable;
        }


        internal static bool IsTryParseMethod(
            this MethodInfo mi
        )
        {
            if (mi.Name != "TryParse")
                return false;
            var parameters = mi.GetParameters();
            if (parameters.Length != 2)
                return false;
            return parameters[0].ParameterType == typeof(string) &&
                   parameters[1].IsOut;
        }

        internal static bool HasMethodMatching(
            this Dictionary<string, MethodInfo> haystack,
            MethodInfo needle
        )
        {
            if (!haystack.TryGetValue(needle.Name, out var matchByName))
                return false;
            return matchByName.ReturnType == needle.ReturnType &&
                   matchByName.ExactlyMatchesParametersOf(needle);
        }

        internal static bool ExactlyMatchesParametersOf(
            this MethodInfo src,
            MethodInfo other
        )
        {
            var srcParameters = src.GetParameters();
            var otherParameters = other.GetParameters();
            if (srcParameters.Length != otherParameters.Length)
                return false;
            for (var i = 0; i < srcParameters.Length; i++)
            {
                var p1 = srcParameters[i];
                var p2 = otherParameters[i];
                // only care about positioning and type
                if (p1.ParameterType != p2.ParameterType)
                    return false;
            }
            return true;
        }

        private static readonly IEqualityComparer<string>[] CaseInsensitiveComparers =
        {
            StringComparer.OrdinalIgnoreCase,
            StringComparer.CurrentCultureIgnoreCase,
            StringComparer.InvariantCultureIgnoreCase
        };

        internal static bool IsCaseSensitive(
            this IDictionary<string, object> dictionary
        )
        {
            var comparerProp = dictionary?.GetType().GetProperty("Comparer");
            return comparerProp == null
                ? BruteForceIsCaseSensitive(dictionary)
                : !CaseInsensitiveComparers.Contains(comparerProp.GetValue(dictionary));
        }

        internal static bool ContainsCaseSensitiveDictionary(
            this IDictionary<string, object> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                var asDict = kvp.Value as IDictionary<string, object>; // WRONG! what about different-typed sub-dicts?
                if (asDict == null)
                    continue;
                if (asDict.IsCaseSensitive())
                    return true;
            }
            return false;
        }

        private static bool BruteForceIsCaseSensitive(IDictionary<string, object> data)
        {
            if (data == null)
                return false;
            string upper = null;
            string lower = null;
            foreach (var key in GetKeysOf(data))
            {
                upper = key.ToUpper(CultureInfo.InvariantCulture);
                lower = key.ToLower(CultureInfo.InvariantCulture);
                if (upper != lower)
                    break;
                upper = null;
            }
            if (upper == null)
                return false;
            return !(data.ContainsKey(lower) && data.ContainsKey(upper));
        }

        private static ICollection<TKey> GetKeysOf<TKey, TValue>(IDictionary<TKey, TValue> data)
        {
            ICollection<TKey> result;
            try
            {
                result = data.Keys;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Unable to get keys from provided dictionary; examing inner exception", ex);
            }
            if (result == null)
                throw new InvalidOperationException("Provided dictionary gives null for keys");
            return result;
        }

        internal static bool IsSpecial(this MethodInfo methodInfo)
        {
            return ((int) methodInfo.Attributes & (int) MethodAttributes.SpecialName) ==
                   (int) MethodAttributes.SpecialName;
        }

        internal static IDictionary<string, object> ToCaseInsensitiveDictionary(
            this IDictionary<string, object> data
        )
        {
            return data.ToCaseInsensitiveDictionary(new List<object>());
        }

        internal static IDictionary<string, object> ToCaseInsensitiveDictionary(
            this IDictionary<string, object> data,
            List<object> seenObjects
        )
        {
            // TODO: replace this logic with a case-insensitive passthrough / wrapper
            //   to enable proper write-back to the underlying data source
            if (!data.IsCaseSensitive() &&
                !data.ContainsCaseSensitiveDictionary())
                return data;
            var outer = new Dictionary<string, object>(data, StringComparer.OrdinalIgnoreCase);
            var toReplace = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in outer)
            {
                if (seenObjects.Contains(kvp.Value))
                    continue;
                seenObjects.Add(kvp.Value);
                var inner = kvp.Value as IDictionary<string, object>; // sub-dicts are being coerced )':
                if (inner.IsCaseSensitive())
                {
                    toReplace[kvp.Key] = inner.ToCaseInsensitiveDictionary(seenObjects);
                }
            }
            foreach (var kvp in toReplace)
            {
                outer[kvp.Key] = kvp.Value;
            }
            return outer;
        }

        private static readonly Type NullableGeneric = typeof(Nullable<>);

        internal static bool IsNullableType(this Type t)
        {
            return t != null &&
                t.IsGenericType &&
                t.GetGenericTypeDefinition() == NullableGeneric;
        }
    }
}