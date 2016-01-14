using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Microsoft.AspNet.Identity;

namespace MvcTestHelpers.Fakes
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


        public FakeControllerContext(ControllerBase controller, NameValueCollection formParams, NameValueCollection queryStringParams)
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


        public FakeControllerContext
            (
                ControllerBase controller,
                string userName,
                string[] roles,
                NameValueCollection formParams,
                NameValueCollection queryStringParams,
                HttpCookieCollection cookies,
                SessionStateItemCollection sessionItems
            )
            : base(new FakeHttpContext(CreateClaimsPrincipalFor(userName, roles), formParams, queryStringParams, cookies, sessionItems), new RouteData(), controller)
        {
            controller.ControllerContext = this;
        }

        public static ClaimsPrincipal CreateClaimsPrincipalFor(string userName, string[] roles)
        {
            var claims = CreateClaimsFor(userName ?? "", roles);
            var claimsIdentity = new ClaimsIdentity(claims, 
                DefaultAuthenticationTypes.ApplicationCookie,
                ClaimTypes.Name,
                ClaimTypes.Role);
            return new ClaimsPrincipal(claimsIdentity);

        }

        public static IEnumerable<Claim> CreateClaimsFor(string userName, string[] roles)
        {
            var result = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ExtendedClaimTypes.IdentityProvider, userName),
                new Claim(ClaimTypes.NameIdentifier, userName)
            };
            result.AddRange((roles ?? new string[] {})
                .Where(r => !string.IsNullOrEmpty(r))
                .Select(r => new Claim(ClaimTypes.Role, r)));
            return result;
        }
    }
}