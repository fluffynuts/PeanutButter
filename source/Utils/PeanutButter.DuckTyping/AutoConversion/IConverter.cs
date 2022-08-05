using System;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.AutoConversion
#else
namespace PeanutButter.DuckTyping.AutoConversion
#endif
{
    /// <summary>
    /// Implement this interface for two type to provide
    /// auto-discovered converters to be used when fuzzy-duck-typing
    /// </summary>
    /// <typeparam name="T1">Type to convert from or to</typeparam>
    /// <typeparam name="T2">Type to convert to or from</typeparam>
#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
    internal
#else
    public
#endif
        interface IConverter<T1, T2> : IConverter
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
#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
    internal
#else
    public
#endif
    interface IConverter
    {
        /// <summary>
        /// Should return true when this converter can convert between t1 and t2
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        bool CanConvert(Type t1, Type t2);
    }
}