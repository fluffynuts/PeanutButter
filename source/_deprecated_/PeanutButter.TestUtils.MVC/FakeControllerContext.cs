using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using PeanutButter.RandomGenerators;

namespace PeanutButter.TestUtils.MVC
{
    public class FakeControllerContext : ControllerContext
    {
        public FakeControllerContext(ControllerBase controller)
            : this(controller, null, null, null, null, null, null)
        {
        }

        public FakeControllerContext(ControllerBase controller, HttpCookieCollection cookies)
            : this(controller, null, null, null, null, cookies, null)
        {
        }

        public FakeControllerContext(ControllerBase controller, SessionStateItemCollection sessionItems)
            : this(controller, null, null, null, null, null, sessionItems)
        {
        }


        public FakeControllerContext(ControllerBase controller, NameValueCollection formParams)
            : this(controller, null, null, formParams, null, null, null)
        {
        }


        public FakeControllerContext(
            ControllerBase controller,
            NameValueCollection formParams,
            NameValueCollection queryStringParams)
            : this(controller, null, null, formParams, queryStringParams, null, null)
        {
        }


        public FakeControllerContext(ControllerBase controller, string userName)
            : this(controller, userName, null, null, null, null, null)
        {
        }


        public FakeControllerContext(ControllerBase controller, string userName, string[] roles)
            : this(controller, userName, roles, null, null, null, null)
        {
        }


        public FakeControllerContext(
            ControllerBase controller,
            string userName,
            string[] roles,
            NameValueCollection formParams,
            NameValueCollection queryStringParams,
            HttpCookieCollection cookies,
            SessionStateItemCollection sessionItems
        ) : this(
            controller,
            userName,
            roles,
            formParams,
            queryStringParams,
            cookies,
            sessionItems,
            RandomValueGen.GetRandomHttpUrl()
        )
        {
        }

        public FakeControllerContext(
            ControllerBase controller,
            string userName,
            string[] roles,
            NameValueCollection formParams,
            NameValueCollection queryStringParams,
            HttpCookieCollection cookies,
            SessionStateItemCollection sessionItems,
            string url
        ) : this(
            controller,
            userName,
            roles,
            formParams,
            queryStringParams,
            cookies,
            sessionItems,
            new NameValueCollection(),
            url)
        {
        }

        public FakeControllerContext(
            ControllerBase controller,
            string userName,
            string[] roles,
            NameValueCollection formParams,
            NameValueCollection queryStringParams,
            HttpCookieCollection cookies,
            SessionStateItemCollection sessionItems,
            NameValueCollection requestHeaders,
            string url
        ) : base(
            new FakeHttpContext(
                new FakePrincipal(
                    new FakeIdentity(userName),
                    roles
                ),
                formParams,
                queryStringParams,
                cookies,
                sessionItems,
                requestHeaders,
                url
            ),
            new RouteData(),
            controller)
        {
        }

    }

    public static class HttpSessionStateExtensions
    {
        public static SessionStateItemCollection AsSessionStateItemCollection(
            this HttpSessionStateBase state
        )
        {
            var result = new SessionStateItemCollection();
            // TODO: actually fill in the session
            return result;
        }
    }
}
