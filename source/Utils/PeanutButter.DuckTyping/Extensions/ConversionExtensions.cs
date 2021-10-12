using System;
using Imported.PeanutButter.Utils;
using PeanutButter.DuckTyping.AutoConversion;

namespace PeanutButter.DuckTyping.Extensions
{
    /// <summary>
    /// Provides an extension method on object to attempt to convert
    /// to any other type supported by any existing (or located)
    /// converters. If you need a special conversion to happen
    /// then implement IConverter&lt;T1, T2&gt; to convert between
    /// T1 and T2. Common .net type conversion from strings is already
    /// supported (based on culture-invariant conversions, eg "1.23" -> 1.23M)
    /// </summary>
    public static class ConversionExtensions
    {
        /// <summary>
        /// Attempts to convert the object being operated on
        /// to the required type.
        /// - null always returns false
        /// - attempting to convert to the same type returns the original object
        /// </summary>
        /// <param name="input"></param>
        /// <param name="converted"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryConvertTo<T>(
            this object input,
            out T converted
        )
        {
            var desiredType = typeof(T);
            converted = default;
            if (input is null)
            {
                return desiredType.IsNullableType();
            }
            
            var success = input.TryConvertTo(desiredType, out var convertedObject);
            if (success)
            {
                converted = (T)convertedObject;
            }
            return success;
        }

        /// <summary>
        /// Attempts to convert the object being operated on
        /// to the required type.
        /// - null always returns false
        /// - attempting to convert to the same type returns the original object
        /// </summary>
        /// <param name="input"></param>
        /// <param name="desiredType"></param>
        /// <param name="converted"></param>
        /// <returns></returns>
        public static bool TryConvertTo(
            this object input,
            Type desiredType,
            out object converted
        )
        {
            converted = desiredType.DefaultValue();
            var inputType = input.GetType();
            if (inputType == desiredType)
            {
                converted = input;
                return true;
            }

            var converter = ConverterLocator.GetConverter(
                input.GetType(),
                desiredType
            );

            if (converter is null)
            {
                return false;
            }

            try
            {
                converted = converter.Convert(input);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        
    }
}