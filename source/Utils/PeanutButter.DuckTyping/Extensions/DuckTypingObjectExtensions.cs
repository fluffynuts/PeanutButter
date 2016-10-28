using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PeanutButter.DuckTyping.AutoConversion;

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.DuckTyping.Extensions
{
    internal class TypePair
    {
        public Type Type { get; set; }
        public Type FuzzyType { get; set; }
    }
    public static class DuckTypingObjectExtensions
    {
        public static bool CanDuckAs<T>(this object src)
        {
            return CanDuckAs<T>(src, false);
        }

        public static bool CanFuzzyDuckAs<T>(this object src)
        {
            return CanDuckAs<T>(src, true);
        }

        public static T DuckAs<T>(this object src) where T: class
        {
            return src.DuckAs<T>(false);
        }

        public static T FuzzyDuckAs<T>(this object src) where T: class
        {
            return src.DuckAs<T>(true);
        }

        private static bool CanDuckAs<T>(this object src, bool allowFuzzy)
        {
            var type = typeof(T);
            var srcType = src.GetType();

            return type.CanDuckAs(srcType, allowFuzzy);
        }

        private static readonly string[] _objectMethodNames = 
            typeof(object).GetMethods().Select(m => m.Name).ToArray();

        internal static bool CanDuckAs(
            this Type type,
            Type srcType,
            bool allowFuzzy
        )
        {
            var expectedProperties = allowFuzzy ? type.FindFuzzyProperties() :type.FindProperties();
            var expectedPrimitives = expectedProperties.GetPrimitiveProperties(allowFuzzy);
            var srcProperties = allowFuzzy ? srcType.FindFuzzyProperties() : srcType.FindProperties();
            var srcPrimitives = srcProperties.GetPrimitiveProperties(allowFuzzy);

            var mismatches = srcPrimitives.FindPrimitivePropertyMismatches(expectedPrimitives, allowFuzzy);
            if (mismatches.Any())
            {
                var missing = mismatches
                                .Where(kvp => !srcPrimitives.ContainsKey(kvp.Key));
                if (missing.Any())
                    return false;
                var accessMismatches = mismatches.Where(kvp => 
                    !expectedPrimitives[kvp.Key].IsNoMoreRestrictiveThan(srcPrimitives[kvp.Key]));
                if (accessMismatches.Any())
                    return false;
                if (!allowFuzzy)
                    return false;
                if (!HaveConvertersFor(mismatches, expectedPrimitives))
                    return false;
            }

            var expectedMethods = allowFuzzy ? type.FindFuzzyMethods() : type.FindMethods();
            if (srcType.IsInterface)
                expectedMethods = expectedMethods.Except(_objectMethodNames);
            var srcMethods = allowFuzzy ? srcType.FindFuzzyMethods() : srcType.FindMethods();
            return srcMethods.IsSuperSetOf(expectedMethods);
        }

        private static bool HaveConvertersFor(
            Dictionary<string, PropertyInfo> mismatches, 
            Dictionary<string, PropertyInfo> expectedPrimitives)
        {
            foreach (var kvp in mismatches)
            {
                var srcType = kvp.Value.PropertyType;
                var targetType = expectedPrimitives[kvp.Key].PropertyType;
                if (ConverterLocator.GetConverter(srcType, targetType) == null)
                    return false;
            }
            return true;
        }

        private static Dictionary<string, MethodInfo> Except(
            this Dictionary<string, MethodInfo> src,
            IEnumerable<string> others 
        )
        {
            return src.Where(kvp => !others.Contains(kvp.Key))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        internal static T DuckAs<T>(this object src, bool allowFuzzy) where T: class
        {
            if (src == null) return null;
            var duckable = allowFuzzy ? src.CanFuzzyDuckAs<T>() : src.CanDuckAs<T>();
            if (!duckable) return null;

            var duckType = FindOrCreateDuckTypeFor<T>(allowFuzzy);
            return (T)Activator.CreateInstance(duckType, src);
            
        }

        private static readonly Dictionary<Type, TypePair> _duckTypes 
            = new Dictionary<Type, TypePair>();

        private static Type FindOrCreateDuckTypeFor<T>(bool isFuzzy)
        {
            var key = typeof(T);
            lock(_duckTypes)
            {
                if (!_duckTypes.ContainsKey(key))
                {
                    _duckTypes[key] = new TypePair();
                }
                var match = _duckTypes[key];
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
