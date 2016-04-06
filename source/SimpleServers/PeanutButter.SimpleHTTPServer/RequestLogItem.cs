using System.Net;

namespace PeanutButter.SimpleHTTPServer
{
    public class RequestLogItem
    {
        public string Path { get; }
        public HttpStatusCode StatusCode { get; }
        public string Message { get; }
        public string Method { get; }

        public RequestLogItem(string path, HttpStatusCode code, string method, string message)
        {
            Path = path;
            StatusCode = code;
            Method = method;
            Message = message ?? StatusCode.ToString();
        }
    }
}