using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Builders;
#else
namespace PeanutButter.TestUtils.AspNetCore.Builders;
#endif

/// <summary>
/// Provides a trivial implementation of IRouter producing MVC-like virtual
/// paths from GetVirtualPath
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class TrivialRouter : IRouter
{
    /// <inheritdoc />
    public Task RouteAsync(RouteContext context)
    {
        // not implemented for testing
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public VirtualPathData GetVirtualPath(VirtualPathContext context)
    {
        var area = (context.Values["area"] ?? context.AmbientValues["area"]) as string;
        var controller = (context.Values["controller"] ?? context.AmbientValues["controller"]) as string;
        var action = (context.Values["action"] ?? context.AmbientValues["action"]) as string;
        var allKeys = context.Values.Keys.Concat(context.AmbientValues.Keys)
            .Except(
                new[]
                {
                    "area",
                    "controller",
                    "action"
                }
            );
        var urlParameters = new Dictionary<string, object>();
        foreach (var k in allKeys)
        {
            urlParameters[k] = context.Values[k] ?? context.AmbientValues[k];
        }

        var query = urlParameters.Any()
            ? "?" + string.Join(
                "&",
                urlParameters.Select(
                    kvp =>
                        $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode($"{kvp.Value}")}"
                )
            )
            : "";
        var parts = new[]
            {
                area,
                controller,
                action
            }
            .Where(s => s is not null)
            .ToArray();
        return new VirtualPathData(
            this,
            $"{string.Join("/", parts)}{query}"
        );
    }
}