using System.Net;

namespace PeanutButter.SimpleHTTPServer
{
    public class RequestLogItem
    {
        public string Path { get; }
        public HttpStatusCode StatusCode { get; }
        public string Message { get; }
        public RequestLogItem(string path, HttpStatusCode code, string message)
        {
            Path = path;
            StatusCode = code;
            Message = message ?? StatusCode.ToString();
        }
    }
}