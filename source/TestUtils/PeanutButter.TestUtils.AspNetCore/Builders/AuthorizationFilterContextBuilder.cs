using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.Utils;

namespace PeanutButter.TestUtils.AspNetCore.Builders
{
    /// <summary>
    /// Builds AuthorizationFilterContexts
    /// </summary>
    public class AuthorizationFilterContextBuilder
        : Builder<AuthorizationFilterContextBuilder, AuthorizationFilterContext>
    {
        /// <summary>
        /// Constructs the starting point for an AuthorizationFilterContext
        /// </summary>
        /// <returns></returns>
        protected override AuthorizationFilterContext ConstructEntity()
        {
            var httpContext = HttpContextBuilder.BuildDefault();
            return new AuthorizationFilterContext(
                new ActionContext(
                    httpContext,
                    new RouteData(),
                    new ControllerActionDescriptor(),
                    new ModelStateDictionary()
                ),
                new List<IFilterMetadata>()
            );
        }

        /// <summary>
        /// Sets up the controller name and type info for the AuthorizationFilterContext
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AuthorizationFilterContextBuilder ForController<T>()
            where T : ControllerBase
        {
            return WithActionDescriptorMutator(descriptor =>
            {
                descriptor.ControllerName = typeof(T).Name.RegexReplace("Controller$", "");
                descriptor.ControllerTypeInfo = typeof(T).GetTypeInfo();
            });
        }

        /// <summary>
        /// Sets up the method info, action name and display name for the AuthorizationFilterContext
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public AuthorizationFilterContextBuilder ForAction(
            string action
        )
        {
            return WithActionDescriptorMutator(descriptor =>
            {
                var controllerType = descriptor.ControllerTypeInfo;
                var methodInfo = controllerType.GetMethod(action);
                if (methodInfo is null)
                {
                    throw new Exception($"${controllerType} has no method '{action}'");
                }

                descriptor.MethodInfo = methodInfo;
                descriptor.ActionName = action;
                descriptor.DisplayName = action;
            });
        }

        /// <summary>
        /// Sets a header on the request
        /// </summary>
        /// <param name="header"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AuthorizationFilterContextBuilder WithRequestHeader<T>(
            string header,
            T value
        )
        {
            return WithRequestMutator(req => req.Headers[header] = $"{value}");
        }


        /// <summary>
        /// Add a request mutation
        /// </summary>
        /// <param name="mutator"></param>
        /// <returns></returns>
        public AuthorizationFilterContextBuilder WithRequestMutator(
            Action<HttpRequest> mutator
        )
        {
            return With(o =>
            {
                mutator(o.HttpContext.Request);
            });
        }

        /// <summary>
        /// Add a response mutation
        /// </summary>
        /// <param name="mutator"></param>
        /// <returns></returns>
        public AuthorizationFilterContextBuilder WithResponseMutator(
            Action<HttpResponse> mutator
        )
        {
            return With(o => mutator(o.HttpContext.Response));
        }

        /// <summary>
        /// Mutate the action context as a controller action context
        /// </summary>
        /// <param name="mutator"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public AuthorizationFilterContextBuilder WithActionDescriptorMutator(
            Action<ControllerActionDescriptor> mutator
        )
        {
            return With(o =>
            {
                if (o.ActionDescriptor is not ControllerActionDescriptor descriptor)
                {
                    throw new NotSupportedException(
                        $"{nameof(o.ActionDescriptor)} must be a {nameof(ControllerActionDescriptor)}"
                    );
                }

                mutator(descriptor);
            });
        }

        /// <summary>
        /// Sets a JSON body for the request
        /// </summary>
        /// <param name="payload"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AuthorizationFilterContextBuilder WithJsonBody<T>(T payload)
        {
            return With(o =>
            {
                var json = payload is string str
                    ? str
                    : JsonSerializer.Serialize(payload);
                o.HttpContext.Request.Body = json.AsStream();
                o.HttpContext.Request.ContentType = "application/json";
            });
        }

        /// <summary>
        /// Set a cookie on the request
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public AuthorizationFilterContextBuilder WithRequestCookie(
            string key,
            string value
        )
        {
            return With(o =>
            {
                if (o.HttpContext.Request.Cookies is not FakeRequestCookieCollection cookies)
                {
                    // TODO: modify the header instead?
                    throw new NotSupportedException(
                        $"Can only set cookies when the Request.Cookies has been set up as a {nameof(FakeRequestCookieCollection)}"
                    );
                }

                cookies[key] = value;
            });
        }

        /// <summary>
        /// Set the url on the associated request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public AuthorizationFilterContextBuilder WithRequestUrl(
            string url
        )
        {
            return WithRequestUrl(new Uri(url));
        }

        /// <summary>
        /// Set the url on the associated request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public AuthorizationFilterContextBuilder WithRequestUrl(
            Uri url
        )
        {
            return WithRequestMutator(
                req => req.SetUrl(url)
            );
        }

        /// <summary>
        /// Set multiple cookies on the request
        /// </summary>
        /// <param name="cookies"></param>
        /// <returns></returns>
        public AuthorizationFilterContextBuilder WithRequestCookies(
            IDictionary<string, string> cookies
        )
        {
            return WithRequestMutator(req =>
            {
                CookieUtil.GenerateCookieHeader(
                    cookies ?? new Dictionary<string, string>(),
                    req,
                    overwrite: false
                );
            });
        }
    }
}