using System;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web;

namespace PeanutButter.TestUtils.MVC
{
    public class FakeHttpRequest : HttpRequestBase
    {
        public override NameValueCollection Form { get; }
        public override NameValueCollection QueryString { get; }
        public override HttpCookieCollection Cookies { get; }
        public override NameValueCollection Headers { get; }
        public override Uri Url { get; }
        public override string ContentType { get; set; }
        public override Stream InputStream { get; }
        public override string FilePath { get; }
        public override string HttpMethod { get; }
        public override string ApplicationPath { get; }
        public override string Path { get; }
        public override string RawUrl { get; }

        public FakeHttpRequest(
            NameValueCollection formParams,
            NameValueCollection queryStringParams,
            HttpCookieCollection cookies,
            NameValueCollection headers,
            string url,
            string contentType = "text/html",
            string contents = "",
            string method = "GET",
            string applicationPath = "/",
            string path = "/")
        {
            Form = formParams;
            QueryString = queryStringParams;
            Cookies = cookies;
            Headers = headers;
            Url = new Uri(url);
            ContentType = contentType;
            InputStream = new MemoryStream(Encoding.UTF8.GetBytes(contents));
            FilePath = Url.AbsolutePath;
            HttpMethod = method;
            ApplicationPath = applicationPath;
            Path = path;
            RawUrl = Url.PathAndQuery;
        }


        public override string this[String key] => QueryString[key] ?? Form[key];
    }
}
