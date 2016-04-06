namespace PeanutButter.SimpleHTTPServer
{
    public static class HttpMethodsExtensions
    {
        public static bool Matches(this HttpMethods method, HttpMethods otherMethod)
        {
            return method == HttpMethods.Any || method == otherMethod;
        }

        public static bool Matches(this HttpMethods method, string otherMethod)
        {
            return method == HttpMethods.Any || method.ToString().ToUpper() == (otherMethod ?? "").ToUpper();
        }
    }
}