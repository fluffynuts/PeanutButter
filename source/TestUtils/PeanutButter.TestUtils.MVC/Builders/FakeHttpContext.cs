using System.Collections.Specialized;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.SessionState;

namespace PeanutButter.TestUtils.MVC.Builders
{
    /// <summary>
    /// Original code: http://stephenwalther.com/archive/2008/07/01/asp-net-mvc-tip-12-faking-the-controller-context
    /// </summary>
    public class FakeHttpContext : HttpContextBase
    {
        private readonly IPrincipal _principal;
        private readonly NameValueCollection _formParams;
        private readonly NameValueCollection _queryStringParams;
        private readonly HttpCookieCollection _cookies;
        private readonly SessionStateItemCollection _sessionItems;
        private HttpRequestBase _request;
        private HttpSessionStateBase _session;

        public FakeHttpContext(IPrincipal principal, 
                                NameValueCollection formParams, 
                                NameValueCollection queryStringParams, 
                                HttpCookieCollection cookies, 
                                SessionStateItemCollection sessionItems )
        {
            _principal = principal;
            _formParams = formParams;
            _queryStringParams = queryStringParams;
            _cookies = cookies;
            _sessionItems = sessionItems;
        }

        public override HttpRequestBase Request
        {
            get
            {
                return _request ?? (_request = new FakeHttpRequest(_formParams, _queryStringParams, _cookies));
            }
        }

        public override IPrincipal User
        {
            get
            {
                return _principal;
            }
        }

        public override HttpSessionStateBase Session
        {
            get
            {
                return _session ?? (_session = new FakeHttpSessionState(_sessionItems));
            }
        }

    }


}
