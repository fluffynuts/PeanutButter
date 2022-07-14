using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PeanutButter.TestUtils.AspNetCore.Fakes;

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
            WithHttpContext(HttpContextBuilder.BuildDefault());
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
    }
}