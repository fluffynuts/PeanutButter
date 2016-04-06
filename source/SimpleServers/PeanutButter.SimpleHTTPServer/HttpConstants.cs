namespace PeanutButter.SimpleHTTPServer
{
    public static class HttpConstants
    {
        public const string CONTENT_LENGTH_HEADER = "Content-Length";
        public const string CONTENT_TYPE_HEADER = "Content-Type";
        public const string METHOD_GET = "GET";
        public const string METHOD_POST = "POST";
        public const string MIMETYPE_HTML = "text/html";
        public const string MIMETYPE_JSON = "application/json";
        public const string MIMETYPE_BYTES = "application/octet-stream";
        public const int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB
        public const string HTTP_STATUS_NOTFOUND = "File not found";
        public const string HTTP_STATUS_INTERNALERROR = "An internal error occurred";
    }
}