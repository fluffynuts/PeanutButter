using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
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
        return With(o =>
        {
            if (controller is null)
            {
                o.ActionDescriptor.ControllerName = null;
                o.ActionDescriptor.ControllerTypeInfo = null;
                return;
            }

            controller.ControllerContext = o;
            var controllerType = controller.GetType();
            o.ActionDescriptor.ControllerName = controllerType.Name.RegexReplace("Controller$", "");
            o.ActionDescriptor.ControllerTypeInfo = controllerType.GetTypeInfo();
        });
    }
}