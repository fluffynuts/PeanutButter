using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using PeanutButter.TestUtils.AspNetCore.Fakes;

// ReSharper disable ClassNeverInstantiated.Global

namespace PeanutButter.TestUtils.AspNetCore.Builders;

/// <summary>
/// Constructs an ActionExecutingContext
/// </summary>
public class ActionExecutingContextBuilder
    : Builder<ActionExecutingContextBuilder, ActionExecutingContext>
{
    private ActionContext _actionContext;
    private IList<IFilterMetadata> _filterMetadata;
    private IDictionary<string, object> _actionArguments;
    private object _controller;
    private HttpContext _httpContext;
    private RouteData _routeData;
    private ActionDescriptor _actionDescriptor;

    /// <inheritdoc />
    public ActionExecutingContextBuilder()
    {
        WithPreCursor(() => _actionContext = GenerateDefaultActionContext())
            .WithPreCursor(() => _filterMetadata = new List<IFilterMetadata>())
            .WithPreCursor(() => _actionArguments = new Dictionary<string, object>())
            .WithPreCursor(() => _controller = null)
            .WithPreCursor(() => _httpContext = null)
            .WithPreCursor(() => _routeData = null)
            .WithPreCursor(() => _actionDescriptor = null);
    }

    /// <summary>
    /// Sets an action argument on the ActionExecutingContext
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public ActionExecutingContextBuilder WithActionArgument(
        string key,
        object value
    )
    {
        return With(
            o => o.ActionArguments[key] = value
        );
    }

    /// <summary>
    /// Sets the "action" route parameter
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public ActionExecutingContextBuilder WithAction(
        string action
    )
    {
        return WithRouteValue("action", action);
    }

    /// <summary>
    /// Sets a value on the associated route data
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public ActionExecutingContextBuilder WithRouteValue(
        string key,
        string value
    )
    {
        return With(o => o.RouteData.Values[key] = value);
    }

    /// <summary>
    /// Sets the controller for the ActionExecutingContext
    /// </summary>
    /// <param name="controller"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public ActionExecutingContextBuilder WithController<T>(
        T controller
    ) where T : ControllerBase
    {
        return With(
            o =>
            {
                var fake = o.As<FakeActionExecutingContext>();
                fake.SetController(controller);
            }
        );
    }

    /// <summary>
    /// Sets an header on the HttpContext.Request of the ActionExecutingContext
    /// </summary>
    /// <param name="header"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public ActionExecutingContextBuilder WithHeader(
        string header,
        string value
    )
    {
        return With(
            o => o.HttpContext.Request.Headers[header] = value
        );
    }

    /// <summary>
    /// Adds filter metadata to the ActionExecutingContext
    /// </summary>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public ActionExecutingContextBuilder WithFilterMetadata(
        IFilterMetadata metadata
    )
    {
        return With(
            o => o.Filters.Add(metadata)
        );
    }

    /// <summary>
    /// Constructs the fake ActionExecutingContext
    /// </summary>
    /// <returns></returns>
    /// <exception cref="MissingConstructorRequirementException{ActionExecutingContext}"></exception>
    protected override ActionExecutingContext ConstructEntity()
    {
        return new FakeActionExecutingContext(
            _actionContext ?? throw Missing(nameof(_actionContext)),
            _filterMetadata ?? throw Missing(nameof(_filterMetadata)),
            _actionArguments ?? throw Missing(nameof(_actionArguments)),
            _controller // could be set later
        );

        MissingConstructorRequirementException<ActionExecutingContext> Missing(string ctx)
        {
            return new MissingConstructorRequirementException<ActionExecutingContext>(ctx);
        }
    }

    private ActionContext GenerateDefaultActionContext()
    {
        return new ActionContext(
            _httpContext ?? HttpContextBuilder.BuildDefault(),
            _routeData ?? new RouteData(),
            _actionDescriptor ?? new ActionDescriptor()
        );
    }
}