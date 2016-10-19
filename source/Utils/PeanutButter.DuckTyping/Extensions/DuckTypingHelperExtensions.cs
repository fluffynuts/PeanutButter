using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.DuckTyping.Extensions
{
    internal static class DuckTypingHelperExtensions
    {
        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _propertyCache =
            new Dictionary<Type, Dictionary<string, PropertyInfo>>();
        private static readonly Dictionary<Type, Dictionary<string, MethodInfo>> _methodCache =
            new Dictionary<Type, Dictionary<string, MethodInfo>>();
        internal static Dictionary<string, PropertyInfo> FindProperties(this Type type)
        {
            lock (_propertyCache)
            {
                if (!_propertyCache.ContainsKey(type))
                {
                    _propertyCache[type] = GetPropertiesFor(type);
                }
                return _propertyCache[type];
            }
        }

        private static readonly BindingFlags _seekFlags = 
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;

        private static Dictionary<string, PropertyInfo> GetPropertiesFor(Type type)
        {
            return type.GetProperties(_seekFlags)
                .ToDictionary(
                    pi => pi.Name,
                    pi => pi
                );
        }

        internal static Dictionary<string, MethodInfo> FindMethods(this Type type)
        {
            lock(_methodCache)
            {
                if (!_methodCache.ContainsKey(type))
                {
                    _methodCache[type] = GetMethodsFor(type);
                }
                return _methodCache[type];
            }
        }

        private static Dictionary<string, MethodInfo> GetMethodsFor(Type type)
        {
            return type.GetMethods(_seekFlags)
                .ToDictionary(
                    pi => pi.Name,
                    pi => pi
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

    }
}