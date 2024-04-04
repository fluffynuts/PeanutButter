using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.Utils;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore;
#else
namespace PeanutButter.TestUtils.AspNetCore;
#endif

/// <summary>
/// Provides extensions for action results
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    static class ActionResultExtensions
{
    /// <summary>
    /// Resolve the HttpResponse that would be generated for
    /// the provided action result (synchronous overload)
    /// </summary>
    /// <param name="actionResult"></param>
    /// <returns></returns>
    public static HttpResponse ResolveResponse(
        this IActionResult actionResult
    )
    {
        return Async.RunSync(actionResult.ResolveResponseAsync);
    }

    /// <summary>
    /// Resolve the HttpResponse that would be generated for
    /// the provided action result
    /// </summary>
    /// <param name="actionResult"></param>
    /// <returns></returns>
    public static async Task<HttpResponse> ResolveResponseAsync(
        this IActionResult actionResult
    )
    {
        var result = new ActionContext(
            HttpContextBuilder.BuildDefault(),
            RouteDataBuilder.BuildDefault(),
            new ControllerActionDescriptor(),
            new ModelStateDictionary()
        );
        await actionResult.ExecuteResultAsync(result);
        return result.HttpContext.Response;
    }
}