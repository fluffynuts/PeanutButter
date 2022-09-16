using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using PeanutButter.TestUtils.AspNetCore.Fakes;
using PeanutButter.TestUtils.AspNetCore.Utils;

// ReSharper disable MemberCanBePrivate.Global

namespace PeanutButter.TestUtils.AspNetCore.Builders;

/// <summary>
/// Starts off a builder for your ApiController
/// (controller inheriting from ControllerBase only)
/// </summary>
public static class ControllerBuilder
{
    /// <summary>
    /// Create the builder for your controller
    /// </summary>
    /// <typeparam name="TController"></typeparam>
    /// <returns></returns>
    public static ControllerBuilder<TController> For<TController>()
        where TController : ControllerBase
    {
        return new ControllerBuilder<TController>();
    }
}

/// <summary>
/// Builds your
/// </summary>
/// <typeparam name="TController"></typeparam>
public class ControllerBuilder<TController>
    : Builder<ControllerBuilder<TController>, TController>
    where TController : ControllerBase
{
    private Func<TController> _factory;

    /// <inheritdoc />
    public ControllerBuilder()
    {
        WithControllerContext(
                ControllerContextBuilder.Create()
                    .WithControllerType(typeof(TController))
                    .Build()
            )
            .WithRouteData(RouteDataBuilder.BuildDefault())
            .WithHttpContext(HttpContextBuilder.Create()
                .WithRequestServices(new MinimalServiceProvider())
                .Build()
            )
            .WithModelMetadataProvider(
                () => new DefaultModelMetadataProvider(
                    new DefaultCompositeMetadataDetailsProvider(
                        new IMetadataDetailsProvider[0]
                    )
                )
            )
            .WithModelValidator(() => new DefaultObjectModelValidator())
            .WithTempDataDictionaryFactory(() => new TempDataDictionaryFactory(
                    new SessionStateTempDataProvider()
                )
            )
            .WithOptions(() => new DefaultOptions())
            .WithFactory(DefaultFactory);
    }

    /// <summary>
    /// Constructs the controller, either using a provided factory
    /// or with the assumption that the controller has a parameterless constructor
    /// </summary>
    /// <returns></returns>
    protected override TController ConstructEntity()
    {
        return _factory();
    }

    private TController DefaultFactory()
    {
        return Activator.CreateInstance<TController>();
    }

    /// <summary>
    /// Provide a factory to create the controller. If your controller
    /// has constructor parameters, this is how you'd inject them.
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithFactory(
        Func<TController> factory
    )
    {
        _factory = factory ?? DefaultFactory;
        return this;
    }

    /// <summary>
    /// Set a query parameter on the request object
    /// NB: this will _not_ automatically map to method
    /// parameters on your actions, so you only need to
    /// set these if you're interrogating the raw query
    /// or want to update the overall querystring
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithRequestQueryParameter(
        string parameter,
        string value
    )
    {
        return WithRequestQueryParameter(
            parameter,
            new StringValues(value)
        );
    }

    /// <summary>
    /// Sets the hostname for the request
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithRequestHost(
        string host
    )
    {
        return With(
            o => o.Request.Host = o.Request.Host.Port.HasValue
                ? new HostString(host, o.Request.Host.Port.Value)
                : new HostString(host)
        );
    }

    /// <summary>
    /// Sets the port for the request
    /// </summary>
    /// <param name="port"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithRequestPort(
        int port
    )
    {
        return With(
            o => o.Request.Host = new HostString(o.Request.Host.Host, port)
        );
    }

    /// <summary>
    /// Sets the host for the request
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithRequestHost(HostString host)
    {
        return With(
            o => o.Request.Host = host
        );
    }

    /// <summary>
    /// Sets the scheme for the request
    /// </summary>
    /// <param name="scheme"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithRequestScheme(
        string scheme
    )
    {
        return With(
            o => o.Request.Scheme = scheme
        );
    }

    /// <summary>
    /// Sets a query parameter on the request for the controller
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithRequestQueryParameter(
        string parameter,
        StringValues value
    )
    {
        return With(
            o => o.Request.Query.As<FakeQueryCollection>()[parameter] = value
        );
    }

    /// <summary>
    /// Sets an item on the http context for the controller
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithHttpContextItem(
        string key,
        object value
    )
    {
        return With(
            o => o.HttpContext.Items[key] = value
        );
    }

    /// <summary>
    /// Sets a cookie value on the request for the controller
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithRequestCookie(
        string key,
        string value
    )
    {
        return With(
            o => o.Request.Cookies.As<FakeRequestCookieCollection>()[key] = value
        );
    }

    /// <summary>
    /// Sets a header on the request for the controller
    /// </summary>
    /// <param name="header"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithRequestHeader(
        string header,
        string value
    )
    {
        return With(
            o => o.Request.Headers[header] = value
        );
    }

    /// <summary>
    /// Sets the controller context for your controller
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithControllerContext(
        ControllerContext ctx
    )
    {
        return With(
            o => o.ControllerContext = ctx
        );
    }

    /// <summary>
    /// Sets the action descriptor for your controller
    /// </summary>
    /// <param name="actionDescriptor"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithActionDescriptor(
        ControllerActionDescriptor actionDescriptor
    )
    {
        return With(
            o => o.ControllerContext.ActionDescriptor = actionDescriptor
        );
    }

    /// <summary>
    /// Sets the route data for your controller
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithRouteData(RouteData data)
    {
        return With(
            o => o.ControllerContext.RouteData = data
        );
    }

    /// <summary>
    /// Sets the http context for your controller
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithHttpContext(
        HttpContext context
    )
    {
        return With(o => o.ControllerContext.HttpContext = context)
            .WithActionContext(ctx => new ActionContext(
                    ctx.HttpContext,
                    ctx.RouteData,
                    ctx.ActionDescriptor
                )
            );
    }

    /// <summary>
    /// Set the options for the controller
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithOptions(
        IOptions<MvcOptions> options
    )
    {
        return WithOptions(() => options);
    }

    /// <summary>
    /// Set the options for the controller (late-resolved singleton)
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithOptions(
        Func<IOptions<MvcOptions>> factory
    )
    {
        return WithRegistration(
            (sp, _) => sp.RegisterSingleton(factory)
        );
    }

    /// <summary>
    /// Set the temp data dictionary factory for the controller
    /// </summary>
    /// <param name="tempDataDictionaryFactory"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithTempDataDictionaryFactory(
        ITempDataDictionaryFactory tempDataDictionaryFactory
    )
    {
        return WithTempDataDictionaryFactory(() => tempDataDictionaryFactory);
    }

    /// <summary>
    /// Set the temp data dictionary factory for the controller (late-resolved singleton)
    /// </summary>
    /// <param name="factoryFactory"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithTempDataDictionaryFactory(
        Func<ITempDataDictionaryFactory> factoryFactory
    )
    {
        return WithRegistration(
            (sp, _) => sp.RegisterSingleton(factoryFactory)
        );
    }

    /// <summary>
    /// Set the model validator
    /// </summary>
    /// <param name="validator"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithModelValidator(
        IObjectModelValidator validator
    )
    {
        return WithModelValidator(() => validator);
    }

    /// <summary>
    /// Set the model validator (late-resolved singleton)
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithModelValidator(
        Func<IObjectModelValidator> factory
    )
    {
        return WithRegistration(
            (sp, _) => sp.RegisterSingleton(factory)
        );
    }

    /// <summary>
    /// Set the model metadata provider
    /// </summary>
    /// <param name="modelMetadataProvider"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithModelMetadataProvider(
        Func<IModelMetadataProvider> modelMetadataProvider
    )
    {
        return WithRegistration(
            (sp, _) => sp.RegisterSingleton(modelMetadataProvider)
        );
    }

    /// <summary>
    /// Set the model metadata provider (late-bound singleton)
    /// </summary>
    /// <param name="modelMetadataProvider"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithModelMetadataProvider(
        IModelMetadataProvider modelMetadataProvider
    )
    {
        return WithModelMetadataProvider(() => modelMetadataProvider);
    }

    /// <summary>
    /// Set the action context (will be automatically called when you set
    /// the http context). This variant will have access to the controller
    /// so it can access any property of said controller.
    /// </summary>
    /// <param name="factory"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithActionContext(
        Func<ControllerContext, ActionContext> factory
    )
    {
        return WithRegistration(
            (sp, o) => sp.RegisterSingleton(() => factory(o.ControllerContext))
        );
    }

    /// <summary>
    /// Set the action context (late-bound singleton)
    /// </summary>
    /// <param name="actionContext"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithActionContext(
        ActionContext actionContext
    )
    {
        return WithRegistration(
            (sp, _) => sp.RegisterSingleton(() => actionContext)
        );
    }

    private ControllerBuilder<TController> WithRegistration(
        Action<IMinimalServiceProvider, TController> registration
    )
    {
        return With(
            o =>
            {
                var context = o.ControllerContext?.HttpContext
                    ?? throw new InvalidOperationException("HttpContext not set up on ControllerContext yet");
                if (context.RequestServices is not IMinimalServiceProvider minimalServiceProvider)
                {
                    throw new InvalidOperationException(
                        $"Cannot late-bind services the RequestServices on HttpContext is not a {nameof(IMinimalServiceProvider)}"
                    );
                }

                registration(minimalServiceProvider, o);
            }
        );
    }

    /// <summary>
    /// Sets the action on the controller descriptor. If the
    /// action can be found on the controller, the controller
    /// descriptor's method info will also be set
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithAction(string name)
    {
        return With(
            o => o.ControllerContext.ActionDescriptor.ActionName = name
        ).With(
            o => o.ControllerContext.ActionDescriptor.MethodInfo =
                typeof(TController).GetMethod(name)
        );
    }

    /// <summary>
    /// Sets the url on the associated request. Does NOT set
    /// any routing parameters (eg action)
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithRequestUrl(string url)
    {
        return WithRequestUrl(new Uri(url));
    }

    /// <summary>
    /// Sets the url on the associated request. Does NOT set
    /// any routing parameters (eg action)
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithRequestUrl(Uri url)
    {
        return WithRequestMutator(o => o.SetUrl(url));
    }

    /// <summary>
    /// Facilitates arbitrary mutations on the existing FakeHttpRequest
    /// for this build
    /// </summary>
    /// <param name="mutator"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithRequestMutator(
        Action<FakeHttpRequest> mutator
    )
    {
        return With(o => mutator(o.Request.As<FakeHttpRequest>()));
    }

    /// <summary>
    /// Set multiple request headers at once
    /// </summary>
    /// <param name="headers"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithRequestHeaders(
        IDictionary<string, string> headers
    )
    {
        return WithRequestMutator(
            r =>
            {
                foreach (var kvp in headers)
                {
                    r.Headers[kvp.Key] = kvp.Value;
                }
            }
        );
    }

    /// <summary>
    /// Set the request method for the associated request
    /// </summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public ControllerBuilder<TController> WithRequestMethod(
        string method
    )
    {
        return WithRequestMutator(req =>
        {
            req.Method = method;
        });
    }
}