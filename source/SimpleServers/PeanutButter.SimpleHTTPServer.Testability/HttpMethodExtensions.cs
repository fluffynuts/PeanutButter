namespace PeanutButter.SimpleHTTPServer.Testability
{
    public static class HttpMethodExtensions
    {
        public static bool Matches(this HttpMethods method, string otherMethod)
        {
            return method == HttpMethods.Any ||
                   method.ToString().ToLowerInvariant() == (otherMethod ?? "").ToLowerInvariant();
        }
    }
}