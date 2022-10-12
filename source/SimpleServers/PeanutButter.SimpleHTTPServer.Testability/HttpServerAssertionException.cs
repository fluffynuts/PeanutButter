using System;

namespace PeanutButter.SimpleHTTPServer.Testability
{
    public class HttpServerAssertionException : Exception
    {
        public HttpServerAssertionException(string message) : base(message)
        {
        }
    }
}