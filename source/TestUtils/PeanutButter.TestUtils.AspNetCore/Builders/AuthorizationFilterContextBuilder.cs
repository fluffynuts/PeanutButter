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

// ReSharper disable StaticMemberInGenericType

#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
using Imported.PeanutButter.Utils;
namespace ImportedPeanutButter.TestUtils.AspNetCore.Builders;
#else
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.Utils;
namespace PeanutButter.TestUtils.AspNetCore.Builders;
#endif

/// <summary>
/// Easier start for building an AuthorizationContext via
/// AuthorizationContextBuilder.ForController&lt;TController&gt;()
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    abstract class AuthorizationFilterContextBuilder
{
    /// <summary>
    /// Sets up the controller name and type info for the AuthorizationFilterContext
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static AuthorizationFilterContextBuilder<T> ForController<T>()
        where T : ControllerBase
    {
        return AuthorizationFilterContextBuilder<T>
            .Create()
            .WithController<T>();
    }
}

/// <summary>
/// Builds AuthorizationFilterContexts
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class AuthorizationFilterContextBuilder<TController>
    : Builder<AuthorizationFilterContextBuilder<TController>, AuthorizationFilterContext>
    where TController : ControllerBase
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
    public AuthorizationFilterContextBuilder<TController> WithController<T>()
        where T : ControllerBase
    {
        return WithActionDescriptorMutator(
            descriptor =>
            {
                descriptor.ControllerName = typeof(T).Name.RegexReplace("Controller$", "");
                descriptor.ControllerTypeInfo = typeof(T).GetTypeInfo();
            }
        ).ForDefaultAction();
    }

    /// <summary>
    /// Selects a default action on the controller to associate with the auth filter
    /// context so that the descriptor will be valid by default
    /// </summary>
    /// <returns></returns>
    public AuthorizationFilterContextBuilder<TController> ForDefaultAction()
    {
        var methods = typeof(TController)
            .GetMethods()
            .Where(mi => mi.DeclaringType == typeof(TController))
            .ToArray();
        var selected = DefaultActionSelectors.Aggregate(
            null as MethodInfo,
            (acc, cur) => acc ?? cur(methods)
        );
        return ForAction(selected?.Name);
    }

    private static readonly Func<MethodInfo[], MethodInfo>[] DefaultActionSelectors =
    {
        TryFindIndex,
        TryFindFirstGetWithEmptyRoute,
        FindFirstWithActionResult,
        FindFirst
    };

    private static MethodInfo TryFindFirstGetWithEmptyRoute(MethodInfo[] arg)
    {
        return arg.FirstOrDefault(
            mi =>
            {
                var attribs = mi.GetCustomAttributes().ToArray();
                var isGet = attribs.Any(a => a.GetType() == typeof(HttpGetAttribute)) ||
                    attribs.None(a => NonGetAttributes.Contains(a.GetType()));
                if (!isGet)
                {
                    return false;
                }

                var routeAttrib = attribs.FirstOrDefault(a => a is RouteAttribute) as RouteAttribute;
                return routeAttrib is null || routeAttrib.Template == "";
            }
        );
    }

    private static readonly HashSet<Type> NonGetAttributes = new HashSet<Type>(
        new[]
        {
            typeof(HttpPostAttribute),
            typeof(HttpPatchAttribute),
            typeof(HttpPutAttribute),
            typeof(HttpDeleteAttribute),
            typeof(HttpOptionsAttribute),
            typeof(HttpHeadAttribute)
        }
    );

    private static MethodInfo FindFirst(MethodInfo[] arg)
    {
        return arg.FirstOrDefault();
    }

    private static MethodInfo FindFirstWithActionResult(MethodInfo[] arg)
    {
        var want = typeof(IActionResult);
        return arg.Aggregate(
            null as MethodInfo,
            (acc, cur) => acc ?? (want.IsAssignableFrom(cur.ReturnType)
                    ? cur
                    : null
                )
        );
    }

    private static MethodInfo TryFindIndex(MethodInfo[] arg)
    {
        return arg.FirstOrDefault(mi => mi.Name.Equals("index", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Sets up the method info, action name and display name for the AuthorizationFilterContext
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public AuthorizationFilterContextBuilder<TController> ForAction(
        string action
    )
    {
        return WithActionDescriptorMutator(
            descriptor =>
            {
                if (action is null)
                {
                    descriptor.MethodInfo = null;
                    descriptor.ActionName = null;
                    descriptor.DisplayName = null;
                    return;
                }

                var controllerType = descriptor.ControllerTypeInfo;
                var methodInfo = controllerType.GetMethod(action);
                if (methodInfo is null)
                {
                    throw new Exception($"{controllerType} has no method '{action}'");
                }

                descriptor.MethodInfo = methodInfo;
                descriptor.ActionName = action;
                descriptor.DisplayName = action;
            }
        );
    }

    /// <summary>
    /// Sets a header on the request
    /// </summary>
    /// <param name="header"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public AuthorizationFilterContextBuilder<TController> WithRequestHeader<T>(
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
    public AuthorizationFilterContextBuilder<TController> WithRequestMutator(
        Action<HttpRequest> mutator
    )
    {
        return With(
            o =>
            {
                mutator(o.HttpContext.Request);
            }
        );
    }

    /// <summary>
    /// Add a response mutation
    /// </summary>
    /// <param name="mutator"></param>
    /// <returns></returns>
    public AuthorizationFilterContextBuilder<TController> WithResponseMutator(
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
    public AuthorizationFilterContextBuilder<TController> WithActionDescriptorMutator(
        Action<ControllerActionDescriptor> mutator
    )
    {
        return With(
            o =>
            {
                if (o.ActionDescriptor is not ControllerActionDescriptor descriptor)
                {
                    throw new NotSupportedException(
                        $"{nameof(o.ActionDescriptor)} must be a {nameof(ControllerActionDescriptor)}"
                    );
                }

                mutator(descriptor);
            }
        );
    }

    /// <summary>
    /// Sets a JSON body for the request
    /// </summary>
    /// <param name="payload"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public AuthorizationFilterContextBuilder<TController> WithJsonBody<T>(T payload)
    {
        return With(
            o =>
            {
                var json = payload is string str
                    ? str
                    : JsonSerializer.Serialize(payload);
                o.HttpContext.Request.Body = json.AsStream();
                o.HttpContext.Request.ContentType = "application/json";
            }
        );
    }

    /// <summary>
    /// Set a cookie on the request
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public AuthorizationFilterContextBuilder<TController> WithRequestCookie(
        string key,
        string value
    )
    {
        return With(
            o =>
            {
                if (o.HttpContext.Request.Cookies is not FakeRequestCookieCollection cookies)
                {
                    // TODO: modify the header instead?
                    throw new NotSupportedException(
                        $"Can only set cookies when the Request.Cookies has been set up as a {nameof(FakeRequestCookieCollection)}"
                    );
                }

                cookies[key] = value;
            }
        );
    }

    /// <summary>
    /// Set the url on the associated request
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public AuthorizationFilterContextBuilder<TController> WithRequestUrl(
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
    public AuthorizationFilterContextBuilder<TController> WithRequestUrl(
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
    public AuthorizationFilterContextBuilder<TController> WithRequestCookies(
        IDictionary<string, string> cookies
    )
    {
        return WithRequestMutator(
            req =>
            {
                CookieUtil.GenerateCookieHeader(
                    cookies ?? new Dictionary<string, string>(),
                    req,
                    overwrite: false
                );
            }
        );
    }
}