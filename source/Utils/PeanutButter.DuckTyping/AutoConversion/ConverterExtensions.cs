using System.Linq;

#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
namespace Imported.PeanutButter.DuckTyping.AutoConversion
#else
namespace PeanutButter.DuckTyping.AutoConversion
#endif
{
    /// <summary>
    /// Provides "generic" object-in, object-out conversion extension
    /// method `Convert(other)` for IConverter implementations
    /// </summary>
#if BUILD_PEANUTBUTTER_DUCKTYPING_INTERNAL
    internal
#else
    public
#endif
        static class ConverterExtensions
    {
        /// <summary>
        /// Convert the provided object to the converter's other type,
        /// as an object.
        /// </summary>
        /// <param name="converter"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static object Convert(this IConverter converter, object other)
        {
            if (other == null)
            {
                return null;
            }

            var otherType = other.GetType();
            var methodInfo = converter.GetType().GetMethods()
                .FirstOrDefault(mi =>
                {
                    if (mi.Name != "Convert")
                    {
                        return false;
                    }

                    var parameters = mi.GetParameters();
                    return parameters.Length == 1 &&
                        parameters[0].ParameterType == otherType;
                });
            return methodInfo?.Invoke(converter, new object[] { other });
        }
    }
}