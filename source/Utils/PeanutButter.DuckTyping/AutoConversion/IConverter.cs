using System;
using System.Linq;

namespace PeanutButter.DuckTyping.AutoConversion
{
    /// <summary>
    /// Implement this interface for two type to provide
    /// auto-discovered converters to be used when fuzzy-duck-typing
    /// </summary>
    /// <typeparam name="T1">Type to convert from or to</typeparam>
    /// <typeparam name="T2">Type to convert to or from</typeparam>
    public interface IConverter<T1, T2> : IConverter
    {
        /// <summary>
        /// Convert an object of type T2 to T1
        /// </summary>
        /// <param name="input">Value to convert</param>
        /// <returns>Converted value, as implemented by the converter</returns>
        T1 Convert(T2 input);

        /// <summary>
        /// Convert an object of type T1 to T2
        /// </summary>
        /// <param name="input">Value to convert</param>
        /// <returns>Converted value, as implemented by the converter</returns>
        T2 Convert(T1 input);
    }

    /// <summary>
    /// Base interface for converters of all types
    /// </summary>
    public interface IConverter
    {
        /// <summary>
        /// One of the types converted from / to
        /// This property provides a quick lookup when seeking converters
        /// </summary>
        Type T1 { get; }

        /// <summary>
        /// The other type converted from / to
        /// This property provides a quick lookup when seeking converters
        /// </summary>
        Type T2 { get; }
    }
}

