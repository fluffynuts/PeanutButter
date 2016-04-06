using System.Net;

namespace PeanutButter.SimpleHTTPServer
{
    public class RequestLogItem
    {
        public string Path { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public string Message { get; private set; }
        public RequestLogItem(string path, HttpStatusCode code, string message)
        {
            Path = path;
            StatusCode = code;
            Message = message ?? StatusCode.ToString();
        }
    }
}