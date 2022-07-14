using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.Utils;

// ReSharper disable ClassNeverInstantiated.Global

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    /// <summary>
    /// Builds a controller context
    /// </summary>
    public class ControllerContextBuilder : Builder<ControllerContextBuilder, ControllerContext>
    {
        /// <inheritdoc />
        public ControllerContextBuilder()
        {
            // ensure fake is installed for late overriding of request/response
            WithHttpContext(HttpContextBuilder.BuildDefault())
                .WithActionDescriptor(ControllerActionDescriptorBuilder.BuildDefault());
        }

        /// <summary>
        /// Sets the http context for the controller
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public ControllerContextBuilder WithHttpContext(
            HttpContext context
        )
        {
            return With(o => o.HttpContext = context);
        }

        /// <summary>
        /// Sets the request on the http context for the controller (if possible)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public ControllerContextBuilder WithRequest(
            HttpRequest request
        )
        {
            return With(o =>
            {
                var fake = o.HttpContext.As<FakeHttpContext>();
                fake.SetRequestAccessor(() => request);
            });
        }

        /// <summary>
        /// Sets the response on the http context for the controller (if possible)
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public ControllerContextBuilder WithResponse(
            HttpResponse response
        )
        {
            return With(o =>
            {
                var fake = o.HttpContext.As<FakeHttpContext>();
                fake.SetResponseAccessor(() => response);
            });
        }

        /// <summary>
        /// Sets the ControllerActionDescriptor on the context
        /// </summary>
        /// <param name="descriptor"></param>
        /// <returns></returns>
        public ControllerContextBuilder WithActionDescriptor(
            ControllerActionDescriptor descriptor
        )
        {
            return With(
                o => o.ActionDescriptor = descriptor
            );
        }

        /// <summary>
        /// Sets the controller type on the action descriptor (as well as taking a guess at the name)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ControllerContextBuilder WithControllerType(Type type)
        {
            return With(
                o => o.ActionDescriptor.ControllerTypeInfo = type.GetTypeInfo()
            ).With(
                o => o.ActionDescriptor.ControllerName =
                    type.Name.RegexReplace("Controller$", "")
            );
        }
    }
}