using System.Collections.Specialized;
using System.Web;
using NSubstitute;

namespace PeanutButter.TestUtils.MVC
{
    public class FakeHttpResponse : HttpResponseBase
    {
        public override NameValueCollection Headers { get; }
        public override HttpCookieCollection Cookies { get; }
        public override int StatusCode { get; set; }
        public override string Status { get; set; }
        public override bool Buffer { get; set; }
        public override string Charset { get; set; }

        public FakeHttpResponse()
        {
            Headers = new NameValueCollection();
            Cookies = new HttpCookieCollection();
            Cache = Substitute.For<HttpCachePolicyBase>();
        }

        public override void Clear()
        {
        }

        public override bool TrySkipIisCustomErrors { get; set; }

        /// <summary>
        /// Used to rewrite urls to include the session id when in cookieless mode
        /// (which we don't care about)
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public override string ApplyAppPathModifier(string virtualPath)
        {
            return virtualPath;
        }

        public override HttpCachePolicyBase Cache { get; }

        public override void AddHeader(string name, string value)
        {
            Headers[name] = value;
        }
    }
}
