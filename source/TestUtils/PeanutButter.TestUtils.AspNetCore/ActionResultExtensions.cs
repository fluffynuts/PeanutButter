using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

#if BUILD_PEANUTBUTTER_INTERNAL
using Imported.PeanutButter.TestUtils.AspNetCore.Builders;
using Imported.PeanutButter.Utils;
namespace Imported.PeanutButter.TestUtils.AspNetCore;
#else
using PeanutButter.TestUtils.AspNetCore.Builders;
using PeanutButter.Utils;
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


    /// <summary>
    /// Attempts to fetch the model off of the ActionResult.
    /// This requires that you know the model type, otherwise
    /// it will fail.
    /// </summary>
    /// <param name="actionResult"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T FetchModel<T>(
        this IActionResult actionResult
    )
    {
        return actionResult switch
        {
            JsonResult jsonResult => (T)jsonResult.Value,
            ViewResult viewResult => (T)viewResult.Model,
            _ => actionResult.Get<T>("Model")
        };
    }
    
    /// <summary>
    /// Verifies the status code on the action result
    /// </summary>
    /// <param name="actionResult"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static HttpStatusCode StatusCode(
        this IActionResult actionResult
    )
    {
        var statusCode = actionResult.GetOrDefault<int?>(nameof(ObjectResult.StatusCode), null);
        if (statusCode is null)
        {
            var type = actionResult.GetType();
            var prop = type.GetProperty(nameof(ObjectResult.StatusCode));
            if (prop is not null)
            {
                // the result has the prop, but it's nullable, so optional...
                if (actionResult is JsonResult)
                {
                    // if you don't set a status code on JsonResults, you're saying 👌
                    return HttpStatusCode.OK;
                }

                throw new NotImplementedException(
                    $"StatusCode inference not implemented for action of type: {type}"
                );
            }

            if (actionResult is ForbidResult)
            {
                return HttpStatusCode.Forbidden;
            }

            if (actionResult is UnauthorizedResult)
            {
                return HttpStatusCode.Unauthorized;
            }

            throw new InvalidOperationException(
                $"{actionResult} ({type}) has no StatusCode property"
            );
        }

        return (HttpStatusCode) statusCode;
    }

}