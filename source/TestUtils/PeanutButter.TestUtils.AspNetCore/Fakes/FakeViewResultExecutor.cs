using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

#if BUILD_PEANUTBUTTER_INTERNAL
namespace Imported.PeanutButter.TestUtils.AspNetCore.Fakes;
#else
namespace PeanutButter.TestUtils.AspNetCore.Fakes;
#endif

/// <summary>
/// Actually rendering a view is going to be tricky, and probably
/// unnecessary. For testing purposes, the caller needs to be able
/// to assert the view name and the provided data. This fake renderer
/// will execute to arrive at:
/// 1. the response body will be the view name
/// 2. The http context on the action context will have the following Items set:
///    - Model
///    - ViewData
///    - ContentType
///    - StatusCode
/// </summary>
#if BUILD_PEANUTBUTTER_INTERNAL
internal
#else
public
#endif
    class FakeViewResultExecutor
    : IActionResultExecutor<PartialViewResult>,
      IActionResultExecutor<ViewResult>
{
    /// <inheritdoc />
    public async Task ExecuteAsync(
        ActionContext context,
        PartialViewResult result
    )
    {
        await Resolve(
            context,
            result.StatusCode,
            result.ContentType,
            result.Model,
            result.ViewName,
            result.ViewData
        );
    }

    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true
    };

    private async Task Resolve(
        ActionContext context,
        int? statusCode,
        string contentType,
        object model,
        string viewName,
        ViewDataDictionary viewData
    )
    {
        await context.HttpContext.Response.WriteAsync(
            $@"
Status: {statusCode ?? 200}
Content Type: {contentType ?? "text/html"}
View: {viewName}

Model:
{(model is null ? "(null)" : JsonSerializer.Serialize(model, Options))}

ViewData:
{(viewData is null ? "(null)" : JsonSerializer.Serialize(viewData))}
".Trim()
        );
        context.HttpContext.Items["Model"] = model;
        context.HttpContext.Items["ViewData"] = viewData;
        context.HttpContext.Response.StatusCode =
            statusCode ?? (int)HttpStatusCode.OK;
        context.HttpContext.Response.ContentType =
            contentType ?? "text/html";
    }

    /// <inheritdoc />
    public async Task ExecuteAsync(
        ActionContext context,
        ViewResult result
    )
    {
        await Resolve(
            context,
            result.StatusCode,
            result.ContentType,
            result.Model,
            result.ViewName,
            result.ViewData
        );
    }
}