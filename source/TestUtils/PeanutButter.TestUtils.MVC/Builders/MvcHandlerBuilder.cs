using System.Web.Mvc;
using System.Web.Routing;

namespace PeanutButter.TestUtils.MVC.Builders
{
    public class MvcHandlerBuilder
        : GenericBuilderWithFieldAccess<MvcHandlerBuilder, MvcHandler>
    {
        private RequestContext _requestContext;

        public override MvcHandler ConstructEntity()
        {
            return new MvcHandler(_requestContext);
        }

        public MvcHandlerBuilder()
        {
            WithContext(new RequestContext());
        }

        public MvcHandlerBuilder WithContext(RequestContext requestContext)
        {
            return WithField(b => b._requestContext = requestContext);
        }
    }
}
