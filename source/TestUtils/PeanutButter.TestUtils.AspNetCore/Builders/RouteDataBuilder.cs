using Microsoft.AspNetCore.Routing;

namespace PeanutButter.TestUtils.AspNetCore.Builders;

/// <summary>
/// Builds RouteData
/// </summary>
public class RouteDataBuilder : Builder<RouteDataBuilder, RouteData>
{
    /// <summary>
    /// Sets up the default RouteData
    /// </summary>
    public RouteDataBuilder()
    {
        WithRouter(new TrivialRouter());
    }

    /// <summary>
    /// Sets a route value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public RouteDataBuilder WithRouteValue(string key, string value)
    {
        return With(
            o => o.Values[key] = value
        );
    }

    /// <summary>
    /// Clears out any existing route values
    /// </summary>
    /// <returns></returns>
    public RouteDataBuilder WithNoRouteValues()
    {
        return With(
            o => o.Values.Clear()
        );
    }

    /// <summary>
    /// Sets a data token
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public RouteDataBuilder WithDataToken(string key, string value)
    {
        return With(
            o => o.DataTokens[key] = value
        );
    }

    /// <summary>
    /// Removes all data tokens
    /// </summary>
    /// <returns></returns>
    public RouteDataBuilder WithNoDataTokens()
    {
        return With(
            o => o.DataTokens.Clear()
        );
    }

    /// <summary>
    /// Adds a router
    /// </summary>
    /// <param name="router"></param>
    /// <returns></returns>
    public RouteDataBuilder WithRouter(IRouter router)
    {
        return With(
            o => o.Routers.Add(router)
        );
    }

    /// <summary>
    /// Clears all routers
    /// </summary>
    /// <returns></returns>
    public RouteDataBuilder WithNoRouters()
    {
        return With(
            o => o.Routers.Clear()
        );
    }
}