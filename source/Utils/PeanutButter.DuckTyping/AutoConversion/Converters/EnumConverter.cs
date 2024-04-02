using System;
using System.Linq;
using System.Reflection;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.AutoConversion.Converters
#else
namespace PeanutButter.DuckTyping.AutoConversion.Converters
#endif
{
    internal static class EnumConverter
    {
        public static bool TryConvert(
            Type fromType,
            Type toType,
            object sourceValue,
            out object result
        )
        {
            result = null;
            var enumType = Choose(fromType, toType, t => t.IsEnum);
            var stringType = Choose(fromType, toType, t => t == typeof(string));
            if (enumType == null || stringType == null)
                return false;
            if (sourceValue.GetType() != typeof(string))
            {
                // ReSharper disable once ConstantConditionalAccessQualifier
                result = sourceValue?.ToString();
                return true;
            }

            var method = GenericTryParse.MakeGenericMethod(enumType)
                ?? throw new InvalidOperationException(
                    $"Unable to create generic method TryParse for {enumType}"
                );
            var args = new[]
            {
                sourceValue,
                true,
                null
            };
            var parsed = (bool)method.Invoke(null, args);
            if (parsed)
                result = args[2];
            return parsed;
        }

        public static bool CanPerhapsConvertBetween(
            Type left,
            Type right
        )
        {
            var enumType = Choose(left, right, t => t.IsEnum);
            if (enumType == null)
                return false;
            var stringType = Choose(left, right, t => t == typeof(string));
            return stringType != null;
        }

        private static readonly MethodInfo GenericTryParse
            = typeof(Enum).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(mi => mi.Name == nameof(Enum.TryParse) && mi.HasExpectedParametersForCaseInsensitiveTryParse());

        private static T Choose<T>(T left, T right, Func<T, bool> test)
        {
            return test(left)
                ? left
                : (test(right)
                    ? right
                    : default(T));
        }
    }

    internal static class MethodInfoExtensions
    {
        public static bool HasExpectedParametersForCaseInsensitiveTryParse(
            this MethodInfo methodInfo
        )
        {
            var args = methodInfo.GetParameters();
            return args.Length == 3 &&
                args[0].ParameterType == typeof(string) &&
                args[1].ParameterType == typeof(bool) &&
                args[2].IsOut;
        }
    }
}