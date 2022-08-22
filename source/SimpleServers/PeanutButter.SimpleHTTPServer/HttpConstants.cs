namespace PeanutButter.SimpleHTTPServer
{
    /// <summary>
    /// Provides some constants which are useful in an http context
    /// </summary>
    public static class HttpConstants
    {
        /// <summary>
        /// Common HTTP headers
        /// </summary>
        public static class Headers
        {
            /// <summary>
            /// Content-Length
            /// </summary>
            public const string CONTENT_LENGTH = "Content-Length";
            /// <summary>
            /// Content-Type
            /// </summary>
            public const string CONTENT_TYPE = "Content-Type";
        }

        /// <summary>
        /// Common HTTP Methods
        /// </summary>
        public static class Methods
        {
            /// <summary>
            /// GET
            /// </summary>
            public const string GET = "GET";
            /// <summary>
            /// POST
            /// </summary>
            public const string POST = "POST";

            /// <summary>
            /// PUT method.
            /// </summary>
            public const string PUT = "PUT";

            /// <summary>
            /// UPDATE method.
            /// </summary>
            public const string UPDATE = "UPDATE";

            /// <summary>
            /// DELETE method.
            /// </summary>
            public const string DELETE = "DELETE";

            /// <summary>
            /// PATCH method.
            /// </summary>
            public const string PATCH = "PATCH";
        }

        /// <summary>
        /// Common MIME types
        /// </summary>
        public static class MimeTypes
        {
            /// <summary>
            /// HTML
            /// </summary>
            public const string HTML = "text/html";
            /// <summary>
            /// JSON
            /// </summary>
            public const string JSON = "application/json";
            /// <summary>
            /// Bytes
            /// </summary>
            public const string BYTES = "application/octet-stream";
            /// <summary>
            /// XML
            /// </summary>
            public const string XML = "text/xml";
        }

        /// <summary>
        /// Common HTTP status titles
        /// </summary>
        public static class Statuses
        {
            /// <summary>
            /// File not found
            /// </summary>
            public const string NOTFOUND = "File not found";
            /// <summary>
            /// Internal error
            /// </summary>
            public const string INTERNALERROR = "An internal error occurred";
        }

        /// <summary>
        /// Establishes the default maximum size for posts to an HTTPServer
        /// </summary>
        public const int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB
    }
}