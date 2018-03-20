using System.Globalization;

namespace PeanutButter.SimpleHTTPServer
{
    /// <summary>
    /// Provides extension methods for HttpMethods enum values
    /// </summary>
    public static class HttpMethodsExtensions
    {
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        /// <summary>
        /// Determines whether the HttpMethod being acted on matches
        /// the provided method.
        /// </summary>
        /// <param name="method">Method to act upon</param>
        /// <param name="otherMethod">Method to test against</param>
        /// <returns>True if the methods are an exact match or otherMethod is Any</returns>
        public static bool Matches(this HttpMethods method, HttpMethods otherMethod)
        {
            return method == HttpMethods.Any ||
                   otherMethod == HttpMethods.Any ||
                   method == otherMethod;
        }

        /// <summary>
        /// Determines whether the HttpMethod being acted on matches
        /// the provided method.
        /// </summary>
        /// <param name="method">Method to act upon</param>
        /// <param name="otherMethod">Method to test against</param>
        /// <returns>True if the methods are an exact match or otherMethod is Any</returns>
        public static bool Matches(this HttpMethods method, string otherMethod)
        {
            return method == HttpMethods.Any ||
                   method.ToString().ToUpper(Culture) == (otherMethod ?? string.Empty).ToUpper(Culture);
        }
    }
}