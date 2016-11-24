using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PeanutButter.DuckTyping.AutoConversion;

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.DuckTyping.Extensions
{
    public class UnDuckableException : Exception
    {
        private IEnumerable<string> _errors;
        public IEnumerable<string> Errors => _errors;

        public UnDuckableException(IEnumerable<string> errors)
            : base("Unable to perform duck operation. Examine errors for more information")
        {
            _errors = errors;
        }
    }

    public static class DuckTypingObjectExtensions
    {
        public static bool CanDuckAs<T>(this object src)
        {
            return PrivateCanDuckAs<T>(src, false, false);
        }

        public static bool CanFuzzyDuckAs<T>(this object src)
        {
            return PrivateCanDuckAs<T>(src, true, false);
        }

        public static T DuckAs<T>(this object src) where T : class
        {
            return src.DuckAs<T>(false);
        }

        public static T DuckAs<T>(this object src, bool throwOnError) where T : class
        {
            return src.InternalDuckAs<T>(false, throwOnError);
        }

        private static readonly MethodInfo _genericFuzzyDuckAsMethod =
            FindGenericDuckMethod("FuzzyDuckAs");

        private static readonly MethodInfo _genericDuckAsMethod =
            FindGenericDuckMethod("DuckAs");

        private static MethodInfo FindGenericDuckMethod(string name)
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

        public static object FuzzyDuckAs(this object src, Type toType)
        {
            return NonGenericDuck(src, toType, false, _genericFuzzyDuckAsMethod);
        }

        public static object FuzzyDuckAs(this object src, Type toType, bool throwOnError)
        {
            return UnwrapTargetInvokationFor(
                () => NonGenericDuck(src, toType, throwOnError, _genericFuzzyDuckAsMethod)
            );
        }

        public static object DuckAs(this object src, Type toType)
        {
            return NonGenericDuck(src, toType, false, _genericDuckAsMethod);
        }

        public static object DuckAs(this object src, Type toType, bool throwOnError)
        {
            return UnwrapTargetInvokationFor(
                () => NonGenericDuck(src, toType, throwOnError, _genericDuckAsMethod)
            );
        }

        private static object UnwrapTargetInvokationFor(Func<object> toRun)
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

        private static object NonGenericDuck(
            object src,
            Type toType,
            bool throwOnError,
            MethodInfo genericMethod
        )
        {
            var specific = genericMethod.MakeGenericMethod(toType);
            return specific.Invoke(null, new object[] {src, throwOnError});
        }

        public static T FuzzyDuckAs<T>(this object src) where T : class
        {
            return src.FuzzyDuckAs<T>(false);
        }

        public static T FuzzyDuckAs<T>(this object src, bool throwOnError) where T : class
        {
            return src.InternalDuckAs<T>(true, throwOnError);
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
            return (T) Activator.CreateInstance(type, new object[] {src});
        }

        private static bool PrivateCanDuckAs<T>(this object src, bool allowFuzzy, bool throwOnError)
        {
            var asDictionary = src as IDictionary<string, object>;
            if (asDictionary != null)
                return asDictionary.CanDuckDictionaryAs<T>(allowFuzzy);
            var type = typeof(T);
            var srcType = src.GetType();

            return type.InternalCanDuckAs(srcType, allowFuzzy, throwOnError);
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
            var errors = DictionaryDuckErrorsFor(src, type, allowFuzzy);
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

        private static void FindDictionaryDuckErrorFor(
            PropertyInfo prop,
            IDictionary<string, object> src,
            bool allowFuzzy,
            List<string> errors
        )
        {
            var targetType = prop.PropertyType;
            object stored;
            if (!src.TryGetValue(prop.Name, out stored))
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
                        $"Stored value for {prop.Name} is null but target type {targetType.Name} does not allow nulls");
                return;
            }
            var asDictionary = stored as IDictionary<string, object>;
            if (asDictionary != null)
            {
                if (!CanDuckDictionaryAs(asDictionary, targetType, allowFuzzy))
                    errors.Add($"Property {prop.Name} is not a dictionary and can't be ducked to {targetType.Name}");
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

        private static readonly string[] _objectMethodNames =
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
                expectedMethods = expectedMethods.Except(_objectMethodNames);
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
                errors.Add($"Missing target {Prop(missing.Count())} {NamesOf(missing)}");
                return;
            }
            var accessMismatches = mismatches.Where(kvp =>
                    !expectedPrimitives[kvp.Key].IsNoMoreRestrictiveThan(srcPrimitives[kvp.Key]))
                    .ToArray();
            if (accessMismatches.Any())
            {
                errors.Add($"Mismatched target accessors for {MakeTargetAccessorMessageFor(accessMismatches, srcPrimitives)}");
                return;
            }
            var typeMismatchError =
                $"Type mismatch for {Prop(mismatches.Count())}: {GetTypeMismatchErrorsFor(mismatches, expectedPrimitives)}";
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
            return String.Join(", ", parts);
        }

        private static string MakeTargetAccessorMessageFor(
            IEnumerable<KeyValuePair<string, PropertyInfo>> accessMismatches, 
            Dictionary<string, PropertyInfo> expectedPrimitives)
        {
            var legends = accessMismatches.Select(kvp => $"{kvp.Key} {GetSetFor(expectedPrimitives[kvp.Key])} -> {GetSetFor(kvp.Value)}");
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
            var srcAsDict = src as IDictionary<string, object>;
            if (allowFuzzy && srcAsDict != null)
                src = srcAsDict.ToCaseInsensitiveDictionary();

            var duckType = FindOrCreateDuckTypeFor<T>(allowFuzzy);
            return (T) Activator.CreateInstance(duckType, src);
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