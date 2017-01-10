using System.Collections.Specialized;
using System.Web;

namespace PeanutButter.TestUtils.MVC.Builders
{
    /// <summary>
    /// Original code: http://stephenwalther.com/archive/2008/07/01/asp-net-mvc-tip-12-faking-the-controller-context
    /// </summary>
    public class FakeHttpRequest : HttpRequestBase
    {
        private readonly NameValueCollection _formParams;
        private readonly NameValueCollection _queryStringParams;
        private readonly HttpCookieCollection _cookies;

        public FakeHttpRequest(NameValueCollection formParams, NameValueCollection queryStringParams, HttpCookieCollection cookies)
        {
            _formParams = formParams;
            _queryStringParams = queryStringParams;
            _cookies = cookies;
        }

        public override NameValueCollection Form => _formParams;

        public override NameValueCollection QueryString => _queryStringParams;

        public override HttpCookieCollection Cookies => _cookies;
    }
}
