using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.DuckTyping.Extensions
{
    internal static class DuckTypingHelperExtensions
    {
        private static readonly Dictionary<Type, PropertyInfoContainer> _propertyCache =
            new Dictionary<Type, PropertyInfoContainer>();
        private static readonly Dictionary<Type, MethodInfoContainer> _methodCache =
            new Dictionary<Type, MethodInfoContainer>();
        internal static Dictionary<string, PropertyInfo> FindProperties(this Type type)
        {
            lock (_propertyCache)
            {
                CachePropertiesIfRequired(type);
                return _propertyCache[type].PropertyInfos;
            }
        }

        private static void CachePropertiesIfRequired(Type type)
        {
            if (!_propertyCache.ContainsKey(type))
            {
                _propertyCache[type] = GetPropertiesFor(type);
            }
        }

        internal static Dictionary<string, PropertyInfo> FindFuzzyProperties(this Type type)
        {
            lock (_propertyCache)
            {
                CachePropertiesIfRequired(type);
                return _propertyCache[type].FuzzyPropertyInfos;
            }
        }

        private static readonly BindingFlags _seekFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;

        private static PropertyInfoContainer GetPropertiesFor(Type type)
        {
            return new PropertyInfoContainer(
                type.GetAllImplementedInterfaces()
                            .Select(i => i.GetProperties(_seekFlags))
                            .SelectMany(p => p)
                    .ToArray());
        }

        internal static Dictionary<string, MethodInfo> FindMethods(this Type type)
        {
            lock (_methodCache)
            {
                CacheMethodInfosIfRequired(type);
                return _methodCache[type].MethodInfos;
            }
        }
        internal static Dictionary<string, MethodInfo> FindFuzzyMethods(
            this Type type
        )
        {
            lock (_methodCache)
            {
                CacheMethodInfosIfRequired(type);
                return _methodCache[type].FuzzyMethodInfos;
            }
        }

        internal static Type[] GetAllImplementedInterfaces(this Type interfaceType)
        {
            var result = new List<Type> { interfaceType };
            foreach (var type in interfaceType.GetInterfaces())
            {
                result.AddRange(type.GetAllImplementedInterfaces());
            }
            return result.ToArray();
        }


        private static void CacheMethodInfosIfRequired(Type type)
        {
            if (!_methodCache.ContainsKey(type))
            {
                _methodCache[type] = GetMethodsFor(type);
            }
        }

        private static MethodInfoContainer GetMethodsFor(Type type)
        {
            return new MethodInfoContainer(
                type.GetMethods(_seekFlags)
                    .Where(mi => !mi.IsSpecial())
                    .ToArray()
            );
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
                    allowFuzzy ? Comparers.FuzzyComparer : Comparers.NonFuzzyComparer);
        }

        internal static bool IsPrimitiveSuperSetOf(
            this Dictionary<string, PropertyInfo> src,
            Dictionary<string, PropertyInfo> other
        )
        {
            return !src.FindPrimitivePropertyMismatches(other, true).Any();
        }


        internal static bool IsSuperSetOf(
            this Dictionary<string, MethodInfo> src,
            Dictionary<string, MethodInfo> other)
        {
            return other.All(kvp => src.HasMethodMatching(kvp.Value));
        }


        static readonly HashSet<Type> _treatAsPrimitives = new HashSet<Type>(new[] {
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
                                ? Comparers.FuzzyComparer
                                : Comparers.NonFuzzyComparer);
        }

        internal static bool ShouldTreatAsPrimitive(this Type type)
        {
            return type.IsPrimitive || // types .net thinks are primitive
                    type.IsValueType || // includes enums, structs, https://msdn.microsoft.com/en-us/library/s1ax56ch.aspx
                    _treatAsPrimitives.Contains(type); // catch cases like strings and Date(/Time) containers
        }

        internal static bool HasNonComplexPropertyMatching(
            this Dictionary<string, PropertyInfo> haystack,
            PropertyInfo needle
        )
        {
            PropertyInfo matchByName;
            if (!haystack.TryGetValue(needle.Name, out matchByName))
                return false;
            if (!matchByName.PropertyType.ShouldTreatAsPrimitive())
                return true;
            return matchByName.PropertyType == needle.PropertyType &&
                    needle.IsNoMoreRestrictiveThan(matchByName);
//                   (!needle.CanRead || matchByName.CanRead) &&
//                   (!needle.CanWrite || matchByName.CanWrite);
        }

        internal static bool IsNoMoreRestrictiveThan(
            this PropertyInfo src,
            PropertyInfo target
        )
        {
            return (!src.CanRead || target.CanRead) &&
                    (!src.CanWrite || target.CanWrite);
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
            if (parameters[0].ParameterType != typeof(string))
                return false;
            return parameters[1].IsOut;
        }

        internal static bool HasMethodMatching(
            this Dictionary<string, MethodInfo> haystack,
            MethodInfo needle
        )
        {
            MethodInfo matchByName;
            if (!haystack.TryGetValue(needle.Name, out matchByName))
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

        internal static bool IsSpecial(this MethodInfo methodInfo)
        {
            return ((int)methodInfo.Attributes & (int)MethodAttributes.SpecialName) == (int)MethodAttributes.SpecialName;
        }
    }
}