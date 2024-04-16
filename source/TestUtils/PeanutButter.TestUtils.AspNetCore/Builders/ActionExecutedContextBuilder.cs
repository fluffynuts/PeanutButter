using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
namespace Imported.PeanutButter.TestUtils.AspNetCore.Builders;
#else
using PeanutButter.TestUtils.AspNetCore.Fakes;
namespace PeanutButter.TestUtils.AspNetCore.Builders;
#endif

/// <summary>
/// Builds an ActionExecutedContext
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class ActionExecutedContextBuilder
    : Builder<ActionExecutedContextBuilder, ActionExecutedContext>
{
    private ActionContext _actionContext;
    private IList<IFilterMetadata> _filterMetadata;
    private object _controller;
    private HttpContext _httpContext;
    private RouteData _routeData;
    private ActionDescriptor _actionDescriptor;

    /// <inheritdoc />
    public ActionExecutedContextBuilder()
    {
        WithPreCursor(() => _actionContext = GenerateDefaultActionContext())
            .WithPreCursor(() => _filterMetadata = new List<IFilterMetadata>())
            .WithPreCursor(() => _controller = null)
            .WithPreCursor(() => _httpContext = null)
            .WithPreCursor(() => _routeData = null)
            .WithPreCursor(() => _actionDescriptor = null);
    }

    /// <summary>
    /// Set the controller for the ActionExecutedContext
    /// </summary>
    /// <param name="controller"></param>
    /// <returns></returns>
    public ActionExecutedContextBuilder WithController<T>(
        T controller
    ) where T : ControllerBase
    {
        return With(
            o =>
            {
                var fake = o.As<FakeActionExecutedContext>();
                fake.SetController(controller);
            }
        );
    }

    /// <summary>
    /// Set an header on the HttpContest.Request of the ActionExecutedContext
    /// </summary>
    /// <param name="header"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public ActionExecutedContextBuilder WithHeader(
        string header,
        string value
    )
    {
        return With(
            o => o.HttpContext.Request.Headers[header] = value
        );
    }

    /// <summary>
    /// Add filter metadata to the ActionExecutedContext
    /// </summary>
    /// <param name="metadata"></param>
    /// <returns></returns>
    public ActionExecutedContextBuilder WithFilterMetadata(
        IFilterMetadata metadata
    )
    {
        return With(
            o => o.Filters.Add(metadata)
        );
    }

    /// <summary>
    /// Constructs the fake ActionExecutedContext
    /// </summary>
    /// <returns></returns>
    /// <exception cref="MissingConstructorRequirementException{ActionExecutingContext}"></exception>
    protected override ActionExecutedContext ConstructEntity()
    {
        return new FakeActionExecutedContext(
            _actionContext ?? throw Missing(nameof(_actionContext)),
            _filterMetadata ?? throw Missing(nameof(_filterMetadata)),
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