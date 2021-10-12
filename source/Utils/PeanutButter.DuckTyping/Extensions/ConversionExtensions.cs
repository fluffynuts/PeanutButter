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
        /// <param name="output"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryConvertTo<T>(
            this object input,
            out T output
        )
        {
            var desiredType = typeof(T);
            output = default;
            if (input is null)
            {
                return desiredType.IsNullableType();
            }
            
            var inputType = input.GetType();
            if (inputType == desiredType)
            {
                output = (T)input;
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
                output = (T)converter.Convert(input);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}