using System;
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

        public static T DuckAs<T>(this object src) where T : class
        {
            return src.DuckAs<T>(false);
        }

        private static readonly MethodInfo _genericFuzzyDuckAsMethod = 
            typeof(DuckTypingObjectExtensions)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(mi => mi.Name == "FuzzyDuckAs" && mi.IsGenericMethod);
        public static object FuzzyDuckAs(this object src, Type toType)
        {
            return NonGenericDuck(src, toType, _genericFuzzyDuckAsMethod);
        }

        private static readonly MethodInfo _genericDuckAsMethod =
            typeof(DuckTypingObjectExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(mi => mi.Name == "DuckAs" && mi.IsGenericMethod);
        public static object DuckAs(this object src, Type toType)
        {
            return NonGenericDuck(src, toType, _genericDuckAsMethod);
        }

        private static object NonGenericDuck(
            object src,
            Type toType,
            MethodInfo genericMethod
        )
        {
            var specific = genericMethod.MakeGenericMethod(toType);
            return specific.Invoke(null, new object[] { src } );
        }

        public static T FuzzyDuckAs<T>(this object src) where T : class
        {
            return src.DuckAs<T>(true);
        }

        public static T ForceFuzzyDuckAs<T>(this Dictionary<string, object> src)
        {
            return ForceDuckAs<T>(src, true);
        }

        public static T ForceDuckAs<T>(this Dictionary<string, object> src)
        {
            return ForceDuckAs<T>(src, false);
        }

        private static T ForceDuckAs<T>(Dictionary<string, object> src, bool allowFuzzy)
        {
            var typeMaker = new TypeMaker();
            var type = allowFuzzy ? typeMaker.MakeTypeImplementing<T>() : typeMaker.MakeFuzzyTypeImplementing<T>();
            return (T)Activator.CreateInstance(type, new object[] { src } );
        } 

        private static bool CanDuckAs<T>(this object src, bool allowFuzzy)
        {
            var asDictionary = src as IDictionary<string, object>;
            if (asDictionary != null)
                return asDictionary.CanDuckDictionaryAs<T>(allowFuzzy);
            var type = typeof(T);
            var srcType = src.GetType();

            return type.CanDuckAs(srcType, allowFuzzy);
        }

        private static bool CanDuckDictionaryAs<T>(
            this IDictionary<string, object> src,
            bool allowFuzzy
        )
        {
            var type = typeof(T);
            return CanDuckDictionaryAs(src, type, allowFuzzy);
        }

        private static bool CanDuckDictionaryAs(
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
            foreach (var prop in properties)
            {
                var targetType = prop.PropertyType;
                object stored;
                if (!src.TryGetValue(prop.Name, out stored))
                {
                    if (prop.PropertyType.IsNullable()) {
                        continue;
                    }
                    return false;
                }
                if (stored == null)
                {
                    if (ShimShamBase.GetDefaultValueFor(targetType) != null)
                        return false;
                    continue;
                }
                var asDictionary = stored as IDictionary<string, object>;
                if (asDictionary != null)
                {
                    if (CanDuckDictionaryAs(asDictionary, targetType, allowFuzzy))
                        continue;
                    return false;
                }


#pragma warning disable S2219 // Runtime type checking should be simplified
                // ReSharper disable once UseMethodIsInstanceOfType
                var srcType = stored.GetType();
                if (!targetType.IsAssignableFrom(srcType))
                {
                    if (allowFuzzy && CanAutoConvert(srcType, targetType)) continue;
#pragma warning restore S2219 // Runtime type checking should be simplified
                    return false;
                }
            }
            return true;
        }

        private static readonly string[] _objectMethodNames =
            typeof(object).GetMethods().Select(m => m.Name).ToArray();

        internal static bool CanDuckAs(
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
            if (toType.IsInterface)
                expectedMethods = expectedMethods.Except(_objectMethodNames);
            var srcMethods = allowFuzzy ? toType.FindFuzzyMethods() : toType.FindMethods();
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

        internal static T DuckAs<T>(this object src, bool allowFuzzy) where T : class
        {
            if (src == null) return null;
            var duckable = allowFuzzy ? src.CanFuzzyDuckAs<T>() : src.CanDuckAs<T>();
            if (!duckable) return null;
            var srcAsDict = src as IDictionary<string, object>;
            if (allowFuzzy && srcAsDict != null)
                src = srcAsDict.ToCaseInsensitiveDictionary();

            var duckType = FindOrCreateDuckTypeFor<T>(allowFuzzy);
            return (T)Activator.CreateInstance(duckType, src);

        }

        private static readonly Dictionary<Type, TypePair> _duckTypes
            = new Dictionary<Type, TypePair>();

        private static Type FindOrCreateDuckTypeFor<T>(bool isFuzzy)
        {
            var key = typeof(T);
            return FindOrCreateDuckTypeFor<T>(key, isFuzzy);
        }

        private static Type FindOrCreateDuckTypeFor<T>(Type key, bool isFuzzy)
        {
            lock (_duckTypes)
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
