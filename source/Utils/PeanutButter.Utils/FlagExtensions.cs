using System;
using System.Collections.Generic;
using System.Linq;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.Utils
#else
namespace PeanutButter.Utils
#endif
{
    /// <summary>
    /// Provides extensions to help with common enum operations
    /// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
    internal
#else
    public
#endif
        static class FlagExtensions
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
            T flag
        ) where T : struct
        {
            if (!IsEnum<T>())
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

        /// <summary>
        /// Tests for multiple flags in an int value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="flag1"></param>
        /// <param name="flag2"></param>
        /// <param name="moreFlags"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool HasFlags<T>(
            this T value,
            T flag1,
            T flag2,
            params T[] moreFlags
        )
        {
            if (!ConvertToInt(value, out var intValue))
            {
                return false;
            }

            var ints = new List<int>();
            if (!ConvertToInt(flag1, out var intFlag1))
            {
                return false;
            }

            ints.Add(intFlag1);
            if (!ConvertToInt(flag2, out var intFlag2))
            {
                return false;
            }

            ints.Add(intFlag2);
            foreach (var flag in moreFlags)
            {
                if (!ConvertToInt(flag, out var intFlag))
                {
                    return false;
                }

                ints.Add(intFlag);
            }
            
            return intValue.HasFlags(ints);
        }

        /// <summary>
        /// Tests for multiple flags in an int value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="flag1"></param>
        /// <param name="flag2"></param>
        /// <param name="moreFlags"></param>
        /// <returns></returns>
        public static bool HasFlags(
            this int value,
            int flag1,
            int flag2,
            params int[] moreFlags
        )
        {
            var allFlags = new List<int>(moreFlags)
            {
                flag1,
                flag2
            };
            return value.HasFlags(allFlags);
        }

        /// <summary>
        /// Tests for multiple flags in an int value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static bool HasFlags(
            this int value,
            IEnumerable<int> flags
        )
        {
            foreach (var flag in flags)
            {
                if (!value.HasFlag(flag))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a new value that is the original enum
        /// with the required flag added
        /// </summary>
        /// <param name="enumValue"></param>
        /// <param name="flag"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T WithFlag<T>(
            this T enumValue,
            T flag
        ) where T : struct
        {
            if (!IsEnum<T>())
            {
                return enumValue;
            }

            if (ConvertToInt(enumValue, out var intValue) &&
                ConvertToInt(flag, out var intFlag))
            {
                // unfortunately, we have to sink to boxing ):
                return (T) (object) intValue.WithFlag(intFlag);
            }

            return enumValue;
        }

        /// <summary>
        /// Returns a new value that is the original enum
        /// with the required flag added
        /// </summary>
        /// <param name="value"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static int WithFlag(
            this int value,
            int flag
        )
        {
            return value | flag;
        }

        /// <summary>
        /// Returns a new value that is the original
        /// enum value without the provided flag
        /// </summary>
        /// <param name="enumValue"></param>
        /// <param name="flag"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T WithoutFlag<T>(
            this T enumValue,
            T flag
        ) where T : struct
        {
            if (!IsEnum<T>())
            {
                return enumValue;
            }
            if (ConvertToInt(enumValue, out var intValue) &&
                ConvertToInt(flag, out var intFlag))
            {
                return (T)(object) intValue.WithoutFlag(intFlag);
            }
            return enumValue;
        }

        /// <summary>
        /// Returns a new value which is the original
        /// integer value without the provided integer flag
        /// </summary>
        /// <param name="value"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static int WithoutFlag(
            this int value,
            int flag
        )
        {
            return value & ~flag;
        }

        private static bool ConvertToInt<T>(T value, out int result)
        {
            try
            {
                result = (int) Convert.ChangeType(value, typeof(int));
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
            int flag
        )
        {
            return (value & flag) == flag;
        }

        private static bool IsEnum<T>()
        {
            return typeof(T).IsEnum;
        }
    }
}