using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using NSubstitute;
using PeanutButter.RandomGenerators;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.MVC.Builders
{
    public class FakeRoute : RouteBase
    {
        private readonly RouteData _routeData;
        private readonly VirtualPathData _virtualPathData;

        public FakeRoute(
            RouteData routeData,
            VirtualPathData virtualPathData)
        {
            _routeData = routeData;
            _virtualPathData = virtualPathData;
        }

        public override RouteData GetRouteData(
            HttpContextBase httpContext)
        {
            return _routeData;
        }

        public override VirtualPathData GetVirtualPath(
            RequestContext requestContext,
            RouteValueDictionary values)
        {
            return _virtualPathData;
        }
    }

    public class FakeRouteBuilder : GenericBuilderWithFieldAccess<FakeRouteBuilder, FakeRoute>
    {
        private RouteData _routeData;
        private VirtualPathData _virtualPathData;

        public override FakeRouteBuilder WithRandomProps()
        {
            return base.WithRandomProps()
                .WithRouteData(RouteDataBuilder.BuildRandom())
                .WithVirtualPathData(VirtualPathDataBuilder.BuildRandom());
        }

        public FakeRouteBuilder WithRouteData(RouteData routeData)
        {
            return WithField(o => o._routeData = routeData);
        }

        public FakeRouteBuilder WithVirtualPathData(VirtualPathData virtualPathData)
        {
            return WithField(o => o._virtualPathData = virtualPathData);
        }

        public override FakeRoute Build()
        {
            return new FakeRoute(
                _routeData,
                _virtualPathData);
        }
    }

    public class VirtualPathDataBuilder : GenericBuilderWithFieldAccess<VirtualPathDataBuilder, VirtualPathData>
    {
        public VirtualPathDataBuilder WithRoute(RouteBase route)
        {
            return WithProp(o => o.Route = route);
        }

        public VirtualPathDataBuilder WithDataToken(
            string key,
            string value)
        {
            return WithProp(o => o.DataTokens[key] = value);
        }

        public VirtualPathDataBuilder WithDataTokens(
            IDictionary<string, string> dataTokens)
        {
            return WithProp(o =>
            {
                o.DataTokens.Clear();
                dataTokens.ForEach(kvp => WithDataToken(kvp.Key, kvp.Value));
            });
        }


    }

    public class RouteDataBuilder : GenericBuilderWithFieldAccess<RouteDataBuilder, RouteData>
    {
        public override RouteDataBuilder WithRandomProps()
        {
            return WithRoute(FakeRouteBuilder.BuildRandom())
                .WithRouteHandler(Substitute.For<IRouteHandler>());
        }

        private IRouteHandler _routeHandler;

        public RouteDataBuilder WithRoute(RouteBase route)
        {
            return WithProp(o => o.Route = route);
        }

        public RouteDataBuilder WithValues(
            IDictionary<string, string> routeValues
        )
        {
            return WithProp(o =>
            {
                o.Values.Clear();
                routeValues.ForEach(kvp => WithValue(kvp.Key, kvp.Value));
            });
        }

        public RouteDataBuilder WithValue(
            string key,
            string value)
        {
            return WithProp(o => o.Values[key] = value);
        }

        public RouteDataBuilder WithDataToken(
            string key,
            string value)
        {
            return WithProp(o => o.DataTokens[key] = value);
        }

        public RouteDataBuilder WithDataTokens(
            IDictionary<string, string> dataTokens)
        {
            return WithProp(o =>
            {
                o.DataTokens.Clear();
                dataTokens.ForEach(kvp => WithDataToken(kvp.Key, kvp.Value));
            });
        }

        public RouteDataBuilder WithRouteHandler(IRouteHandler routeHandler)
        {
            return WithField(o => o._routeHandler = routeHandler);
        }

        public override RouteData Build()
        {
            var result = new RouteData();
            result.Route = FakeRouteBuilder.Create()
                .WithRouteData(result)
                .Build();
            result.RouteHandler = _routeHandler;
            return result;
        }
    }
}