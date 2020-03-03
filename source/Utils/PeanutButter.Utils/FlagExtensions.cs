using System;

namespace PeanutButter.Utils
{
    /// <summary>
    /// Provides extensions to help with common enum operations
    /// </summary>
    public static class FlagExtensions
    {
        /// <summary>
        /// Tests if a "flag" enum contains the required flag. Enums which have values
        /// declared as powers of 2 can be or'd together to provide a final value, eg
        /// when using BindingFlags:
        /// var method = typeof(SomeClass).GetMethod("foo", BindingFlags.Public | BindingFlags.Instance);
        /// - in this case, one could do something like:
        /// if (flags.HasFlag(BindingFlags.Public))
        /// {
        ///     // do whatever one does with public members
        /// }
        /// </summary>
        /// <param name="enumValue"></param>
        /// <param name="flag"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool HasFlag<T>(
            this T enumValue,
            T flag) where T: struct
        {
            if (!typeof(T).IsEnum)
            {
                return false; // can't do this comparison
            }

            if (ConvertToInt(enumValue, out var intValue) &&
                ConvertToInt(flag, out var intFlag))
            {
                return intValue.HasFlag(intFlag);
            }
            
            return false;
        }

        private static bool ConvertToInt<T>(T value, out int result)
        {
            try
            {
                result = (int)Convert.ChangeType(value, typeof(int));
                return true;
            }
            catch
            {
                result = 0;
                return false;
            }
        }

        /// <summary>
        /// Provides the integer analogy to testing flags in an enum value,
        /// since the former relies on integer math anyway. This essentially
        /// tests if the flag's on-bits are all on in the value, eg
        /// 6.HasFlag(2); // true
        /// </summary>
        /// <param name="value"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static bool HasFlag(
            this int value,
            int flag)
        {
            return (value & flag) == flag;
        }
    }
}