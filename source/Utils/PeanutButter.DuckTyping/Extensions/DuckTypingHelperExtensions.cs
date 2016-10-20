using System;
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
            return new PropertyInfoContainer(type.GetProperties(_seekFlags));
        }

        internal static Dictionary<string, MethodInfo> FindMethods(this Type type)
        {
            lock(_methodCache)
            {
                CacheMethodInfosIfRequired(type);
                return _methodCache[type].MethodInfos;
            }
        }
        internal static Dictionary<string, MethodInfo> FindFuzzyMethods(
            this Type type
        )
        {
            lock(_methodCache)
            {
                CacheMethodInfosIfRequired(type);
                return _methodCache[type].FuzzyMethodInfos;
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

        internal static bool IsSuperSetOf(
            this Dictionary<string, PropertyInfo> src,
            Dictionary<string, PropertyInfo> other)
        {
            return other.All(kvp => src.HasPropertyMatching(kvp.Value));
        }


        internal static bool IsSuperSetOf(
            this Dictionary<string, MethodInfo> src,
            Dictionary<string, MethodInfo> other)
        {
            return other.All(kvp => src.HasMethodMatching(kvp.Value));
        }

        internal static bool HasPropertyMatching(
            this Dictionary<string, PropertyInfo>  haystack,
            PropertyInfo needle
        )
        {
            PropertyInfo matchByName;
            if (!haystack.TryGetValue(needle.Name, out matchByName))
                return false;
            return matchByName.PropertyType == needle.PropertyType &&
                   (!needle.CanRead || matchByName.CanRead) &&
                   (!needle.CanWrite || matchByName.CanWrite);
        }

        internal static bool HasMethodMatching(
            this Dictionary<string, MethodInfo>  haystack,
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