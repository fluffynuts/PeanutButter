using System.Linq;
using System.Reflection;
using PeanutButter.DuckTyping.Extensions;

namespace PeanutButter.DuckTyping.AutoConversion.Converters
{
    internal abstract class GenericStringConverterBase<T>: ConverterBase
    {
        protected readonly MethodInfo _tryParse = GetTryParseMethod();

        private static MethodInfo GetTryParseMethod()
        {
            return typeof(T)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(mi => DuckTypingHelperExtensions.IsTryParseMethod(mi));
        }
    }
}