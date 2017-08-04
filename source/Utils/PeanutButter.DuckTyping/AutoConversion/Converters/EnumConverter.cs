using System;
using System.Linq;
using System.Reflection;

namespace PeanutButter.DuckTyping.AutoConversion.Converters
{
    internal static class EnumConverter
    {
        public static bool TryConvert(
            Type fromType,
            Type toType,
            object sourceValue,
            out object result)
        {
            result = null;
            var enumType = Check(fromType, toType, t => t.IsEnum);
            var stringType = Check(fromType, toType, t => t == typeof(string));
            if (enumType == null || stringType == null)
                return false;
            if (sourceValue.GetType() != typeof(string))
            {
                result = sourceValue?.ToString();
                return true;
            }
            var method = _genericTryParse.MakeGenericMethod(enumType);
            var args = new object[] {sourceValue, true, null};
            var parsed = (bool) method.Invoke(null, args);
            if (parsed)
                result = args[2];
            return parsed;
        }

        private static readonly MethodInfo _genericTryParse
            = typeof(Enum).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .First(mi => mi.Name == nameof(Enum.TryParse) && mi.HasExpectedParametersForCaseInsensitiveTryParse());

        private static T Check<T>(T left, T right, Func<T, bool> test)
        {
            return test(left) ? left : (test(right) ? right : default(T));
        }
    }

    internal static class MethodInfoExtensions
    {
        public static bool HasExpectedParametersForCaseInsensitiveTryParse(this MethodInfo methodInfo)
        {
            var args = methodInfo.GetParameters();
            return args.Length == 3 &&
                   args[0].ParameterType == typeof(string) &&
                   args[1].ParameterType == typeof(bool) &&
                   args[2].IsOut;
        }
    }
}