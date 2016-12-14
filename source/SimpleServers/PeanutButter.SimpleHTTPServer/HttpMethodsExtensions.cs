using System.Globalization;

namespace PeanutButter.SimpleHTTPServer
{
    public static class HttpMethodsExtensions
    {
        private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;

        public static bool Matches(this HttpMethods method, HttpMethods otherMethod)
        {
            return method == HttpMethods.Any || 
                    otherMethod == HttpMethods.Any ||
                    method == otherMethod;
        }

        public static bool Matches(this HttpMethods method, string otherMethod)
        {
            return method == HttpMethods.Any ||
                    method.ToString().ToUpper(_culture) == (otherMethod ?? string.Empty).ToUpper(_culture);
        }
    }
}