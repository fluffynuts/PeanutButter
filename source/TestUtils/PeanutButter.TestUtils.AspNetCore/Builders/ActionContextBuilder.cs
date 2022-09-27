using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    /// <summary>
    /// Builds ActionContexts
    /// </summary>
    public class ActionContextBuilder : RandomizableBuilder<ActionContextBuilder, ActionContext>
    {
        /// <inheritdoc />
        public ActionContextBuilder()
        {
            WithHttpContext(HttpContextBuilder.BuildDefault())
                .WithActionDescriptor(new ActionDescriptor())
                .WithRouteData(RouteDataBuilder.BuildDefault());
        }

        /// <summary>
        /// Sets the RouteData on the ActionContext
        /// </summary>
        /// <param name="routeData"></param>
        /// <returns></returns>
        public ActionContextBuilder WithRouteData(RouteData routeData)
        {
            return With(o => o.RouteData = routeData);
        }

        /// <summary>
        /// Sets the ActionDescriptor on the ActionContext
        /// </summary>
        /// <param name="actionDescriptor"></param>
        /// <returns></returns>
        public ActionContextBuilder WithActionDescriptor(ActionDescriptor actionDescriptor)
        {
            return With(o => o.ActionDescriptor = actionDescriptor);
        }

        /// <inheritdoc />
        protected override ActionContext ConstructEntity()
        {
            return new ActionContext();
        }

        /// <summary>
        /// Sets the HttpContext on the ActionContext
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public ActionContextBuilder WithHttpContext(HttpContext httpContext)
        {
            return With(o => o.HttpContext = httpContext);
        }

        /// <inheritdoc />
        public override ActionContextBuilder Randomize()
        {
            return this;
        }

        /// <summary>
        /// Set a RouteData Value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public ActionContextBuilder WithRouteDataValue(string key, string value)
        {
            return WithRouteDataMutator(o => o.Values[key] = value);
        }

        /// <summary>
        /// Mutate the RouteData on the ActionDescriptor
        /// </summary>
        /// <param name="mutator"></param>
        /// <returns></returns>
        public ActionContextBuilder WithRouteDataMutator(Action<RouteData> mutator)
        {
            return With(o => mutator(o.RouteData));
        }
    }
}