using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.Utils;

// ReSharper disable ClassNeverInstantiated.Global

namespace PeanutButter.TestUtils.AspNetCore.Builders;

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
            .WithRouteData(new RouteData())
            .WithRouteDataValue("controller", "Home")
            .WithRouteDataValue("action", "Index")
            .WithActionDescriptor(ControllerActionDescriptorBuilder.BuildDefault());
    }

    /// <summary>
    /// Sets a RouteData value (eg "controller", "action", etc)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public ControllerContextBuilder WithRouteDataValue(string key, string value)
    {
        return With(o => o.RouteData.Values[key] = value);
    }

    /// <summary>
    /// Sets the RouteData on the context
    /// </summary>
    /// <param name="routeData"></param>
    /// <returns></returns>
    public ControllerContextBuilder WithRouteData(RouteData routeData)
    {
        return With(o => o.RouteData = routeData);
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

    /// <summary>
    /// Add the identity to the available claims identities
    /// - the original ClaimsPrincipal should be empty
    /// </summary>
    /// <param name="identity"></param>
    /// <returns></returns>
    public ControllerContextBuilder WithIdentity(
        IIdentity identity
    )
    {
        return With(o =>
        {
            var principal = o.HttpContext.User ?? new ClaimsPrincipal();
            principal.AddIdentity(new ClaimsIdentity(identity));
            o.HttpContext.User = principal;
        });
    }

    /// <summary>
    /// Sets the User on the HttpContext of the ControllerContext
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public ControllerContextBuilder WithUser(
        ClaimsPrincipal user
    )
    {
        return With(o => o.HttpContext.User = user);
    }

    /// <summary>
    /// Facilitates easier http context mutations
    /// </summary>
    /// <param name="mutator"></param>
    /// <returns></returns>
    public ControllerContextBuilder WithHttpContextMutator(
        Action<FakeHttpContext> mutator
    )
    {
        return With(o => mutator(o.HttpContext.As<FakeHttpContext>()));
    }

    /// <summary>
    /// Associates the context with the provided controller
    /// </summary>
    /// <param name="controller"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public ControllerContextBuilder WithController<T>(
        T controller
    ) where T : ControllerBase
    {
        var controllerType = typeof(T);
        var controllerName = controllerType.Name.RegexReplace("Controller$", "");
        return With(o =>
            {
                controller.ControllerContext = o;
                o.ActionDescriptor.ControllerName = controllerType.Name.RegexReplace("Controller$", "");
                o.ActionDescriptor.ControllerTypeInfo = controllerType.GetTypeInfo();
                if (o.ActionDescriptor.ActionName is not null &&
                    o.ActionDescriptor.MethodInfo is null)
                {
                    // allows setting action before controller
                    o.ActionDescriptor.MethodInfo = controllerType.GetMethod(o.ActionDescriptor.ActionName);
                }
            }
        ).WithRouteDataValue("controller", controllerName);
    }

    /// <summary>
    /// Set a request header on the associated http context
    /// </summary>
    /// <param name="header"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public ControllerContextBuilder WithRequestHeader(
        string header,
        string value
    )
    {
        return WithHttpContextMutator(o => o.Request.Headers[header] = value);
    }

    /// <summary>
    /// Sets the action on the context
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public ControllerContextBuilder WithAction(string name)
    {
        return With(o =>
        {
            o.ActionDescriptor.ActionName = o.ActionDescriptor.DisplayName =  name;
            if (o.ActionDescriptor.ControllerTypeInfo is not null)
            {
                o.ActionDescriptor.MethodInfo = o.ActionDescriptor.ControllerTypeInfo.GetMethod(name);
            }
        });
        }
    }