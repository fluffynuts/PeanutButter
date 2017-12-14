using System.Collections.Generic;
using System.Net;

namespace PeanutButter.SimpleHTTPServer
{
    /// <summary>
    /// Log item for requests
    /// </summary>
    public class RequestLogItem
    {
        /// <summary>
        /// Path of the request
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Status code (result) for the request
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Message provided for the request
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// HTTP method (verb) for the request
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Headers provided with the request
        /// </summary>
        public Dictionary<string, string> Headers { get; }

        /// <inheritdoc />
        public RequestLogItem(
            string path,
            HttpStatusCode code,
            string method,
            string message,
            Dictionary<string, string> headers
        )
        {
            Path = path;
            StatusCode = code;
            Method = method;
            Message = message ?? StatusCode.ToString();
            Headers = headers ?? new Dictionary<string, string>();
        }
    }
}