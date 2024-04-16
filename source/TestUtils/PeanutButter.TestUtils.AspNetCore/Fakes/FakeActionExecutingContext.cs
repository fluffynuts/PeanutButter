using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
// ReSharper disable ConstantNullCoalescingCondition

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
#else
namespace PeanutButter.TestUtils.AspNetCore.Fakes;
#endif

/// <summary>
/// Provides an ActionExecutingContext implementation where
/// the controller can be late-set
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class FakeActionExecutingContext : ActionExecutingContext, IFake
{
    /// <inheritdoc />
    public FakeActionExecutingContext(
        ActionContext actionContext,
        IList<IFilterMetadata> filters,
        IDictionary<string, object> actionArguments,
        object controller
    ) : base(actionContext, filters, actionArguments, controller)
    {
        _controller = controller;
        Filters = filters ?? new List<IFilterMetadata>();
    }

    /// <inheritdoc />
    public override object Controller =>
        _controller;

    private object _controller;

    /// <summary>
    /// Sets the controller
    /// </summary>
    /// <param name="controller"></param>
    public void SetController(object controller)
    {
        _controller = controller;
    }

    /// <inheritdoc />
    public override IList<IFilterMetadata> Filters { get; }
}