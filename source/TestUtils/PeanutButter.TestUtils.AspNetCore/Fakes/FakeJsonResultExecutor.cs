using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
#else
namespace PeanutButter.TestUtils.AspNetCore.Fakes;
#endif

/// <summary>
/// 
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class FakeJsonResultExecutor
    : IActionResultExecutor<JsonResult>
{
    /// <inheritdoc />
    public async Task ExecuteAsync(
        ActionContext context,
        JsonResult result
    )
    {
        var json = result.Value is null
            ? ""
            : JsonSerializer.Serialize(result.Value);
        await context.HttpContext.Response.WriteAsync(
            json
        );
        context.HttpContext.Response.StatusCode =
            result.StatusCode ?? (int)HttpStatusCode.OK;
        context.HttpContext.Response.ContentType =
            result.ContentType ?? "application/json";
    }
}