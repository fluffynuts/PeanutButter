using System.Web;
using System.Web.Routing;

namespace PeanutButter.TestUtils.MVC.Builders
{
    public class RequestContextBuilder 
        : GenericBuilderWithFieldAccess<RequestContextBuilder, RequestContext>
    {
        private HttpContextBase _httpContext;
        private RouteData _routeData;

        public override RequestContext ConstructEntity()
        {
            return new RequestContext(
                _httpContext ?? FakeHttpContextBuilder.BuildDefault(),
                _routeData ?? RouteDataBuilder.BuildDefault()
            );
        }

        public RequestContextBuilder WithHttpContext(HttpContextBase context)
        {
            return WithField(b => b._httpContext = context);
        }

        public RequestContextBuilder WithRouteData(RouteData routeData)
        {
            return WithField(b => b._routeData = routeData);
        }
    }
}
